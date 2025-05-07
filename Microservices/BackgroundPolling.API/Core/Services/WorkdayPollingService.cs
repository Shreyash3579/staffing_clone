using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.Models.Workday;
using BackgroundPolling.API.ViewModels;
using Hangfire;
using Microservices.Common.Core.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class WorkdayPollingService : IWorkdayPollingService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICcmApiClient _ccmApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IWorkdayPollingRepository _workdayPollingRepository;
        private readonly IWorkdayRedisConnectorAPIClient _workdayRedisConnectorApiClient;
        private readonly IPollMasterRepository _pollMasterRepostory;
        private readonly IBasisApiClient _basisApiClient;
        private readonly IMemoryCache _cache;

        public WorkdayPollingService(IResourceApiClient resourceApiClient,
            IWorkdayPollingRepository workdayPollingRepository, IMemoryCache cache,
            ICcmApiClient ccmApiClient, IWorkdayRedisConnectorAPIClient workdayRedisConnectorAPIClient,
            IBasisApiClient basisApiClient,
        IBackgroundJobClient backgroundJobClient, IPollMasterRepository pollMasterRepostory, IStaffingApiClient staffingApiClient)
        {
            _resourceApiClient = resourceApiClient;
            _workdayPollingRepository = workdayPollingRepository;
            _ccmApiClient = ccmApiClient;
            _workdayRedisConnectorApiClient = workdayRedisConnectorAPIClient;
            _backgroundJobClient = backgroundJobClient;
            _pollMasterRepostory = pollMasterRepostory;
            _staffingApiClient = staffingApiClient;
            _basisApiClient = basisApiClient;
            _cache = cache;
        }


        public async Task<IEnumerable<AnalyticsResourceTransactionViewModel>> UpdateAnalyitcsDataForPendingTransactions(DateTime? lastModifiedDateOnOrAfter)
        {
            var currentPstDateTime = DateTime.Now.Date.ConvertToPacificStandardTime();
            var pendingTransitionsTask = _resourceApiClient.GetFutureTransitions();
            var pendingPromotionsTask = _resourceApiClient.GetFuturePromotions();
            var pendingTransfersTask = _resourceApiClient.GetFutureTransfers();
            var pendingTerminationTask = _resourceApiClient.GetFutureTerminations();
            var pendingLoAsTask = _resourceApiClient.GetPendingLOATransactions();

            await Task.WhenAll(pendingTransitionsTask, pendingPromotionsTask, pendingTransfersTask, pendingLoAsTask);

            var pendingTransitions = pendingTransitionsTask.Result;
            var pendingPromotions = pendingPromotionsTask.Result;
            var pendingTransfers = pendingTransfersTask.Result;
            var pendingTermination = pendingTerminationTask.Result;
            var pendingLoAs = pendingLoAsTask.Result;


            var pendingPromotionsVm = ConvertToScheduleDetailMaster(pendingPromotions);
            var pendingTransfersVm = ConvertToScheduleDetailMaster(pendingTransfers);
            var pendingTransitionsVm = ConvertToScheduleDetailMaster(pendingTransitions);
            var pendingTerminationvm = ConvertToScheduleDetailMaster(pendingTermination);

            var pendingTransactions = ConvertToAnalyticsResourceTransaction(pendingPromotionsVm)
                .Concat(ConvertToAnalyticsResourceTransaction(pendingTransfersVm))
                .Concat(ConvertToAnalyticsResourceTransaction(pendingTransitionsVm))
                .Concat(ConvertToAnalyticsResourceTransaction(pendingTerminationvm))
                .Concat(ConvertToAnalyticsResourceTransaction(pendingLoAs));

            // Check for pending transactions updated recently to avoid update same records again and again
            var lastPolledTime = await _pollMasterRepostory.GetLastPolledTimeStampFromAnalyticsDB(Constants.WorkdayStaffingTransactionsToBeEffective);
            var pstDateTime = lastModifiedDateOnOrAfter.HasValue
                ? lastModifiedDateOnOrAfter.Value.ConvertToPacificStandardTime()
                : (lastPolledTime == DateTime.MinValue ? DateTime.Now.AddHours(-2).ConvertToPacificStandardTime() : lastPolledTime);

            pendingTransactions = pendingTransactions.Where(x => x.LastUpdated >= pstDateTime);

            if (pendingTransactions == null || !pendingTransactions.Any())
            {
                await _pollMasterRepostory.UpsertPollMasterOnAnalyticsDB(Constants.WorkdayStaffingTransactionsToBeEffective, currentPstDateTime);
                return Enumerable.Empty<AnalyticsResourceTransactionViewModel>();
            }

            var maxLastUpdatedDateTime = (DateTime)pendingTransactions.Max(x => x.LastUpdated);
            var resourceTransactionTable = GetAnalyticsResourceTransactionDTO(pendingTransactions);

            await _workdayPollingRepository.UpdateAnalyitcsDataForPendingTransactions(
                resourceTransactionTable);

            await _pollMasterRepostory.UpsertPollMasterOnAnalyticsDB(Constants.WorkdayStaffingTransactionsToBeEffective, maxLastUpdatedDateTime);

            var employeeCodes =
                string.Join(",", pendingTransactions.Select(x => x.EmployeeCode).Distinct());

            /*
             * Trigger job of updating resource availability data
             */
            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                x.UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));

            /*
             * Trigger job of updating cost for the pending transaction
             * Any future transaction likely to happen like promotion, transfer
             * requires scheudlemasterDetail table to be updated with correct cost
             */
            _backgroundJobClient.Enqueue(() => UpdateCostForAllocationsAnalyticsData());
            _backgroundJobClient.Enqueue(() => UpdateCostForPlaceholdersAnalyticsData());

            return pendingTransactions;
        }

        public async Task UpdateCostForAllocationsAnalyticsData()
        {

            var recordsWithoutCost = await _workdayPollingRepository.GetRecordsWithoutCostForAllocations();

            if (!recordsWithoutCost.Any())
            {
                return;
            }

            var employees = await _resourceApiClient.GetEmployeesIncludingTerminated();
            var recordsWithBillCode = GetRecordsWithCurrentInfoUpdated(recordsWithoutCost, employees);
            var recordsWithPointInTimeInfo = await GetRecordsWithPointInTimeInfo(recordsWithBillCode, employees);
            var recordsWithBillRate = await GetRecordsWithCostUpdated(recordsWithPointInTimeInfo);
            var recordsWithBillRateUpdatedByLoA = await GetRecordsWithLoAUpdated(recordsWithBillRate);
            var recordsWithCostUpdatedTable = CreateAnalyticsRecordsDTO(recordsWithBillRateUpdatedByLoA);
            await _workdayPollingRepository.UpdateRecordsWithoutCostForAllocations(recordsWithCostUpdatedTable);
            /*
            * Trigger job to update records in capacity analysis daily table
            * fot the changes happen in SMD and RA
            * */
            //_backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
            //     x.UpsertCapacityAnalysisDaily(false, null));

            return;
        }

        public async Task UpdateCostForPlaceholdersAnalyticsData()
        {

            var recordsWithoutCost = await _workdayPollingRepository.GetRecordsWithoutCostForPlaceholders();

            if (!recordsWithoutCost.Any()) return;

            var employees = await _resourceApiClient.GetEmployeesIncludingTerminated();
            var recordsWithBillCode = GetRecordsWithCurrentInfoUpdated(recordsWithoutCost, employees);
            var recordsWithPointInTimeInfo = await GetRecordsWithPointInTimeInfo(recordsWithBillCode, employees);
            var recordsWithBillRate = await GetRecordsWithCostUpdated(recordsWithPointInTimeInfo);
            var recordsWithBillRateUpdatedByLoA = await GetRecordsWithLoAUpdated(recordsWithBillRate);
            var recordsWithCostUpdatedTable = CreateAnalyticsRecordsDTO(recordsWithBillRateUpdatedByLoA);
            await _workdayPollingRepository.UpdateRecordsWithoutCostForPlaceholders(recordsWithCostUpdatedTable);
            /*
            * Trigger job to update records in capacity analysis daily table
            * fot the changes happen in SMD and RA
            * */
            //_backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
            //     x.UpsertCapacityAnalysisDaily(false, null));

            return;
        }


        public async Task<IEnumerable<LOATransaction>> UpdateAnalyticsDataForLoAUpdatedRecently(DateTime? lastModifiedDateOnOrAfter, DateTime? fromEffectiveDate)
        {
            var lastPolledTime = await _pollMasterRepostory.GetLastPolledTimeStampFromAnalyticsDB(Constants.LoAUpdatedRecentlyInworkday);
            var pstDateTime = lastModifiedDateOnOrAfter.HasValue
                ? lastModifiedDateOnOrAfter.Value.ConvertToPacificStandardTime()
                : (fromEffectiveDate.HasValue ? fromEffectiveDate.Value.ConvertToPacificStandardTime() : (lastPolledTime == DateTime.MinValue ? DateTime.Now.AddHours(-2).ConvertToPacificStandardTime() : lastPolledTime));


            lastPolledTime = DateTime.Now.Date.ConvertToPacificStandardTime();


            var loaTransactionsUpdatedRecently = fromEffectiveDate != null
                ? await _workdayRedisConnectorApiClient.GetEmployeesLOATransactionsByEfectiveDate(pstDateTime)
                : await _workdayRedisConnectorApiClient.GetEmployeesLOATransactionsByModifiedDate(pstDateTime);

            if (loaTransactionsUpdatedRecently.Count() == 0)
            {
                return Enumerable.Empty<LOATransaction>();
            }

            var transactions = await GetEmployeesLoATransactions(loaTransactionsUpdatedRecently);

            if (transactions == null || !transactions.Any())
            {
                await _pollMasterRepostory.UpsertPollMasterOnAnalyticsDB(Constants.LoAUpdatedRecentlyInworkday, lastPolledTime);
                return Enumerable.Empty<LOATransaction>();
            }

            var resourceTransactionTable =
                GetAnalyticsResourceTransactionDTO(ConvertToAnalyticsResourceTransaction(transactions));

            var maxLastModifiedDate = transactions.Max(x => x.LastModifiedDate).Value;

            await _workdayPollingRepository.UpdateAnalyticsDataForLoAUpdatedRecently(resourceTransactionTable);
            await _pollMasterRepostory.UpsertPollMasterOnAnalyticsDB(Constants.LoAUpdatedRecentlyInworkday, maxLastModifiedDate);
            var employeeCodes =
                string.Join(",", transactions.Select(x => x.EmployeeCode).Distinct());

            /*
             * Trigger job of updating cost for the resource available in full capacity
             * Resource avilability will be recalculated whenever there is change in scheudleMasterDetail Table
             */
            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                x.UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));

            return transactions;
        }

        public async Task InsertDailyAvailabilityTillNextYearForAll()
        {
            await Task.Run(() => _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                x.InsertDailyAvailabilityTillNextYearForAll(null)));
        }

        public async Task<List<ScheduleMaster>> UpdateServiceLineInScheduleMaster()
        {
            var eCodesWithoutServiceLineTask = _workdayPollingRepository.GetECodesWithoutServiceLine();
            var employeesTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            await Task.WhenAll(employeesTask, eCodesWithoutServiceLineTask);
            var employees = employeesTask.Result;
            var eCodesWithoutServiceLine = eCodesWithoutServiceLineTask.Result;
            if (eCodesWithoutServiceLine != null && eCodesWithoutServiceLine.Any())
            {
                var recordsWithUpdatedServiceLine = (from employee in employees
                                                     join eCode in eCodesWithoutServiceLine
                                                        on employee.EmployeeCode.Trim() equals eCode.Trim()
                                                     select new ScheduleMaster
                                                     {
                                                         EmployeeCode = eCode,
                                                         ServiceLineCode = employee.ServiceLine?.ServiceLineCode ?? "N/A",
                                                         ServiceLineName = employee.ServiceLine?.ServiceLineName ?? "N/A"
                                                     }).ToList();
                if (recordsWithUpdatedServiceLine.Count > 0)
                {
                    var recordsWithUpdatedServiceLineTable = ConvertToScheduleMasterDTO(recordsWithUpdatedServiceLine);
                    await _workdayPollingRepository.UpdateRecordsWithoutServiceLine(recordsWithUpdatedServiceLineTable);
                    await _workdayPollingRepository.UpdateAnalyticsRecordsWithoutServiceLine(recordsWithUpdatedServiceLineTable);
                }
                return recordsWithUpdatedServiceLine;
            }
            return new List<ScheduleMaster>();
        }

        public void UpdateCostForResourcesAvailableInFullCapacity(string employeeCodes = null)
        {
            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                x.UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));
        }

        public async Task<IEnumerable<ResourceLOA>> UpdateOverrideFlagForStaffingCommtimentsFromSource()
        {
            var futureCommitments = await _resourceApiClient.GetFutureLOAs();

            if (!futureCommitments.Any())
                return Enumerable.Empty<ResourceLOA>();

            var dto = CovertToCommitmentMasterDTO(futureCommitments);

            await _workdayPollingRepository.UpdateOverrideFlagForStaffingCommtimentsFromSource(dto);

            return futureCommitments;
        }

        public async Task<string> DeleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate()
        {
            var allEmployees = await _resourceApiClient.GetEmployeesIncludingTerminated();
            var terminatedEmployees = allEmployees.Where(x => x.ActiveStatus == "Terminated" && x.TerminationDate.HasValue).ToList();

            if (!terminatedEmployees.Any())
                return "No Data Deleted in this run";

            var resourcesTerminations = ConvertToResourceTransactionModel(terminatedEmployees);

            var dto = CovertToResourceTransactionDTO(resourcesTerminations);

            var affectedEmployeesForCommitments = await _workdayPollingRepository.DeleteCommitmentDataForTermedEmployeesAfterTerminationDate(dto);
            var affectedEmployeesForScheduleMaster = await _workdayPollingRepository.DeleteStaffingDataForTermedEmployeesAfterTerminationDate(dto);
            await _workdayPollingRepository.DeleteAnalyticsDataForTermedEmployeesAfterTerminationDate(dto);

            var deletedEmployees = RemoveTerminatedEmployeesFromSecurityUsers(terminatedEmployees);

            if (string.IsNullOrEmpty(affectedEmployeesForCommitments) && string.IsNullOrEmpty(affectedEmployeesForScheduleMaster))
            {
                return "No Data Deleted in this run";
            }

            StringBuilder str = new StringBuilder();
            str.AppendLine("Commitments Data Upserted for Following Employees");
            str.AppendLine(affectedEmployeesForCommitments);
            str.AppendLine("Staffing Data Upserted for Following Employees");
            str.AppendLine(affectedEmployeesForScheduleMaster);

            /*
            * Trigger job to update records in capacity analysis daily table
            * fot the changes happen in SMD and RA
            * */
            //_backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
            //    x.UpsertCapacityAnalysisDaily(false, null));

            return str.ToString();
        }

        public async Task<IEnumerable<Commitment>> SaveWorkdayLoaAndTransitionAsShortTermCommitment()
        {
            var loaTransactions = await
                   _workdayRedisConnectorApiClient.GetEmployeesLOATransactionsPendingFromRedis();

            var employeeTransactions = await
                   _workdayRedisConnectorApiClient.GetEmployeesStaffingTransactionsPendingFromRedis();

            var shortTermCommitments = new List<Commitment>();
            if (loaTransactions.Any())
            {
                var loasWithSuccessfullTransaction = loaTransactions.ToList().Where(x =>
                    x.TransactionStatus.Contains("Successfully Completed")
                    && x.Transaction != null
                    && x.Transaction.FirstDayOfLeave != DateTime.MinValue);

                shortTermCommitments = ConvertLOATransactionToCommitment(loasWithSuccessfullTransaction);
            }

            if (employeeTransactions.Any())
            {
                var employeeTransition = employeeTransactions
               .Where(pt =>
                   (pt.BusinessProcessType == "Termination" ||
                   pt.BusinessProcessType.StartsWith("Change Organization Assignments")) && pt.TransactionStatus == "Successfully Completed")
               .Where(t => t.Transaction.TransitionStartDate != null &&
                           (t.Transaction.CostCenterProposed.CostCenterId.StartsWith("0300_") &&
                            t.Transaction.TransitionStartDate.Value.Date >= DateTime.Today.Date));

                employeeTransition = employeeTransition.GroupBy(g => g.EmployeeCode).Select(g => g.FirstOrDefault());

                shortTermCommitments.AddRange(ConvertTransitionToCommitment(employeeTransition));

            }
            var shortTermCommitmentsDataTable = ConvertToCommitmentDTO(shortTermCommitments);

            await _workdayPollingRepository.CreateShortTermCommitmentsForWorkdayLOAsAndTransitions(shortTermCommitmentsDataTable);

            var commitmentIds = string.Join(",", shortTermCommitments.Select(x => x.Id).Distinct());
            /*
            * Trigger job to update commitment in SMD and RA
            */
            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                x.UpdateAnlayticsDataForUpsertedCommitment(commitmentIds));

            return shortTermCommitments;
        }



        public async Task<IEnumerable<ServiceLine>> SaveWorkdayServiceLineListForTableau()
        {
            var serviceLines = await _resourceApiClient.GetServiceLines();

            if (serviceLines.Any())
            {
                var serviceLineDataTable = ConvertToServiceLineDTO(serviceLines);

                var savedServiceLines = await _workdayPollingRepository.SaveWorkdayServiceLineListForTableau(serviceLineDataTable);
                return savedServiceLines;
            }
            return Enumerable.Empty<ServiceLine>();
        }

        public async Task<IEnumerable<PDGrade>> SaveWorkdayPDGradeListForTableau()
        {
            var pdGrades = await _resourceApiClient.GetPDGrades();

            if (pdGrades.Any())
            {
                var pdGradeDataTable = ConvertToPDGradeDTO(pdGrades);

                var savedPDGrades = await _workdayPollingRepository.SaveWorkdayPDGradeListForTableau(pdGradeDataTable);
                return savedPDGrades;
            }
            return Enumerable.Empty<PDGrade>();
        }

        public async Task<string> SaveWorkdayTimeOffsForTableau()
        {

            _cache.TryGetValue<int>("skipTimeOffRecordCount", out var skipRecordCount);

            var employeesHashMaps = await _resourceApiClient.GetEmployeeIdTypeMaps();
            var employeeCount = employeesHashMaps.Count();

            var employeeTimeOffCountToFetchFromWorkday =
                Convert.ToInt32(ConfigurationUtility.GetValue("EmployeeTimeOffCountToFetchFromWorkday"));

            var employeesIdTypeMapBatch = employeesHashMaps.ToList().OrderByDescending(e => e.Key).Skip(skipRecordCount)
                .Take(employeeTimeOffCountToFetchFromWorkday).ToList();

            if (!employeesIdTypeMapBatch.Any())
            {
                _cache.Set("skipTimeOffRecordCount", 0);
                return $"All Employees Time offs (Historical + Pending) saved to DB";
            }

            var employeeCodes = string.Join(",", employeesIdTypeMapBatch.Select(x => x.Key));
            var timeOffs = await _resourceApiClient.GetEmployeesTimeoffs(employeeCodes, null, null);
            var distinctTimeOffs = timeOffs.GroupBy(x => new { x.EmployeeCode, x.StartDate, x.EndDate, x.Status }).Select(grp => grp.FirstOrDefault());
            var dto = ConvertToTimeOffDTO(distinctTimeOffs);

            await _workdayPollingRepository.SaveWorkdayTimeOffsForTableau(dto);

            skipRecordCount += employeeTimeOffCountToFetchFromWorkday;
            _cache.Set("skipTimeOffRecordCount", skipRecordCount);

            // Self invocation of a function in order to save Employees trasnactions
            await SaveWorkdayTimeOffsForTableau();

            return $"Saving Employees Time offs (Historical + Pending) to DB is in progress....";
        }


        public async Task UpdateAnalyticsDataHavingIncorrectWorkdayInfo()
        {
            var recordsCorrectedForEmployees = await _workdayPollingRepository.UpdateAnalyticsDataHavingIncorrectWorkdayInfo();

            // Trigger job of updating data
            if (recordsCorrectedForEmployees.Any())
            {
                _backgroundJobClient.Enqueue(() => UpdateCostForAllocationsAnalyticsData());
            }

        }

        public async Task UpdateAnalyticsDataHavingIncorrectCaseInfo()
        {
            var recordsCorrectedForEmployees = await _workdayPollingRepository.UpdateAnalyticsDataHavingIncorrectCaseInfo();

            // Trigger job of updating data
            if (recordsCorrectedForEmployees.Any())
            {
                _backgroundJobClient.Enqueue(() => UpdateCostForAllocationsAnalyticsData());
            }

        }

        public async Task UpdateAnalyticsPlaceholdersDataHavingIncorrectWorkdayInfo()
        {
            var recordsCorrectedForEmployees = await _workdayPollingRepository.UpdateAnalyticsPlaceholderDataHavingIncorrectWorkdayInfo();

            // Trigger job of updating data
            if (recordsCorrectedForEmployees.Any())
            {
                _backgroundJobClient.Enqueue(() => UpdateCostForPlaceholdersAnalyticsData());
            }

        }

        public async Task UpdateAvailabilityDataHavingIncorrectWorkdayInfo()
        {
            var recordsCorrectedForEmployees = await _workdayPollingRepository.UpdateAvailabilityDataHavingIncorrectWorkdayInfo();
            if (recordsCorrectedForEmployees.Any())
            {
                var employeeCodes = string.Join(",", recordsCorrectedForEmployees);
                _backgroundJobClient.Enqueue(() => UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));
            }
        }

        public async Task UpdateAvailabilityDataForExternalCommitmentsAndRingfence()
        {
            var recordsCorrectedForEmployees = await _workdayPollingRepository.UpdateAvailabilityDataForExternalCommitmentsAndRingfence();
            if (recordsCorrectedForEmployees.Any())
            {
                var employeeCodes = string.Join(",", recordsCorrectedForEmployees);
                _backgroundJobClient.Enqueue(() => UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));
            }
        }

        public async Task UpdateAvailabilityDataForMissingOrIrrelevantEntries()
        {
            var recordsCorrectedForEmployees = await _workdayPollingRepository.UpdateAvailabilityDataForMissingOrIrrelevantEntries();
            if (recordsCorrectedForEmployees.Any())
            {
                var employeeCodes = string.Join(",", recordsCorrectedForEmployees);
                _backgroundJobClient.Enqueue(() => UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));
            }
        }
        public async Task<string> UpsertWorkdayEmployeeStaffingTransactionToDB()
        {
            var employeesHashMaps = await _resourceApiClient.GetEmployeeIdTypeMaps();
            var employeeTransactionCountToFetchFromWorkday =
                Convert.ToInt32(ConfigurationUtility.GetValue("EmployeeTransactionCountToFetchFromWorkday"));
            return await UpsertWorkdayEmployeeStaffingTransactionToDB(employeesHashMaps, employeeTransactionCountToFetchFromWorkday);
        }

        public async Task<string> UpsertWorkdayEmployeeLoATransactionToDB()
        {
            var employeesHashMaps = await _resourceApiClient.GetEmployeeIdTypeMaps();
            var employeeLoATransactionCountToFetchFromWorkday =
                Convert.ToInt32(ConfigurationUtility.GetValue("EmployeeLoATransactionCountToFetchFromWorkday"));

            return await UpsertWorkdayEmployeeLoATransactionToDB(employeesHashMaps, employeeLoATransactionCountToFetchFromWorkday);

        }

        public async Task<string> SaveWorkdayEmployeeDataForTableau()
        {
            var allResources = await _resourceApiClient.GetEmployeesIncludingTerminated();

            if (allResources.Any())
            {
                var resourcesDataTable = ConvertToResourceDTO(allResources);

                await _workdayPollingRepository.SaveWorkdayEmployeeDataForTableau(resourcesDataTable);
                return resourcesDataTable.Rows.Count + " Resources saved";
            }
            return "0 Resources saved";
        }

        public async Task<string> DeleteAvailabilityDataForRescindedEmployees(string employeeCodes)
        {
            var eCodesOfRescindedEmployees = await _workdayPollingRepository.DeleteAvailabilityDataForRescindedEmployees(employeeCodes);
            if (eCodesOfRescindedEmployees.Any())
            {
                var listEcodes = string.Join(",", eCodesOfRescindedEmployees);
                return "Following Employees rescinded :" + listEcodes;
            }

            /*
            * Trigger job to update records in capacity analysis daily table
            * fot the changes happen in SMD and RA
            * */
            //_backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
            //    x.UpsertCapacityAnalysisDaily(false, null));

            return "0 Employees Rescinded";
        }

        public async Task<string> ArchiveStaffableAsRoleForPromotedEmployees()
        {
            var employeesWithActiveStaffableAsRoles = await _workdayPollingRepository.GetAllActiveStaffableAsRoles();
            var activeEmployees = await _resourceApiClient.GetEmployees();

            var staffableAsToArchiveForPromotedEmployees = (from staffableRole in employeesWithActiveStaffableAsRoles
                                                            join employee in activeEmployees on staffableRole.EmployeeCode equals employee.EmployeeCode
                                                            where staffableRole.LevelGrade != employee.LevelGrade && !string.IsNullOrEmpty(employee.LevelGrade)
                                                            select staffableRole
                                );

            if (!staffableAsToArchiveForPromotedEmployees.Any())
            {
                return "No Role to Archive";
            }

            foreach (var data in staffableAsToArchiveForPromotedEmployees)
            {
                data.isActive = false;
                data.LastUpdatedBy = "Auto-PromotionApi";
            }

            var upsertedStaffableAs = await _staffingApiClient.UpsertResourcesStaffableAs(staffableAsToArchiveForPromotedEmployees);

            return "Following employees staffable as roles archived : " + string.Join(",", upsertedStaffableAs.Select(x => x.EmployeeCode));
        }

        public async Task UpdateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations()
        {
            await _workdayPollingRepository.UpdateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations();
        }

        public async Task<string> SaveWorkdayEmployeesCertificationsToDB()
        {
            var employeesHashMaps = await _resourceApiClient.GetEmployeeIdTypeMaps();
            var employeesCertificationsCountToFetchFromWorkday =
                Convert.ToInt32(ConfigurationUtility.GetValue("EmployeesCertificationsCountToFetchFromWorkday"));
            return await UpsertWorkdayEmployeesCertificationsToDB(employeesHashMaps, employeesCertificationsCountToFetchFromWorkday);
        }

        public async Task<string> SaveWorkdayEmployeesLanguagesToDB()
        {
            var employeesHashMaps = await _resourceApiClient.GetEmployeeIdTypeMaps();
            var employeesLanguagesCountToFetchFromWorkday =
                Convert.ToInt32(ConfigurationUtility.GetValue("EmployeesLanguagesCountToFetchFromWorkday"));
            return await UpsertWorkdayEmployeesLanguagesToDB(employeesHashMaps, employeesLanguagesCountToFetchFromWorkday);
        }



        #region Private Methods

        private async Task<string> UpsertWorkdayEmployeeStaffingTransactionToDB(Dictionary<string, string> employeesHashMaps,
            int employeeTransactionCountToFetchFromWorkday)
        {
            _cache.TryGetValue<int>("skipRecordCount", out var skipRecordCount);

            var employeesIdTypeMapBatch = employeesHashMaps.ToList().OrderByDescending(e => e.Key).Skip(skipRecordCount)
                .Take(employeeTransactionCountToFetchFromWorkday).ToList();

            if (!employeesIdTypeMapBatch.Any())
            {
                _cache.Set("skipRecordCount", 0);
                return $"All Employees Staffing Transactions (Historical + Pending) saved to DB";
            }

            var employeeCodes = string.Join(",", employeesIdTypeMapBatch.Select(x => x.Key.ToUpper()));
            var transactions = await _resourceApiClient.GetEmployeesStaffingTransactions(employeeCodes);

            var dto = convertWorkdayStaffingTransactionstoDto(transactions);

            await _workdayPollingRepository.UpsertWorkdayEmployeeStaffingTransactionToDB(dto);

            skipRecordCount += employeeTransactionCountToFetchFromWorkday;
            _cache.Set("skipRecordCount", skipRecordCount);

            // Self invocation of a function in order to save Employees trasnactions
            await UpsertWorkdayEmployeeStaffingTransactionToDB(employeesHashMaps, employeeTransactionCountToFetchFromWorkday);

            return $"Saving Employees Staffing Transactions (Historical + Pending) to DB is in progress....";
        }

        private async Task<string> UpsertWorkdayEmployeesCertificationsToDB(Dictionary<string, string> employeesHashMaps,
            int employeesCertificationsCountToFetchFromWorkday)
        {
            _cache.TryGetValue<int>("skipRecordCount", out var skipRecordCount);

            var employeesIdTypeMapBatch = employeesHashMaps.ToList().OrderByDescending(e => e.Key).Skip(skipRecordCount)
                .Take(employeesCertificationsCountToFetchFromWorkday).ToList();

            if (!employeesIdTypeMapBatch.Any())
            {
                _cache.Set("skipRecordCount", 0);
                return $"All Employees Certifications saved to DB";
            }

            var employeeCodes = string.Join(",", employeesIdTypeMapBatch.Select(x => x.Key));
            var employeesCertificates = await _resourceApiClient.GetCertificatesByEmployeeCodes(employeeCodes);

            var dto = ConvertWorkdayEmployeesCertificationsToDto(employeesCertificates);

            await _workdayPollingRepository.UpsertWorkdayEmployeesCertificatesToDB(dto);

            skipRecordCount += employeesCertificationsCountToFetchFromWorkday;
            _cache.Set("skipRecordCount", skipRecordCount);

            // Self invocation of a function in order to save Employees trasnactions
            await UpsertWorkdayEmployeesCertificationsToDB(employeesHashMaps, employeesCertificationsCountToFetchFromWorkday);

            return $"Saving Employees Certifications to DB is in progress....";
        }

        private async Task<string> UpsertWorkdayEmployeesLanguagesToDB(Dictionary<string, string> employeesHashMaps,
            int employeesLanguagesCountToFetchFromWorkday)
        {
            _cache.TryGetValue<int>("skipRecordCount", out var skipRecordCount);

            var employeesIdTypeMapBatch = employeesHashMaps.ToList().OrderByDescending(e => e.Key).Skip(skipRecordCount)
                .Take(employeesLanguagesCountToFetchFromWorkday).ToList();

            if (!employeesIdTypeMapBatch.Any())
            {
                _cache.Set("skipRecordCount", 0);
                return $"All Employees Languages saved to DB";
            }

            var employeeCodes = string.Join(",", employeesIdTypeMapBatch.Select(x => x.Key));
            var employeeslanguages = await _resourceApiClient.GetLanguagesByEmployeeCodes(employeeCodes);

            var dto = ConvertWorkdayEmployeesLanguagesToDto(employeeslanguages);

            await _workdayPollingRepository.UpsertWorkdayEmployeesLanguagesToDB(dto);

            skipRecordCount += employeesLanguagesCountToFetchFromWorkday;
            _cache.Set("skipRecordCount", skipRecordCount);

            // Self invocation of a function in order to save Employees trasnactions
            await UpsertWorkdayEmployeesLanguagesToDB(employeesHashMaps, employeesLanguagesCountToFetchFromWorkday);

            return $"Saving Employees Certifications to DB is in progress....";
        }

        private async Task<string> UpsertWorkdayEmployeeLoATransactionToDB(Dictionary<string, string> employeesHashMaps,
            int employeeLoATransactionCountToFetchFromWorkday)
        {
            _cache.TryGetValue<int>("skipLoARecordCount", out var skipRecordCount);


            var employeesIdTypeMapBatch = employeesHashMaps.ToList().OrderByDescending(e => e.Key).Skip(skipRecordCount)
                .Take(employeeLoATransactionCountToFetchFromWorkday).ToList();

            if (!employeesIdTypeMapBatch.Any())
            {
                _cache.Set("skipLoARecordCount", 0);
                return $"All Employees LOA Transactions (Historical + Pending) saved to DB";
            }

            var employeeCodes = string.Join(",", employeesIdTypeMapBatch.Select(x => x.Key));
            var LoAtransactions = await _resourceApiClient.GetWDEmployeesLoATransactions(employeeCodes.ToUpper());

            var dto = convertWorkdayLoATransactionstoDto(LoAtransactions);

            await _workdayPollingRepository.UpsertWorkdayEmployeeLoATransactionToDB(dto);

            skipRecordCount += employeeLoATransactionCountToFetchFromWorkday;
            _cache.Set("skipLoARecordCount", skipRecordCount);

            // Self invocation of a function in order to save Employees trasnactions
            await UpsertWorkdayEmployeeLoATransactionToDB(employeesHashMaps, employeeLoATransactionCountToFetchFromWorkday);

            return $"Saving Employees LOA Transactions (Historical + Pending) to DB is in progress....";
        }

        private DataTable ConvertWorkdayEmployeesCertificationsToDto(IEnumerable<EmployeeCertificates> employeesCertificates)
        {
            var employeesCertificationDataTable = new DataTable();
            employeesCertificationDataTable.Columns.Add("employeeCode", typeof(string));
            employeesCertificationDataTable.Columns.Add("issuedDate", typeof(DateTime));
            employeesCertificationDataTable.Columns.Add("name", typeof(string));
            employeesCertificationDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var employeeCertificates in employeesCertificates)
            {
                if (employeeCertificates.Certifications != null && employeeCertificates.Certifications.Count > 0)
                {
                    var employeeDistinctCertifications = employeeCertificates.Certifications.GroupBy(g => new { g.Name }).Select(grp => new Certification
                    {
                        Name = grp.Key.Name,
                        IssuedDate = grp.FirstOrDefault().IssuedDate
                    });

                    foreach (var certificate in employeeDistinctCertifications)
                    {
                        var row = employeesCertificationDataTable.NewRow();

                        row["employeeCode"] = employeeCertificates.EmployeeCode;
                        row["issuedDate"] = (object)certificate?.IssuedDate ?? DBNull.Value;
                        row["name"] = certificate.Name;
                        row["lastUpdatedBy"] = "Auto-WorkdayCertifications";

                        employeesCertificationDataTable.Rows.Add(row);
                    }
                }
            }

            return employeesCertificationDataTable;
        }

        private DataTable ConvertWorkdayEmployeesLanguagesToDto(IEnumerable<EmployeeLanguages> employeesLanguages)
        {
            var employeesLanguagesDataTable = new DataTable();
            employeesLanguagesDataTable.Columns.Add("employeeCode", typeof(string));
            employeesLanguagesDataTable.Columns.Add("name", typeof(string));
            employeesLanguagesDataTable.Columns.Add("proficiencyCode", typeof(int));
            employeesLanguagesDataTable.Columns.Add("proficiencyName", typeof(string));
            employeesLanguagesDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var employeeLanguages in employeesLanguages)
            {
                if (employeeLanguages.Languages != null && employeeLanguages.Languages.Count > 0)
                {
                    var employeeDistinctLanguages = employeeLanguages.Languages.GroupBy(g => new { g.Name }).Select(grp => new Language
                    {
                        Name = grp.Key.Name,
                        ProficiencyCode = grp.FirstOrDefault().ProficiencyCode,
                        ProficiencyName = grp.FirstOrDefault().ProficiencyName
                    });
                    foreach (var language in employeeDistinctLanguages)
                    {
                        var row = employeesLanguagesDataTable.NewRow();

                        row["employeeCode"] = employeeLanguages.EmployeeCode;
                        row["name"] = language.Name;
                        row["proficiencyCode"] = (object)language.ProficiencyCode ?? DBNull.Value; ;
                        row["proficiencyName"] = (object)language.ProficiencyName ?? DBNull.Value;
                        row["lastUpdatedBy"] = "Auto-WorkdayLanguages";

                        employeesLanguagesDataTable.Rows.Add(row);
                    }
                }
            }

            return employeesLanguagesDataTable;
        }

        private DataTable convertWorkdayStaffingTransactionstoDto(IEnumerable<EmployeeTransaction> transactions)
        {
            var employeeStaffingTransactionDataTable = new DataTable();
            employeeStaffingTransactionDataTable.Columns.Add("id", typeof(Guid));
            employeeStaffingTransactionDataTable.Columns.Add("businessProcessEvent", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("businessProcessReason", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("businessProcessType", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("businessProcessName", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("completedDate", typeof(DateTime));
            employeeStaffingTransactionDataTable.Columns.Add("effectiveDate", typeof(DateTime));
            employeeStaffingTransactionDataTable.Columns.Add("lastModifiedDate", typeof(DateTime));
            employeeStaffingTransactionDataTable.Columns.Add("mostRecentCorrectionDate", typeof(DateTime));
            employeeStaffingTransactionDataTable.Columns.Add("terminationEffectiveDate", typeof(DateTime));
            employeeStaffingTransactionDataTable.Columns.Add("employeeCode", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("employeeStatus", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("transactionStatus", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("billCodeCurrent", typeof(decimal));
            employeeStaffingTransactionDataTable.Columns.Add("billCodeProposed", typeof(decimal));
            employeeStaffingTransactionDataTable.Columns.Add("fteCurrent", typeof(decimal));
            employeeStaffingTransactionDataTable.Columns.Add("fteProposed", typeof(decimal));
            employeeStaffingTransactionDataTable.Columns.Add("homeOfficeCodeCurrent", typeof(Int16));
            employeeStaffingTransactionDataTable.Columns.Add("homeOfficeCodeProposed", typeof(Int16));
            employeeStaffingTransactionDataTable.Columns.Add("operatingOfficeCodeCurrent", typeof(Int16));
            employeeStaffingTransactionDataTable.Columns.Add("operatingOfficeCodeProposed", typeof(Int16));
            employeeStaffingTransactionDataTable.Columns.Add("pdGradeCurrent", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("pdGradeProposed", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("positionNameCurrent", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("positionNameProposed", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("positionGroupNameCurrent", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("positionGroupNameProposed", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("serviceLineCodeCurrent", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("serviceLineCodeProposed", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("serviceLineNameCurrent", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("serviceLineNameProposed", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("transitionStartDate", typeof(DateTime));
            employeeStaffingTransactionDataTable.Columns.Add("transitionEndDate", typeof(DateTime));
            employeeStaffingTransactionDataTable.Columns.Add("costCentreIdCurrent", typeof(string));
            employeeStaffingTransactionDataTable.Columns.Add("costCentreIdProposed", typeof(string));

            foreach (var transaction in transactions)
            {
                var row = employeeStaffingTransactionDataTable.NewRow();

                row["id"] = (object)transaction.Id ?? DBNull.Value;
                row["businessProcessEvent"] = (object)transaction.BusinessProcessEvent ?? DBNull.Value;
                row["businessProcessReason"] = (object)transaction.BusinessProcessReason ?? DBNull.Value;
                row["businessProcessType"] = (object)transaction.BusinessProcessType ?? DBNull.Value;
                row["businessProcessName"] = (object)transaction.BusinessProcessName ?? DBNull.Value;
                row["completedDate"] = (object)transaction.CompletedDate ?? DBNull.Value;
                row["effectiveDate"] = (object)transaction.EffectiveDate ?? DBNull.Value;
                row["lastModifiedDate"] = (object)transaction.LastModifiedDate ?? DBNull.Value;
                row["mostRecentCorrectionDate"] = (object)transaction.MostRecentCorrectionDate ?? DBNull.Value;
                row["terminationEffectiveDate"] = (object)transaction.TerminationEffectiveDate ?? DBNull.Value;
                row["employeeCode"] = (object)transaction.EmployeeCode ?? DBNull.Value;
                row["employeeStatus"] = (object)transaction.EmployeeStatus ?? DBNull.Value;
                row["transactionStatus"] = (object)transaction.TransactionStatus ?? DBNull.Value;
                row["billCodeCurrent"] = (object)transaction.Transaction?.BillCodeCurrent ?? DBNull.Value;
                row["billCodeProposed"] = (object)transaction.Transaction?.BillCodeProposed ?? DBNull.Value;
                row["fteCurrent"] = (object)transaction.Transaction?.FteCurrent ?? DBNull.Value; row["id"] = (object)transaction.Id ?? DBNull.Value;
                row["fteProposed"] = (object)transaction.Transaction?.FteProposed ?? DBNull.Value;
                row["homeOfficeCodeCurrent"] = (object)transaction.Transaction?.HomeOfficeCurrent?.OfficeCode ?? DBNull.Value;
                row["homeOfficeCodeProposed"] = (object)transaction.Transaction?.HomeOfficeProposed?.OfficeCode ?? DBNull.Value;
                row["operatingOfficeCodeCurrent"] = (object)transaction.Transaction?.SchedulingOfficeCurrent?.OfficeCode ?? DBNull.Value;
                row["operatingOfficeCodeProposed"] = (object)transaction.Transaction?.SchedulingOfficeProposed?.OfficeCode ?? DBNull.Value;
                row["pdGradeCurrent"] = (object)transaction.Transaction?.PdGradeCurrent ?? DBNull.Value;
                row["pdGradeProposed"] = (object)transaction.Transaction?.PdGradeProposed ?? DBNull.Value;
                row["positionNameCurrent"] = (object)transaction.Transaction?.PositionCurrent?.PositionName ?? DBNull.Value;
                row["positionNameProposed"] = (object)transaction.Transaction?.PositionProposed?.PositionName ?? DBNull.Value;
                row["positionGroupNameCurrent"] = (object)transaction.Transaction?.PositionCurrent?.PositionGroupName ?? DBNull.Value;
                row["positionGroupNameProposed"] = (object)transaction.Transaction.PositionProposed?.PositionGroupName ?? DBNull.Value;
                row["serviceLineCodeCurrent"] = (object)transaction.Transaction?.ServiceLineCurrent?.ServiceLineId ?? DBNull.Value;
                row["serviceLineCodeProposed"] = (object)transaction.Transaction?.ServiceLineProposed?.ServiceLineId ?? DBNull.Value;
                row["serviceLineNameCurrent"] = (object)transaction.Transaction?.ServiceLineCurrent?.ServiceLineName ?? DBNull.Value;
                row["serviceLineNameProposed"] = (object)transaction.Transaction?.ServiceLineProposed?.ServiceLineName ?? DBNull.Value;
                row["transitionStartDate"] = (object)transaction.Transaction?.TransitionStartDate ?? DBNull.Value;
                row["transitionEndDate"] = (object)transaction.Transaction?.TransitionEndDate ?? DBNull.Value;
                row["costCentreIdCurrent"] = (object)transaction.Transaction?.CostCenterCurrent?.CostCenterId ?? DBNull.Value;
                row["costCentreIdProposed"] = (object)transaction.Transaction?.CostCenterProposed?.CostCenterId ?? DBNull.Value;

                employeeStaffingTransactionDataTable.Rows.Add(row);
            }

            return employeeStaffingTransactionDataTable;
        }
        private DataTable convertWorkdayLoATransactionstoDto(IEnumerable<EmployeeLoATransaction> transactions)
        {
            var employeeLoATransactionDataTable = new DataTable();
            employeeLoATransactionDataTable.Columns.Add("id", typeof(Guid));
            employeeLoATransactionDataTable.Columns.Add("businessProcessEvent", typeof(string));
            employeeLoATransactionDataTable.Columns.Add("businessProcessReason", typeof(string));
            employeeLoATransactionDataTable.Columns.Add("businessProcessType", typeof(string));
            employeeLoATransactionDataTable.Columns.Add("completedDate", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("effectiveDate", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("lastModifiedDate", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("mostRecentCorrectionDate", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("terminationStatusEffectiveDate", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("employeeCode", typeof(string));
            employeeLoATransactionDataTable.Columns.Add("employeeName", typeof(string));
            employeeLoATransactionDataTable.Columns.Add("employeeStatus", typeof(string));
            employeeLoATransactionDataTable.Columns.Add("transactionStatus", typeof(string));
            employeeLoATransactionDataTable.Columns.Add("lastDayOfWork", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("firstDayOfLeave", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("estimatedLastDayOfLeave", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("actualLastDayOfLeave", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("firstDayBackAtWork", typeof(DateTime));
            employeeLoATransactionDataTable.Columns.Add("firstDayAtWork", typeof(DateTime));

            foreach (var transaction in transactions)
            {
                var row = employeeLoATransactionDataTable.NewRow();

                row["id"] = (object)transaction.Id ?? DBNull.Value;
                row["businessProcessEvent"] = (object)transaction.BusinessProcessEvent ?? DBNull.Value;
                row["businessProcessReason"] = (object)transaction.BusinessProcessReason ?? DBNull.Value;
                row["businessProcessType"] = (object)transaction.BusinessProcessType ?? DBNull.Value;
                row["completedDate"] = (object)transaction.CompletedDate ?? DBNull.Value;
                row["effectiveDate"] = (object)transaction.EffectiveDate ?? DBNull.Value;
                row["lastModifiedDate"] = (object)transaction.LastModifiedDate ?? DBNull.Value;
                row["mostRecentCorrectionDate"] = (object)transaction.MostRecentCorrectionDate ?? DBNull.Value;
                row["terminationStatusEffectiveDate"] = (object)transaction.TerminationStatusEffectiveDate ?? DBNull.Value;
                row["employeeCode"] = (object)transaction.EmployeeCode ?? DBNull.Value;
                row["employeeName"] = (object)transaction.EmployeeName ?? DBNull.Value;
                row["employeeStatus"] = (object)transaction.EmployeeStatus ?? DBNull.Value;
                row["transactionStatus"] = (object)transaction.TransactionStatus ?? DBNull.Value;
                row["lastDayOfWork"] = (object)transaction.Transaction?.LastDayOfWork ?? DBNull.Value;
                row["firstDayOfLeave"] = (object)transaction.Transaction?.FirstDayOfLeave ?? DBNull.Value;
                row["estimatedLastDayOfLeave"] = (object)transaction.Transaction?.EstimatedLastDayOfLeave ?? DBNull.Value;
                row["actualLastDayOfLeave"] = (object)transaction.Transaction?.ActualLastDayOfLeave ?? DBNull.Value;
                row["firstDayBackAtWork"] = (object)transaction.Transaction?.FirstDayBackAtWork ?? DBNull.Value;
                row["firstDayAtWork"] = (object)transaction.Transaction?.FirstDayAtWork ?? DBNull.Value;

                employeeLoATransactionDataTable.Rows.Add(row);
            }

            return employeeLoATransactionDataTable;
        }
        private async Task<IEnumerable<LOATransaction>> GetEmployeesLoATransactions(IEnumerable<LOATransaction> transactions)
        {
            var leaveRequests = transactions.Where(p => p.Transaction != null).ToList();

            var leaveReturnRequests = transactions.Where(p => p.Transaction == null);

            if (leaveReturnRequests != null && leaveReturnRequests.Any())
            {
                var employeeCodes = string.Join(",", leaveReturnRequests.Select(x => x.EmployeeCode).Distinct());

                var employeesLoAs = await _workdayRedisConnectorApiClient.GetEmployeesLOATransactions(employeeCodes);

                var employeesLoARequests = employeesLoAs.Where(p => p.Transaction != null).ToList();

                leaveRequests.AddRange(employeesLoARequests);
            }

            leaveRequests = leaveRequests.GroupBy(x => x.Id).Select(grp => grp.FirstOrDefault()).ToList();

            return leaveRequests;
        }

        private DataTable ConvertToCommitmentDTO(IEnumerable<Commitment> commitments)
        {
            var commitmentDataTable = new DataTable();
            commitmentDataTable.Columns.Add("id", typeof(Guid));
            commitmentDataTable.Columns.Add("employeeCode", typeof(string));
            commitmentDataTable.Columns.Add("commitmentTypeCode", typeof(string));
            commitmentDataTable.Columns.Add("commitmentTypeReasonCode", typeof(string)); // <-- Add this column
            commitmentDataTable.Columns.Add("startDate", typeof(DateTime));
            commitmentDataTable.Columns.Add("endDate", typeof(DateTime));
            commitmentDataTable.Columns.Add("allocation", typeof(Int16));
            commitmentDataTable.Columns.Add("notes", typeof(string));
            commitmentDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var commitment in commitments)
            {
                var row = commitmentDataTable.NewRow();

                row["id"] = (object)commitment.Id ?? DBNull.Value;
                row["employeeCode"] = (object)commitment.EmployeeCode ?? DBNull.Value;
                row["commitmentTypeCode"] = (object)commitment.CommitmentType.CommitmentTypeCode ?? DBNull.Value;
                row["commitmentTypeReasonCode"] = (object)commitment.CommitmentType.CommitmentTypeReasonCode ?? DBNull.Value; // <-- Populate this column
                row["startDate"] = commitment.StartDate;
                row["endDate"] = commitment.EndDate;
                row["allocation"] = (object)commitment.Allocation ?? DBNull.Value;
                row["notes"] = (object)commitment.Notes ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)commitment.LastUpdatedBy ?? DBNull.Value;

                commitmentDataTable.Rows.Add(row);
            }

            return commitmentDataTable;
        }


        private DataTable ConvertToServiceLineDTO(IEnumerable<ServiceLine> serviceLines)
        {
            var serviceLineDataTable = new DataTable();
            serviceLineDataTable.Columns.Add("ServiceLineId", typeof(string));
            serviceLineDataTable.Columns.Add("ServiceLineHierarchyCode", typeof(string));
            serviceLineDataTable.Columns.Add("ServiceLineHierarchyName", typeof(string));
            serviceLineDataTable.Columns.Add("ServiceLineCode", typeof(string));
            serviceLineDataTable.Columns.Add("ServiceLineName", typeof(string));
            serviceLineDataTable.Columns.Add("employeeCount", typeof(short));
            serviceLineDataTable.Columns.Add("inActive", typeof(bool));
            serviceLineDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var serviceLine in serviceLines)
            {
                var row = serviceLineDataTable.NewRow();
                row["ServiceLineId"] = (object)serviceLine.ServiceLineId ?? DBNull.Value;
                row["ServiceLineHierarchyCode"] = (object)serviceLine.ServiceLineHierarchyCode ?? DBNull.Value;
                row["ServiceLineHierarchyName"] = (object)serviceLine.ServiceLineHierarchyName ?? DBNull.Value;
                row["ServiceLineCode"] = (object)serviceLine.ServiceLineCode ?? DBNull.Value;
                row["ServiceLineName"] = (object)serviceLine.ServiceLineName ?? DBNull.Value;
                row["employeeCount"] = (object)serviceLine.EmployeeCount ?? DBNull.Value;
                row["inActive"] = (object)serviceLine.InActive ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-WDServiceLine";

                serviceLineDataTable.Rows.Add(row);
            }

            return serviceLineDataTable;
        }
        private DataTable ConvertToPDGradeDTO(IEnumerable<PDGrade> pdGrades)
        {
            var pdGradeDataTable = new DataTable();
            pdGradeDataTable.Columns.Add("pdGradeCode", typeof(string));
            pdGradeDataTable.Columns.Add("pdGradeName", typeof(string));
            pdGradeDataTable.Columns.Add("description", typeof(string));
            pdGradeDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var pdGrade in pdGrades)
            {
                var row = pdGradeDataTable.NewRow();
                row["pdGradeCode"] = (object)pdGrade.PDGradeCode ?? DBNull.Value;
                row["pdGradeName"] = (object)pdGrade.PDGradeName ?? DBNull.Value;
                row["description"] = (object)pdGrade.Description ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-WDPDGrade";

                pdGradeDataTable.Rows.Add(row);
            }

            return pdGradeDataTable;
        }

        private DataTable ConvertToResourceDTO(IEnumerable<Resource> resources)
        {

            var resourceDataTable = new DataTable();
            resourceDataTable.Columns.Add("employeeCode", typeof(string));
            resourceDataTable.Columns.Add("employeeType", typeof(string));
            resourceDataTable.Columns.Add("firstName", typeof(string));
            resourceDataTable.Columns.Add("lastName", typeof(string));
            resourceDataTable.Columns.Add("familiarFirstName", typeof(string));
            resourceDataTable.Columns.Add("familiarLastName", typeof(string));
            resourceDataTable.Columns.Add("fullName", typeof(string));
            resourceDataTable.Columns.Add("fte", typeof(decimal));
            resourceDataTable.Columns.Add("levelGrade", typeof(string));
            resourceDataTable.Columns.Add("levelName", typeof(string));
            resourceDataTable.Columns.Add("mentorEcode", typeof(string));
            resourceDataTable.Columns.Add("mentorName", typeof(string));
            resourceDataTable.Columns.Add("serviceLineCode", typeof(string));
            resourceDataTable.Columns.Add("serviceLineName", typeof(string));
            resourceDataTable.Columns.Add("serviceLineHierarchyCode", typeof(string));
            resourceDataTable.Columns.Add("serviceLineHierarchyName", typeof(string));
            resourceDataTable.Columns.Add("positionCode", typeof(string));
            resourceDataTable.Columns.Add("positionName", typeof(string));
            resourceDataTable.Columns.Add("positionGroupName", typeof(string));
            resourceDataTable.Columns.Add("billCode", typeof(decimal));
            resourceDataTable.Columns.Add("Status", typeof(bool));
            resourceDataTable.Columns.Add("ActiveStatus", typeof(string));
            resourceDataTable.Columns.Add("InternetAddress", typeof(string));
            resourceDataTable.Columns.Add("startDate", typeof(DateTime));
            resourceDataTable.Columns.Add("hireDate", typeof(DateTime));
            resourceDataTable.Columns.Add("terminationDate", typeof(DateTime));
            resourceDataTable.Columns.Add("homeOfficeCode", typeof(short));
            resourceDataTable.Columns.Add("homeOfficeName", typeof(string));
            resourceDataTable.Columns.Add("homeOfficeAbbreviation", typeof(string));
            resourceDataTable.Columns.Add("operatingOfficeCode", typeof(int));
            resourceDataTable.Columns.Add("operatingOfficeName", typeof(string));
            resourceDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            resourceDataTable.Columns.Add("departmentCode", typeof(string));
            resourceDataTable.Columns.Add("departmentName", typeof(string));
            resourceDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var resource in resources)
            {
                var row = resourceDataTable.NewRow();
                row["employeeCode"] = resource.EmployeeCode;
                row["employeeType"] = resource.EmployeeType;
                row["firstName"] = resource.FirstName;
                row["lastName"] = resource.LastName;
                row["familiarFirstName"] = (object)resource.FamiliarFirstName ?? DBNull.Value;
                row["familiarLastName"] = (object)resource.FamiliarLastName ?? DBNull.Value;
                row["fullName"] = resource.FullName;
                row["fte"] = resource.Fte;
                row["levelGrade"] = (object)resource.LevelGrade ?? DBNull.Value;
                row["levelName"] = (object)resource.LevelName ?? DBNull.Value;
                row["mentorEcode"] = (object)resource.MentorEcode ?? DBNull.Value;
                row["mentorName"] = (object)resource.MentorName ?? DBNull.Value;
                row["serviceLineCode"] = (object)resource.ServiceLine?.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)resource.ServiceLine?.ServiceLineName ?? DBNull.Value;
                row["serviceLineHierarchyCode"] = (object)resource.ServiceLine?.ServiceLineHierarchyCode ?? DBNull.Value;
                row["serviceLineHierarchyName"] = (object)resource.ServiceLine?.ServiceLineHierarchyName ?? DBNull.Value;
                row["positionCode"] = resource.Position.PositionCode;
                row["positionName"] = resource.Position.PositionName;
                row["positionGroupName"] = resource.Position.PositionGroupName;
                row["billCode"] = resource.BillCode;
                row["status"] = resource.Status;
                row["activeStatus"] = resource.ActiveStatus;
                row["internetAddress"] = (object)resource.InternetAddress ?? DBNull.Value;
                row["startDate"] = resource.StartDate;
                row["hireDate"] = resource.HireDate;
                row["terminationDate"] = (object)resource.TerminationDate ?? DBNull.Value;
                row["homeOfficeCode"] = resource.Office.OfficeCode;
                row["homeOfficeName"] = resource.Office.OfficeName;
                row["homeOfficeAbbreviation"] = resource.Office.OfficeAbbreviation;
                row["operatingOfficeCode"] = (object)resource.SchedulingOffice?.OfficeCode ?? DBNull.Value;
                row["operatingOfficeName"] = (object)resource.SchedulingOffice?.OfficeName ?? DBNull.Value;
                row["operatingOfficeAbbreviation"] = (object)resource.SchedulingOffice?.OfficeAbbreviation ?? DBNull.Value;
                row["departmentCode"] = (object)resource.Department?.DepartmentCode ?? DBNull.Value;
                row["departmentName"] = (object)resource.Department?.DepartmentName ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-WDEmployee";

                resourceDataTable.Rows.Add(row);
            }

            return resourceDataTable;
        }

        private DataTable ConvertToTimeOffDTO(IEnumerable<ResourceTimeOff> timeOffs)
        {
            var timeOffDataTable = new DataTable();
            timeOffDataTable.Columns.Add("employeeCode", typeof(string));
            timeOffDataTable.Columns.Add("startDate", typeof(DateTime));
            timeOffDataTable.Columns.Add("endDate", typeof(DateTime));
            timeOffDataTable.Columns.Add("status", typeof(string));
            timeOffDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            timeOffDataTable.Columns.Add("approvedBy", typeof(string));

            foreach (var timeoff in timeOffs)
            {
                var row = timeOffDataTable.NewRow();
                row["employeeCode"] = (object)timeoff.EmployeeCode ?? DBNull.Value;
                row["startDate"] = (object)timeoff.StartDate ?? DBNull.Value;
                row["endDate"] = (object)timeoff.EndDate ?? DBNull.Value;
                row["status"] = (object)timeoff.Status ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-WorkdayTimeoff";
                row["approvedBy"] = (object)timeoff.ApprovedBy != null ? (object)timeoff.ApprovedBy[0] : DBNull.Value;
                timeOffDataTable.Rows.Add(row);
            }

            return timeOffDataTable;
        }
        private List<Commitment> ConvertLOATransactionToCommitment(IEnumerable<LOATransaction> loaTransactions)
        {
            if (loaTransactions.ToList().Count < 1)
                return new List<Commitment>();

            var commitment = loaTransactions.Select(item => new Commitment
            {
                Id = Guid.Parse(item.Id.Trim()),
                EmployeeCode = item.EmployeeCode,
                CommitmentType = new CommitmentType()
                {
                    CommitmentTypeCode = "ST"
                },
                //ShortTerm Commitment will start 3 weeks before the LOA
                StartDate = GetDateForShortTermCommitment("Start Date", item.Transaction),
                EndDate = GetDateForShortTermCommitment("End Date", item.Transaction),
                Notes = "",
                LastUpdatedBy = "Auto-LoaToST"
            });

            return commitment.ToList();
        }
        private List<Commitment> ConvertTransitionToCommitment(IEnumerable<EmployeeTransaction> transitions)
        {
            if (transitions.ToList().Count < 1)
                return new List<Commitment>();

            var commitment = transitions.Select(item => new Commitment
            {
                Id = Guid.Parse(item.Id.Trim()),
                EmployeeCode = item.EmployeeCode,
                CommitmentType = new CommitmentType()
                {
                    CommitmentTypeCode = "ST"
                },
                //ShortTerm Commitment will start 3 weeks before the Transition
                StartDate = item.Transaction.TransitionStartDate.Value.AddDays(-22).Date,
                EndDate = item.Transaction.TransitionStartDate.Value.AddDays(-1).Date,
                Notes = "",
                LastUpdatedBy = "Auto-TransitionToST"
            });

            return commitment.ToList();
        }
        private DateTime GetDateForShortTermCommitment(string column, LOATransactionProcess transaction)
        {
            if (column == "Start Date")
            {
                var startDate = Convert.ToDateTime(transaction.FirstDayOfLeave.AddDays(-22)).Date;
                return startDate;
            }
            else
            {
                var endDate = Convert.ToDateTime(transaction.FirstDayOfLeave.AddDays(-1)).Date;
                return endDate;
            }
        }

        private async Task<IEnumerable<ScheduleMasterDetail>> GetRecordsWithLoAUpdated(
            IEnumerable<ScheduleMasterDetail> records)
        {
            var recordsWithLoAUpdated = records.ToList();
            var listEmployeeCodes = string.Join(",", recordsWithLoAUpdated.Select(r => r.EmployeeCode).Distinct());
            var resourcesLoAs =
                await _resourceApiClient.GetEmployeesLoATransactions(listEmployeeCodes);

            foreach (var record in recordsWithLoAUpdated)
            {
                var resourceLoA = resourcesLoAs.FirstOrDefault(x =>
                        x.EmployeeCode == record.EmployeeCode &&
                        record.Date.Date >= x.StartDate.Value.Date &&
                        record.Date.Date <= x.EndDate.Value.Date);

                if (resourceLoA == null) continue;

                var effectiveEmployeeStatus = string.IsNullOrEmpty(resourceLoA.Description) || resourceLoA.Description.Contains("Unpaid")
                   ? Constants.EmployeeStatus.LOAUnpaid
                   : Constants.EmployeeStatus.LOAPaid;

                record.EmployeeStatusCode = (int)effectiveEmployeeStatus < (int)record.EmployeeStatusCode
                    ? effectiveEmployeeStatus : record.EmployeeStatusCode;
                record.EffectiveCost = 0; //TODO : Remove once user start using actual cost and employeeStatusCode
                record.EffectiveCostReason = record.EffectiveCostReason.ConcatenateIfNotExists("LOA"); //TODO : Remove once user start using actual cost and employeeStatusCode
            }

            return recordsWithLoAUpdated;
        }

        private async Task<IEnumerable<ScheduleMasterDetail>> GetRecordsWithPointInTimeInfo(
            IEnumerable<ScheduleMasterDetail> records,
            IEnumerable<Resource> employees)
        {
            var recordsWithPointInTimeInfo = records.ToList();
            var listEmployeeCodes = recordsWithPointInTimeInfo.Select(x => x.EmployeeCode).Distinct();

            var employeesAllTransactions = await
                _resourceApiClient.GetEmployeesStaffingTransactions(string.Join(",", listEmployeeCodes));
            employeesAllTransactions =
                employeesAllTransactions.Where(x => x.TransactionStatus == "Successfully Completed");

            foreach (var record in recordsWithPointInTimeInfo)
            {
                var employeeAllTransactions = employeesAllTransactions
                    .Where(x => x.EmployeeCode == record.EmployeeCode)
                    .OrderBy(o => o.EffectiveDate.Value.Date)
                    .ThenBy(t => t.LastModifiedDate);

                var employeeTransition = employeeAllTransactions
                    .Where(x => x.BusinessProcessType == "Termination" || x.BusinessProcessType.StartsWith("Change Organization Assignments"))
                    .LastOrDefault(t => t.Transaction.TransitionStartDate != null &&
                        t.Transaction.CostCenterProposed.CostCenterId.StartsWith("0300_") &&
                        t.Transaction.TransitionStartDate.Value.Date <= record.Date.Date &&
                        t.Transaction.TransitionEndDate.Value.Date >= record.Date.Date);

                var employeeTermination = employeeAllTransactions
                    .LastOrDefault(x => x.BusinessProcessType == "Termination"
                        && x.TerminationEffectiveDate.Value.Date <= record.Date.Date);

                // check if resource is rehired then set employeeTermination to null to avoid setting employee status as Teminated
                if (employeeTermination != null)
                {
                    var employeeRehire = employeeAllTransactions
                        .LastOrDefault(x => x.BusinessProcessType == "Hire"
                                && x.EffectiveDate >= employeeTermination.TerminationEffectiveDate.Value.Date
                                && x.EffectiveDate.Value.Date <= record.Date.Date);
                    if (employeeRehire != null)
                    {
                        employeeTermination = null;
                    }
                }

                // check if resource is rehired then set employeeTransition to null to avoid setting employee status as Transition
                if (employeeTransition != null)
                {
                    var employeeRehire = employeeAllTransactions
                    .LastOrDefault(x => x.BusinessProcessType == "Hire" && x.EffectiveDate >= employeeTransition.Transaction.TransitionEndDate.Value.Date
                                && x.EffectiveDate.Value.Date <= record.Date.Date);
                    if (employeeRehire != null)
                    {
                        employeeTransition = null;
                    }
                }

                var employeeTransactionEffectiveNearDate =
                    employeeAllTransactions
                        .LastOrDefault(x => x.EffectiveDate.Value.Date <= record.Date.Date
                                            && x.Transaction.PdGradeProposed != null
                                            && x.Transaction.FteProposed != 0M
                                            && x.Transaction.SchedulingOfficeProposed?.OfficeCode != null)
                    ??
                    employeesAllTransactions
                        .FirstOrDefault(x => x.EffectiveDate.Value.Date > record.Date.Date
                                             && x.Transaction.PdGradeCurrent != null
                                             && x.Transaction.FteCurrent != 0M
                                             && x.Transaction.SchedulingOfficeCurrent?.OfficeCode != null);

                if (employeeTransactionEffectiveNearDate == null
                    && employeeTransition == null && employeeTermination == null)
                {
                    continue;
                }
                if (employeeTransactionEffectiveNearDate.EffectiveDate.Value.Date > record.Date)
                {
                    record.CurrentLevelGrade = employeeTransactionEffectiveNearDate.Transaction.PdGradeCurrent ?? employeeTransactionEffectiveNearDate.Transaction.PdGradeProposed;
                    record.Fte = employeeTransactionEffectiveNearDate.Transaction.FteCurrent;
                    record.BillCode = employeeTransactionEffectiveNearDate.Transaction.BillCodeCurrent;
                    int.TryParse(employeeTransactionEffectiveNearDate.Transaction.SchedulingOfficeCurrent?.OfficeCode,
                        out var officeCode);

                    record.OperatingOfficeCode = officeCode;
                    record.OperatingOfficeName = employeeTransactionEffectiveNearDate.Transaction
                        .SchedulingOfficeCurrent?.OfficeName ?? employeeTransactionEffectiveNearDate.Transaction
                        .SchedulingOfficeProposed?.OfficeName;
                    record.OperatingOfficeAbbreviation = employeeTransactionEffectiveNearDate.Transaction
                        .SchedulingOfficeCurrent?.OfficeAbbreviation ?? employeeTransactionEffectiveNearDate.Transaction
                        .SchedulingOfficeProposed?.OfficeAbbreviation;
                    record.EmployeeStatusCode = Constants.EmployeeStatus.Active;
                    record.ServiceLineCode = employeeTransactionEffectiveNearDate.Transaction.ServiceLineCurrent?.ServiceLineId ?? employeeTransactionEffectiveNearDate.Transaction.ServiceLineProposed?.ServiceLineId;
                    record.ServiceLineName = employeeTransactionEffectiveNearDate.Transaction.ServiceLineCurrent?.ServiceLineName ?? employeeTransactionEffectiveNearDate.Transaction.ServiceLineProposed?.ServiceLineName;
                    record.PositionCode = employeeTransactionEffectiveNearDate.Transaction.PositionCurrent?.PositionCode ?? employeeTransactionEffectiveNearDate.Transaction.PositionProposed?.PositionCode;
                    record.PositionName = employeeTransactionEffectiveNearDate.Transaction.PositionCurrent?.PositionName ?? employeeTransactionEffectiveNearDate.Transaction.PositionProposed?.PositionName;
                    record.PositionGroupName = employeeTransactionEffectiveNearDate.Transaction.PositionCurrent?.PositionGroupName ?? employeeTransactionEffectiveNearDate.Transaction.PositionProposed?.PositionGroupName;
                    record.Position = employeeTransactionEffectiveNearDate.Transaction.PositionCurrent?.PositionGroupName ?? employeeTransactionEffectiveNearDate.Transaction.PositionProposed?.PositionGroupName; //TODO : Remove once user starts using positionGroupName
                }
                if (employeeTransactionEffectiveNearDate.EffectiveDate.Value.Date <= record.Date)
                {
                    record.CurrentLevelGrade = employeeTransactionEffectiveNearDate.Transaction.PdGradeProposed ??
                                               employeeTransactionEffectiveNearDate.Transaction.PdGradeCurrent;
                    record.Fte = employeeTransactionEffectiveNearDate.Transaction.FteProposed == 0M ? employeeTransactionEffectiveNearDate.Transaction.FteCurrent : employeeTransactionEffectiveNearDate.Transaction.FteProposed;
                    record.BillCode = employeeTransactionEffectiveNearDate.Transaction.BillCodeProposed;
                    int.TryParse(employeeTransactionEffectiveNearDate.Transaction.SchedulingOfficeProposed?.OfficeCode ?? employeeTransactionEffectiveNearDate.Transaction.SchedulingOfficeCurrent?.OfficeCode,
                        out var officeCode);
                    record.OperatingOfficeCode = officeCode;
                    record.OperatingOfficeName = employeeTransactionEffectiveNearDate.Transaction
                        .SchedulingOfficeProposed?.OfficeName ?? employeeTransactionEffectiveNearDate.Transaction
                        .SchedulingOfficeCurrent?.OfficeName;
                    record.OperatingOfficeAbbreviation = employeeTransactionEffectiveNearDate.Transaction
                        .SchedulingOfficeProposed?.OfficeAbbreviation ?? employeeTransactionEffectiveNearDate.Transaction
                        .SchedulingOfficeCurrent?.OfficeAbbreviation;
                    record.EmployeeStatusCode = Constants.EmployeeStatus.Active;
                    record.ServiceLineCode = employeeTransactionEffectiveNearDate.Transaction.ServiceLineProposed?.ServiceLineId ?? employeeTransactionEffectiveNearDate.Transaction.ServiceLineCurrent?.ServiceLineId;
                    record.ServiceLineName = employeeTransactionEffectiveNearDate.Transaction.ServiceLineProposed?.ServiceLineName ?? employeeTransactionEffectiveNearDate.Transaction.ServiceLineCurrent?.ServiceLineName;
                    record.PositionCode = employeeTransactionEffectiveNearDate.Transaction.PositionProposed?.PositionCode ?? employeeTransactionEffectiveNearDate.Transaction.PositionCurrent?.PositionCode;
                    record.PositionName = employeeTransactionEffectiveNearDate.Transaction.PositionProposed?.PositionName ?? employeeTransactionEffectiveNearDate.Transaction.PositionCurrent?.PositionName;
                    record.PositionGroupName = employeeTransactionEffectiveNearDate.Transaction.PositionProposed?.PositionGroupName ?? employeeTransactionEffectiveNearDate.Transaction.PositionCurrent?.PositionGroupName;
                    record.Position = employeeTransactionEffectiveNearDate.Transaction.PositionProposed?.PositionGroupName ?? employeeTransactionEffectiveNearDate.Transaction.PositionCurrent?.PositionGroupName; //TODO : Remove once user starts using positionGroupName
                }
                if (employeeTransition?.Transaction?.TransitionStartDate.Value.Date <= record.Date &&
                        employeeTransition?.Transaction?.TransitionEndDate.Value.Date >= record.Date)
                {
                    record.EmployeeStatusCode = Constants.EmployeeStatus.Transition;
                }
                if (employeeTermination?.TerminationEffectiveDate.Value.Date <= record.Date)
                {
                    record.EmployeeStatusCode = Constants.EmployeeStatus.Terminated;
                }
            }

            return recordsWithPointInTimeInfo;
        }

        private IEnumerable<ScheduleMasterDetail> GetRecordsWithCurrentInfoUpdated(
            IEnumerable<ScheduleMasterDetail> recordsWithoutCost, List<Resource> resources)
        {
            var recordsWithBillCode = (from record in recordsWithoutCost
                                       join res in resources on record.EmployeeCode equals res.EmployeeCode into recordEmployee
                                       from resource in recordEmployee.DefaultIfEmpty()
                                       select new ScheduleMasterDetail
                                       {
                                           Id = record.Id,
                                           EmployeeCode = record.EmployeeCode,
                                           EmployeeStatusCode = record.EmployeeStatusCode,
                                           EmployeeName = resource?.FullName ?? record.EmployeeName, //always use current employee Name
                                           Fte = record.Fte,
                                           OperatingOfficeCode = record.OperatingOfficeCode,
                                           OperatingOfficeName = record.OperatingOfficeName ?? (resource?.SchedulingOffice?.OfficeName ?? resource?.Office?.OfficeName),
                                           OperatingOfficeAbbreviation = record.OperatingOfficeAbbreviation ?? (resource?.SchedulingOffice?.OfficeAbbreviation ?? resource?.Office.OfficeAbbreviation),
                                           CurrentLevelGrade = string.IsNullOrEmpty(record.CurrentLevelGrade) ? resource?.LevelGrade : record.CurrentLevelGrade,
                                           Allocation = record.Allocation,
                                           BillCode = resource?.BillCode ?? record.BillCode,
                                           Date = record.Date,
                                           InvestmentCode = record.InvestmentCode,
                                           CaseRoleCode = record.CaseRoleCode,
                                           ServiceLineCode = record.ServiceLineCode ?? resource?.ServiceLine?.ServiceLineCode,
                                           ServiceLineName = record.ServiceLineName ?? resource?.ServiceLine?.ServiceLineName,
                                           PositionCode = record.PositionCode ?? resource?.Position?.PositionCode,
                                           PositionName = record.PositionName ?? resource?.Position?.PositionName,
                                           PositionGroupName = record.PositionGroupName ?? resource?.Position?.PositionGroupName,
                                           Position = record.Position, //TOOD: Remove once user starts using positionGroupName
                                           BillRate = record.BillRate,
                                           BillRateType = record.BillRateType,
                                           BillRateCurrency = record.BillRateCurrency,
                                           ActualCost = record.ActualCost,
                                           EffectiveCost = record.EffectiveCost, //TODO : Remove once user start using actual cost and effectiveStatusCode
                                           EffectiveCostReason = record.EffectiveCostReason //TODO : Remove once user start using actual cost and effectiveStatusCode
                                       }).ToList();

            return recordsWithBillCode;
        }


        private DataTable ConvertToScheduleMasterDTO(IEnumerable<ScheduleMaster> recordsWithUpdatedServiceLine)
        {
            var scheduleMasterTable = new DataTable();
            scheduleMasterTable.Columns.Add("id", typeof(Guid));
            scheduleMasterTable.Columns.Add("clientCode", typeof(int));
            scheduleMasterTable.Columns.Add("caseCode", typeof(int));
            scheduleMasterTable.Columns.Add("oldCaseCode", typeof(string));
            scheduleMasterTable.Columns.Add("employeeCode", typeof(string));
            scheduleMasterTable.Columns.Add("serviceLineCode", typeof(string));
            scheduleMasterTable.Columns.Add("serviceLineName", typeof(string));
            scheduleMasterTable.Columns.Add("operatingOfficeCode", typeof(short));
            scheduleMasterTable.Columns.Add("currentLevelGrade", typeof(string));
            scheduleMasterTable.Columns.Add("allocation", typeof(short));
            scheduleMasterTable.Columns.Add("startDate", typeof(DateTime));
            scheduleMasterTable.Columns.Add("endDate", typeof(DateTime));
            scheduleMasterTable.Columns.Add("pipelineId", typeof(Guid));
            scheduleMasterTable.Columns.Add("investmentCode", typeof(short));
            scheduleMasterTable.Columns.Add("caseRoleCode", typeof(string));
            scheduleMasterTable.Columns.Add("lastUpdated", typeof(DateTime));
            scheduleMasterTable.Columns.Add("lastUpdatedBy", typeof(string));
            scheduleMasterTable.Columns.Add("notes", typeof(string));

            foreach (var record in recordsWithUpdatedServiceLine)
            {
                var row = scheduleMasterTable.NewRow();
                row["id"] = (object)record.Id ?? DBNull.Value;
                row["clientCode"] = (object)record.ClientCode ?? DBNull.Value;
                row["caseCode"] = (object)record.CaseCode ?? DBNull.Value;
                row["oldCaseCode"] = (object)record.OldCaseCode ?? DBNull.Value;
                row["employeeCode"] = (object)record.EmployeeCode ?? DBNull.Value;
                row["serviceLineCode"] = (object)record.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)record.ServiceLineName ?? DBNull.Value;
                row["operatingOfficeCode"] = (object)record.OperatingOfficeCode ?? DBNull.Value;
                row["currentLevelGrade"] = (object)record.CurrentLevelGrade ?? DBNull.Value;
                row["allocation"] = (object)record.Allocation ?? DBNull.Value;
                row["startDate"] = (object)record.StartDate ?? DBNull.Value;
                row["endDate"] = (object)record.EndDate ?? DBNull.Value;
                row["pipelineId"] = (object)record.PipelineId ?? DBNull.Value;
                row["investmentCode"] = (object)record.InvestmentCode ?? DBNull.Value;
                row["caseRoleCode"] = (object)record.CaseRoleCode ?? DBNull.Value;
                row["lastUpdated"] = (object)record.LastUpdated ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-ServiceLine";
                row["notes"] = (object)record.Notes ?? DBNull.Value;
                scheduleMasterTable.Rows.Add(row);
            }

            return scheduleMasterTable;
        }

        private static IEnumerable<ScheduleMasterDetail> ConvertToScheduleDetailMaster(
            IEnumerable<ResourceTransaction> resourceTransactions)
        {
            var viewModel = resourceTransactions.Select(item => new ScheduleMasterDetail
            {
                EmployeeCode = item.EmployeeCode,
                EmployeeStatusCode = item.Type == "Termination" ?
                    Constants.EmployeeStatus.Terminated :
                    (item.Type == "Transition" ?
                        Constants.EmployeeStatus.Transition :
                        Constants.EmployeeStatus.Active),
                Fte = item.FTE,
                OperatingOfficeCode = item.OperatingOffice?.OfficeCode ?? default,
                OperatingOfficeName = item.OperatingOffice?.OfficeName,
                OperatingOfficeAbbreviation = item.OperatingOffice?.OfficeAbbreviation,
                CurrentLevelGrade = item.LevelGrade,
                BillCode = item.BillCode,
                Date = item.Type == "Termination" ? (DateTime)item.EndDate : (DateTime)item.StartDate,
                ServiceLineCode = item.ServiceLine?.ServiceLineCode,
                ServiceLineName = item.ServiceLine?.ServiceLineName,
                PositionCode = item.Position?.PositionCode,
                PositionName = item.Position?.PositionName,
                PositionGroupName = item.Position.PositionGroupName,
                Position = item.Position.PositionGroupName,  // TODO: Delete once user shifted to use posiitionGroupName
                TransactionType = item.Type,
                EffectiveCostReason = item.Type == "Transition" || item.Type == "Termination" ? item.Type : null, // TODO : Remove once user start using actual cost and employeeStatusCode
                LastUpdated = item.LastUpdated
            });

            return viewModel;
        }

        private async Task<IEnumerable<ScheduleMasterDetail>> GetRecordsWithCostUpdated(
            IEnumerable<ScheduleMasterDetail> scheduleMasterDetails)
        {
            var officeCodes = string.Join(",", scheduleMasterDetails.Select(pt => pt.OperatingOfficeCode).Distinct());
            var billRates = await _ccmApiClient.GetBillRateByOffices(officeCodes);
            var recordsWithBillRate = GetAllocationsDividedByBillRate(billRates, scheduleMasterDetails);
            var recordsWithBillRateWithUSDConversion = await ConvertCostToUSD(recordsWithBillRate);
            return recordsWithBillRateWithUSDConversion;
        }

        private async Task<IEnumerable<ScheduleMasterDetail>> ConvertCostToUSD(IEnumerable<ScheduleMasterDetail> resourcesAllocations)
        {
            var currencyCodes = string.Join(",", resourcesAllocations.Select(ra => ra.BillRateCurrency)?.Distinct());
            var currencyRateTypeCodes = "B";
            var effectiveFromDate = resourcesAllocations.Min(ra => ra.Date);
            var effectiveToDate = resourcesAllocations.Max(ra => ra.Date);
            if (string.IsNullOrEmpty(currencyCodes))
                return resourcesAllocations;

            var curencyRates = await _basisApiClient.GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(currencyCodes, currencyRateTypeCodes,
                new DateTime(effectiveFromDate.Year, 1, 1), new DateTime(effectiveToDate.Year, 1, 1));
            var resourcesAllocationsWithUSDCost = resourcesAllocations.Select(r =>
            {
                var currencyRate = curencyRates.FirstOrDefault(c => c.CurrencyCode.Trim() == r.BillRateCurrency?.Trim()
                && c.EffectiveDate.Year == r.Date.Year);
                r.CostInUSD = r.ActualCost * currencyRate?.UsdRate;
                r.CostUSDEffectiveYear = currencyRate?.EffectiveDate.Year;
                r.UsdRate = currencyRate?.UsdRate;
                return r;
            }).ToList();
            return resourcesAllocationsWithUSDCost;
        }


        private static DataTable CreateAnalyticsRecordsDTO(IEnumerable<ScheduleMasterDetail> scheduleMasterDetail)
        {
            var analyticsRecordsTable = new DataTable();
            analyticsRecordsTable.Columns.Add("id", typeof(Guid));
            analyticsRecordsTable.Columns.Add("employeeCode", typeof(string));
            analyticsRecordsTable.Columns.Add("employeeName", typeof(string));
            analyticsRecordsTable.Columns.Add("employeeStatusCode", typeof(short));
            analyticsRecordsTable.Columns.Add("investmentCode", typeof(short));
            analyticsRecordsTable.Columns.Add("currentLevelGrade", typeof(string));
            analyticsRecordsTable.Columns.Add("fte", typeof(decimal));
            analyticsRecordsTable.Columns.Add("operatingOfficeCode", typeof(short));
            analyticsRecordsTable.Columns.Add("operatingOfficeName", typeof(string));
            analyticsRecordsTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            analyticsRecordsTable.Columns.Add("serviceLineCode", typeof(string));
            analyticsRecordsTable.Columns.Add("serviceLineName", typeof(string));
            analyticsRecordsTable.Columns.Add("positionCode", typeof(string));
            analyticsRecordsTable.Columns.Add("positionName", typeof(string));
            analyticsRecordsTable.Columns.Add("positionGroupName", typeof(string));
            analyticsRecordsTable.Columns.Add("position", typeof(string)); // TODO : Remove once user starts using positionGroupName
            analyticsRecordsTable.Columns.Add("actualCost", typeof(decimal));
            analyticsRecordsTable.Columns.Add("costInUSD", typeof(decimal));
            analyticsRecordsTable.Columns.Add("costUSDEffectiveYear", typeof(int));
            analyticsRecordsTable.Columns.Add("usdRate", typeof(decimal));
            analyticsRecordsTable.Columns.Add("effectiveCost", typeof(decimal)); //TODO : Remove once user start using actual cost and employeeStatusCode
            analyticsRecordsTable.Columns.Add("billRate", typeof(decimal));
            analyticsRecordsTable.Columns.Add("billCode", typeof(decimal));
            analyticsRecordsTable.Columns.Add("billRateType", typeof(string));
            analyticsRecordsTable.Columns.Add("billRateCurrency", typeof(string));
            analyticsRecordsTable.Columns.Add("effectiveCostReason", typeof(string)); //TODO : Remove once user start using actual cost and employeeStatusCode
            analyticsRecordsTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var record in scheduleMasterDetail)
            {
                var row = analyticsRecordsTable.NewRow();
                row["id"] = (object)record.Id ?? DBNull.Value;
                row["employeeCode"] = (object)record.EmployeeCode ?? DBNull.Value;
                row["employeeName"] = (object)record.EmployeeName ?? DBNull.Value;
                row["employeeStatusCode"] = (object)record.EmployeeStatusCode ?? DBNull.Value;
                row["investmentCode"] = (object)record.InvestmentCode ?? DBNull.Value;
                row["currentLevelGrade"] = (object)record.CurrentLevelGrade ?? DBNull.Value;
                row["fte"] = (object)record.Fte ?? DBNull.Value;
                row["operatingOfficeCode"] = (object)record.OperatingOfficeCode ?? DBNull.Value;
                row["operatingOfficeName"] = (object)record.OperatingOfficeName ?? DBNull.Value;
                row["operatingOfficeAbbreviation"] = (object)record.OperatingOfficeAbbreviation ?? DBNull.Value;
                row["serviceLineCode"] = (object)record.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)record.ServiceLineName ?? DBNull.Value;
                row["positionCode"] = (object)record.PositionCode ?? DBNull.Value;
                row["positionName"] = (object)record.PositionName ?? DBNull.Value;
                row["positionGroupName"] = (object)record.PositionGroupName ?? DBNull.Value;
                row["position"] = (object)record.Position ?? DBNull.Value; // TODO : Remove once user starts using positionGroupName
                row["actualCost"] = (object)record.ActualCost ?? DBNull.Value;
                row["costInUSD"] = (object)record.CostInUSD ?? DBNull.Value;
                row["costUSDEffectiveYear"] = (object)record.CostUSDEffectiveYear ?? DBNull.Value;
                row["usdRate"] = (object)record.UsdRate ?? DBNull.Value;
                row["effectiveCost"] = (object)record.EffectiveCost ?? DBNull.Value;  //TODO : Remove once user start using actual cost and employeeStatusCode
                row["billRate"] = (object)record.BillRate ?? DBNull.Value;
                row["billCode"] = (object)record.BillCode ?? DBNull.Value;
                row["billRateType"] = (object)record.BillRateType ?? DBNull.Value;
                row["billRateCurrency"] = (object)record.BillRateCurrency ?? DBNull.Value;
                row["effectiveCostReason"] = (object)record.EffectiveCostReason ?? DBNull.Value; //TODO : Remove once user start using actual cost and employeeStatusCode
                row["lastUpdatedBy"] = "Auto-Cost";
                analyticsRecordsTable.Rows.Add(row);
            }

            return analyticsRecordsTable;
        }

        private IEnumerable<ScheduleMasterDetail> GetAllocationsDividedByBillRate(IEnumerable<BillRate> billRates,
            IEnumerable<ScheduleMasterDetail> scheduleMasterDetails)
        {
            var recordsWithBillRates = new List<ScheduleMasterDetail>();
            foreach (var record in scheduleMasterDetails)
            {
                var filteredBillRates = GetBillRatesBySelectedValues(billRates,
                        record.CurrentLevelGrade, "S", record.BillCode, "M", record.OperatingOfficeCode);

                if (!filteredBillRates.Any())
                {
                    UpdateCostDataForAllocationWithNoBillRate(record);
                    recordsWithBillRates.Add(record);
                    continue;
                }
                var billRateForAllocation = filteredBillRates.FirstOrDefault(billRate =>
                        billRate.StartDate <= record.Date &&
                        (billRate.EndDate == null || record.Date <= billRate.EndDate));

                if (billRateForAllocation != null)
                {
                    recordsWithBillRates.AddRange(UpdateBillCostForAllocation(record, billRateForAllocation));
                    continue;
                }

                UpdateCostDataForAllocationWithNoBillRate(record);
                recordsWithBillRates.Add(record);

            }

            return recordsWithBillRates;
        }

        private static void UpdateCostDataForAllocationWithNoBillRate(ScheduleMasterDetail record)
        {
            record.ActualCost = 0M;
            record.CostInUSD = 0M;
            record.BillRate = null;
            record.UsdRate = null;
            record.BillRateCurrency = null;
            record.BillRateType = null;
            record.EffectiveCost = null;
            record.EffectiveCostReason = "Rate NA"; // TODO : Remove once user start using actual cost and employeeStatusCode
        }

        private IEnumerable<ScheduleMasterDetail> UpdateBillCostForAllocation(ScheduleMasterDetail scheduleMasterDetail,
            BillRate billRate)
        {
            var allocationsWithBillRates = new List<ScheduleMasterDetail>();
            var recordDate = scheduleMasterDetail.Date;

            var workingDaysInMonth = GetWorkingDaysInMonth(recordDate.Month, recordDate.Year);

            var clonedResourceAllocation = CloneScheduleMasterDetails(scheduleMasterDetail, recordDate);

            // Bill rate for bill code assigned to resource might not available and hence fetch bill rate for Bill Code 1
            clonedResourceAllocation.BillCode = billRate?.BillCode ?? scheduleMasterDetail.BillCode;
            clonedResourceAllocation.BillRateType = billRate?.Type;
            clonedResourceAllocation.BillRate = billRate?.Rate / workingDaysInMonth;
            clonedResourceAllocation.ActualCost =
                scheduleMasterDetail.Allocation * billRate?.Rate / workingDaysInMonth / 100;
            clonedResourceAllocation.EffectiveCost = scheduleMasterDetail.InvestmentCode == 5 ||
                clonedResourceAllocation.EffectiveCostReason == "Transition"
                ? 0
                : scheduleMasterDetail.Allocation * billRate?.Rate / workingDaysInMonth / 100; //TODO : Remove once user start using actual cost and employeeStatusCode
            clonedResourceAllocation.BillRateCurrency = billRate?.Currency.Trim();
            clonedResourceAllocation.EffectiveCostReason = scheduleMasterDetail.InvestmentCode == 5
                ? (string.IsNullOrEmpty(scheduleMasterDetail.EffectiveCostReason)
                    ? "Investment - Internal PD"
                    : scheduleMasterDetail.EffectiveCostReason.ConcatenateIfNotExists("Investment - Internal PD"))
                : (string.IsNullOrEmpty(scheduleMasterDetail.EffectiveCostReason)
                    ? clonedResourceAllocation.EffectiveCostReason
                    : scheduleMasterDetail.EffectiveCostReason.ConcatenateIfNotExists("Investment - Internal PD")); //TODO : Remove once user start using actual cost and employeeStatusCode
            allocationsWithBillRates.Add(clonedResourceAllocation);

            return allocationsWithBillRates;
        }

        private ScheduleMasterDetail CloneScheduleMasterDetails(ScheduleMasterDetail record, DateTime? recordDate)
        {
            return new ScheduleMasterDetail
            {
                Id = record.Id,
                EmployeeCode = record.EmployeeCode,
                EmployeeName = record.EmployeeName,
                EmployeeStatusCode = record.EmployeeStatusCode,
                Fte = record.Fte,
                ServiceLineCode = record.ServiceLineCode,
                ServiceLineName = record.ServiceLineName,
                CurrentLevelGrade = record.CurrentLevelGrade,
                BillCode = record.BillCode,
                OperatingOfficeCode = record.OperatingOfficeCode,
                OperatingOfficeAbbreviation = record.OperatingOfficeAbbreviation,
                OperatingOfficeName = record.OperatingOfficeName,
                PositionCode = record.PositionCode,
                PositionName = record.PositionName,
                PositionGroupName = record.PositionGroupName,
                Position = record.Position, //TODO : Remove once user starts using positionGroupName
                Allocation = record.Allocation,
                Date = recordDate ?? record.Date,
                InvestmentCode = record.InvestmentCode,
                CaseRoleCode = record.CaseRoleCode,
                TransactionType = record.TransactionType,
                EffectiveCostReason = record.EffectiveCostReason //TODO : Remove once user start using actual cost and employeeStatusCode
            };
        }

        private static int GetWorkingDaysInMonth(int month, int year)
        {
            var weekends = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var businessDaysInMonth = Enumerable.Range(1, daysInMonth)
                .Where(d => !weekends.Contains(new DateTime(year, month, d).DayOfWeek));

            return businessDaysInMonth.Count();
        }

        private IEnumerable<BillRate> GetBillRatesBySelectedValues(IEnumerable<BillRate> billRates, string levelGrade,
            string billRateType, decimal billCode, string breakdown, int officeCode)
        {
            try
            {
                levelGrade = string.IsNullOrEmpty(levelGrade)
                    ? string.Empty
                    : Regex.Replace(levelGrade, @"\s+", string.Empty);

                var filteredBillRates = billRates.Where(br =>
                    string.Equals(br.LevelGrade, levelGrade.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(br.Type, billRateType, StringComparison.OrdinalIgnoreCase) &&
                    br.BillCode == billCode &&
                    br.OfficeCode == officeCode.ToString() &&
                    string.Equals(br.Breakdown, breakdown, StringComparison.OrdinalIgnoreCase));

                return filteredBillRates;
            }
            catch (Exception)
            {
                return Enumerable.Empty<BillRate>();
            }
        }

        private static IEnumerable<AnalyticsResourceTransactionViewModel> ConvertToAnalyticsResourceTransaction(
            IEnumerable<ResourceLOA> resourceLOAs)
        {
            var viewModel = resourceLOAs.Select(item => new AnalyticsResourceTransactionViewModel
            {
                EmployeeCode = item.EmployeeCode,
                EmployeeStatusCode = item.Description.Contains("Unpaid")
                   ? Constants.EmployeeStatus.LOAUnpaid
                   : Constants.EmployeeStatus.LOAPaid,
                EffectiveDate = (DateTime)item.StartDate,
                EndDate = item.EndDate,
                EffectiveCostReason = item.Type,
                LastUpdated = item.LastUpdated,
                LastUpdatedBy = "Auto-PendingTransaction"
            });

            return viewModel;
        }

        private static IEnumerable<AnalyticsResourceTransactionViewModel> ConvertToAnalyticsResourceTransaction(
           IEnumerable<EmployeeTransaction> transactions)
        {
            var viewModel = transactions.Select(item => new AnalyticsResourceTransactionViewModel
            {
                EmployeeCode = item.EmployeeCode,
                EmployeeStatusCode = GetEmployeeStatusForStaffingTransaction(item),
                CurrentLevelGrade = item.TransactionStatus == "Successfully Completed" ? item.Transaction.PdGradeProposed : item.Transaction.PdGradeCurrent,
                FTE = item.TransactionStatus == "Successfully Completed" ? item.Transaction.FteProposed : item.Transaction.FteCurrent,
                ServiceLineCode = item.TransactionStatus == "Successfully Completed" ? item.Transaction.ServiceLineProposed?.ServiceLineId : item.Transaction.ServiceLineCurrent?.ServiceLineId,
                ServiceLineName = item.TransactionStatus == "Successfully Completed" ? item.Transaction.ServiceLineProposed?.ServiceLineName : item.Transaction.ServiceLineCurrent?.ServiceLineId,
                PositionCode = item.TransactionStatus == "Successfully Completed" ? item.Transaction.PositionProposed?.PositionCode : item.Transaction.PositionCurrent?.PositionCode,
                PositionName = item.TransactionStatus == "Successfully Completed" ? item.Transaction.PositionProposed?.PositionName : item.Transaction.PositionCurrent?.PositionName,
                PositionGroupName = item.TransactionStatus == "Successfully Completed" ? item.Transaction.PositionProposed?.PositionGroupName : item.Transaction.PositionCurrent?.PositionGroupName,
                Position = item.TransactionStatus == "Successfully Completed" ? item.Transaction.PositionProposed?.PositionGroupName : item.Transaction.PositionCurrent?.PositionGroupName, // TODO: Rmeove once user starts using positionGroupName
                OperatingOfficeCode = item.TransactionStatus == "Successfully Completed" ? Convert.ToInt32(item.Transaction.SchedulingOfficeProposed?.OfficeCode) : Convert.ToInt32(item.Transaction.SchedulingOfficeCurrent?.OfficeCode),
                OperatingOfficeAbbreviation = item.TransactionStatus == "Successfully Completed" ? item.Transaction.SchedulingOfficeProposed?.OfficeAbbreviation : item.Transaction.SchedulingOfficeCurrent?.OfficeAbbreviation,
                OperatingOfficeName = item.TransactionStatus == "Successfully Completed" ? item.Transaction.SchedulingOfficeProposed?.OfficeName : item.Transaction.SchedulingOfficeCurrent?.OfficeName,
                EffectiveDate = item.Transaction.TransitionStartDate == null ? item.EffectiveDate.Value.Date : item.Transaction.TransitionStartDate.Value.Date,
                EndDate = item.Transaction.TransitionEndDate.HasValue ? item.Transaction.TransitionEndDate.Value.Date : (DateTime?)null,
                LastUpdated = item.LastModifiedDate,
                LastUpdatedBy = "Auto-StaffingUpdated"
            });

            return viewModel;
        }

        private static Constants.EmployeeStatus GetEmployeeStatusForStaffingTransaction(EmployeeTransaction transaction)
        {
            if (transaction.TransactionStatus == "SuccessFullyCompleted")
            {
                if (transaction.TerminationEffectiveDate != null)
                {
                    return Constants.EmployeeStatus.Terminated;
                }
                if (transaction.Transaction.TransitionStartDate != null)
                {
                    return Constants.EmployeeStatus.Transition;
                }
            }

            return Constants.EmployeeStatus.Active;
        }

        private static IEnumerable<AnalyticsResourceTransactionViewModel> ConvertToAnalyticsResourceTransaction(
            IEnumerable<LOATransaction> loaTransactions)
        {
            var viewModel = loaTransactions.Select(item => new AnalyticsResourceTransactionViewModel
            {
                EmployeeCode = item.EmployeeCode,
                EmployeeStatusCode = item.TransactionStatus == "Rescinded"
                    ? Constants.EmployeeStatus.Active
                    : (item.BusinessProcessReason.Contains("Unpaid") ? Constants.EmployeeStatus.LOAUnpaid : Constants.EmployeeStatus.LOAPaid),
                EffectiveDate = (DateTime)item.EffectiveDate,
                EndDate = (item.Transaction?.ActualLastDayOfLeave ?? item.Transaction?.EstimatedLastDayOfLeave).Value.Date,
                EffectiveCostReason = "LoaUpdatedRecently",
                LastUpdated = item.LastModifiedDate,
                LastUpdatedBy = "Auto-LoAUpdated"
            });

            return viewModel;
        }

        private static IEnumerable<AnalyticsResourceTransactionViewModel> ConvertToAnalyticsResourceTransaction(
            IEnumerable<ScheduleMasterDetail> scheduleMasterDetail)
        {
            var viewModel = scheduleMasterDetail.Select(item => new AnalyticsResourceTransactionViewModel
            {
                EmployeeCode = item.EmployeeCode,
                EmployeeStatusCode = item.EmployeeStatusCode,
                EffectiveDate = item.Date,
                CurrentLevelGrade = item.CurrentLevelGrade,
                FTE = item.Fte,
                OperatingOfficeCode = item.OperatingOfficeCode,
                OperatingOfficeAbbreviation = item.OperatingOfficeAbbreviation,
                OperatingOfficeName = item.OperatingOfficeName,
                ServiceLineCode = item.ServiceLineCode,
                ServiceLineName = item.ServiceLineName,
                PositionCode = item.PositionCode,
                PositionName = item.PositionName,
                PositionGroupName = item.PositionGroupName,
                Position = item.Position,  // TODO: Delete once user shifted to use posiitionGroupName
                BillCode = item.BillCode,
                ActualCost = null,
                EffectiveCost = item.EffectiveCostReason != null ? (decimal?)0M : null, //TODO : Remove once user start using actual cost and employeeStatusCode
                BillRate = null,
                BillRateCurrency = null,
                BillRateType = null,
                EffectiveCostReason = item.EffectiveCostReason, //TODO : Remove once user start using actual cost and employeeStatusCode
                LastUpdated = item.LastUpdated,
                LastUpdatedBy = "Auto-PendingTrans"

            });

            return viewModel;
        }

        public static DataTable GetAnalyticsResourceTransactionDTO(
            IEnumerable<AnalyticsResourceTransactionViewModel> resourceTransactions)
        {
            var resourceTransactionsDataTable = new DataTable();
            resourceTransactionsDataTable.Columns.Add("employeeCode", typeof(string));
            resourceTransactionsDataTable.Columns.Add("employeeStatusCode", typeof(short));
            resourceTransactionsDataTable.Columns.Add("effectiveDate", typeof(DateTime));
            resourceTransactionsDataTable.Columns.Add("endDate", typeof(DateTime));
            resourceTransactionsDataTable.Columns.Add("currentLevelGrade", typeof(string));
            resourceTransactionsDataTable.Columns.Add("fte", typeof(decimal));
            resourceTransactionsDataTable.Columns.Add("operatingOfficeCode", typeof(int));
            resourceTransactionsDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            resourceTransactionsDataTable.Columns.Add("operatingOfficeName", typeof(string));
            resourceTransactionsDataTable.Columns.Add("serviceLineCode", typeof(string));
            resourceTransactionsDataTable.Columns.Add("serviceLineName", typeof(string));
            resourceTransactionsDataTable.Columns.Add("positionCode", typeof(string));
            resourceTransactionsDataTable.Columns.Add("positionName", typeof(string));
            resourceTransactionsDataTable.Columns.Add("positionGroupName", typeof(string));
            resourceTransactionsDataTable.Columns.Add("position", typeof(string)); // TODO: Delete once user shifted to use posiitionGroupName
            resourceTransactionsDataTable.Columns.Add("actualCost", typeof(decimal));
            resourceTransactionsDataTable.Columns.Add("effectiveCost", typeof(decimal)); //TODO : Remove once user start using actual cost and effectiveStatusCode
            resourceTransactionsDataTable.Columns.Add("billRate", typeof(decimal));
            resourceTransactionsDataTable.Columns.Add("billCode", typeof(decimal));
            resourceTransactionsDataTable.Columns.Add("billRateType", typeof(string));
            resourceTransactionsDataTable.Columns.Add("billRateCurrency", typeof(string));
            resourceTransactionsDataTable.Columns.Add("effectiveCostReason", typeof(string)); //TODO : Remove once user start using actual cost and effectiveStatusCode
            resourceTransactionsDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var transaction in resourceTransactions)
            {
                var row = resourceTransactionsDataTable.NewRow();

                row["employeeCode"] = (object)transaction.EmployeeCode ?? DBNull.Value;
                row["employeeStatusCode"] = (object)((int)transaction.EmployeeStatusCode) ?? DBNull.Value;
                row["effectiveDate"] = (object)transaction.EffectiveDate.Date ?? DBNull.Value;
                row["endDate"] = (object)transaction.EndDate?.Date ?? DBNull.Value;
                row["currentLevelGrade"] = (object)transaction.CurrentLevelGrade ?? DBNull.Value;
                row["fte"] = (object)transaction.FTE ?? DBNull.Value;
                row["operatingOfficeCode"] = (object)transaction.OperatingOfficeCode ?? DBNull.Value;
                row["operatingOfficeAbbreviation"] = (object)transaction.OperatingOfficeAbbreviation ?? DBNull.Value;
                row["operatingOfficeName"] = (object)transaction.OperatingOfficeName ?? DBNull.Value;
                row["serviceLineCode"] = (object)transaction.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)transaction.ServiceLineName ?? DBNull.Value;
                row["positionCode"] = (object)transaction.PositionCode ?? DBNull.Value;
                row["positionName"] = (object)transaction.PositionName ?? DBNull.Value;
                row["positionGroupName"] = (object)transaction.PositionGroupName ?? DBNull.Value;
                row["position"] = (object)transaction.Position ?? DBNull.Value; // TODO: Delete once user shifted to use posiitionGroupName
                row["actualCost"] = (object)transaction.ActualCost ?? DBNull.Value;
                row["effectiveCost"] = (object)transaction.EffectiveCost ?? DBNull.Value; //TODO : Remove once user start using actual cost and employeeStatusCode
                row["billRate"] = (object)transaction.BillRate ?? DBNull.Value;
                row["billCode"] = (object)transaction.BillCode ?? DBNull.Value;
                row["billRateType"] = (object)transaction.BillRateType ?? DBNull.Value;
                row["billRateCurrency"] = (object)transaction.BillRateCurrency ?? DBNull.Value;
                row["effectiveCostReason"] = (object)transaction.EffectiveCostReason ?? DBNull.Value; //TODO : Remove once user start using actual cost and employeeStatusCode
                row["lastUpdatedBy"] = (object)transaction.LastUpdatedBy ?? DBNull.Value;

                resourceTransactionsDataTable.Rows.Add(row);
            }

            return resourceTransactionsDataTable;
        }

        private static DataTable GetResourceAvailabilityAnalyticsDTO(
            IEnumerable<ResourceAvailabilityViewModel> resourcesWithNoAllocations)
        {
            var resourceAvailabilityDataTable = new DataTable();
            resourceAvailabilityDataTable.Columns.Add("employeeCode", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("employeeName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("employeeStatusCode", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("fte", typeof(decimal));
            resourceAvailabilityDataTable.Columns.Add("operatingOfficeCode", typeof(int));
            resourceAvailabilityDataTable.Columns.Add("operatingOfficeName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("currentLevelGrade", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("serviceLineCode", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("serviceLineName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("positionCode", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("positionName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("positionGroupName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("position", typeof(string)); // TODO : Remove once user starts using positionGroupName
            resourceAvailabilityDataTable.Columns.Add("hireDate", typeof(DateTime));
            resourceAvailabilityDataTable.Columns.Add("terminationDate", typeof(DateTime));
            resourceAvailabilityDataTable.Columns.Add("startDate", typeof(DateTime));
            resourceAvailabilityDataTable.Columns.Add("endDate", typeof(DateTime));
            resourceAvailabilityDataTable.Columns.Add("availability", typeof(int));
            resourceAvailabilityDataTable.Columns.Add("effectiveAvailability", typeof(int));// TODO : Rmeove once user starts using employeeStatusCode
            resourceAvailabilityDataTable.Columns.Add("effectiveAvailabilityReason", typeof(string));// TODO : Rmeove once user starts using employeeStatusCode
            resourceAvailabilityDataTable.Columns.Add("billCode", typeof(decimal));
            resourceAvailabilityDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("opportunityCost", typeof(decimal));
            resourceAvailabilityDataTable.Columns.Add("effectiveOpportunityCost", typeof(decimal)); // TODO : Rmeove once user starts using employeeStatusCode
            resourceAvailabilityDataTable.Columns.Add("effectiveOpportunityCostReason", typeof(string));// TODO : Rmeove once user starts using employeeStatusCode
            resourceAvailabilityDataTable.Columns.Add("billRate", typeof(decimal));
            resourceAvailabilityDataTable.Columns.Add("billRateType", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("billRateCurrency", typeof(string));

            foreach (var resource in resourcesWithNoAllocations)
            {
                var row = resourceAvailabilityDataTable.NewRow();

                row["employeeCode"] = (object)resource.EmployeeCode ?? DBNull.Value;
                row["employeeName"] = (object)resource.EmployeeName ?? DBNull.Value;
                row["employeeStatusCode"] = (object)((int)resource.EmployeeStatusCode) ?? DBNull.Value;
                row["fte"] = resource.Fte;
                row["operatingOfficeCode"] = resource.OperatingOfficeCode;
                row["operatingOfficeAbbreviation"] = (object)resource.OperatingOfficeAbbreviation ?? DBNull.Value;
                row["operatingOfficeName"] = (object)resource.OperatingOfficeName ?? DBNull.Value;
                row["currentLevelGrade"] = (object)resource.CurrentLevelGrade ?? DBNull.Value;
                row["serviceLineCode"] = (object)resource.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)resource.ServiceLineName ?? DBNull.Value;
                row["positionCode"] = (object)resource.PositionCode ?? DBNull.Value;
                row["positionName"] = (object)resource.PositionName ?? DBNull.Value;
                row["positionGroupName"] = (object)resource.PositionGroupName ?? DBNull.Value;
                row["position"] = (object)resource.Position ?? DBNull.Value; // TODO: Remove once user starts using positionGroupName
                row["hireDate"] = resource.HireDate;
                row["terminationDate"] = (object)resource.TerminationDate ?? DBNull.Value;
                row["startDate"] = resource.StartDate;
                row["endDate"] = resource.EndDate;
                row["availability"] = resource.Availability;
                row["effectiveAvailability"] = (object)resource.EffectiveAvailability ?? DBNull.Value; // TODO: Remove once user starts using employeeStatusCode
                row["effectiveAvailabilityReason"] = (object)resource.EffectiveAvailabilityReason ?? DBNull.Value; // TODO: Remove once user starts using employeeStatusCode
                row["billCode"] = resource.BillCode;
                row["lastUpdatedBy"] = (object)resource.LastUpdatedBy ?? DBNull.Value;
                row["opportunityCost"] = (object)resource.OpportunityCost ?? DBNull.Value;
                row["effectiveOpportunityCost"] = (object)resource.EffectiveOpportunityCost ?? DBNull.Value; // TODO: Remove once user starts using employeeStatusCode
                row["effectiveOpportunityCostReason"] =
                    (object)resource.EffectiveOpportunityCostReason ?? DBNull.Value; // TODO: Remove once user starts using employeeStatusCode
                row["billRate"] = (object)resource.BillRate ?? DBNull.Value;
                row["billRateType"] = (object)resource.BillRateType ?? DBNull.Value;
                row["billRateCurrency"] = (object)resource.BillRateCurrency ?? DBNull.Value;

                resourceAvailabilityDataTable.Rows.Add(row);
            }

            return resourceAvailabilityDataTable;
        }

        private DataTable CovertToCommitmentMasterDTO(IEnumerable<ResourceLOA> futureLOAs)
        {
            var commitmentmasterDataTable = new DataTable();
            commitmentmasterDataTable.Columns.Add("id", typeof(Guid));
            commitmentmasterDataTable.Columns.Add("employeeCode", typeof(string));
            commitmentmasterDataTable.Columns.Add("commitmentTypeCode", typeof(string));
            commitmentmasterDataTable.Columns.Add("startDate", typeof(DateTime));
            commitmentmasterDataTable.Columns.Add("endDate", typeof(DateTime));
            commitmentmasterDataTable.Columns.Add("allocation", typeof(int));
            commitmentmasterDataTable.Columns.Add("notes", typeof(string));
            commitmentmasterDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var loa in futureLOAs)
            {
                var row = commitmentmasterDataTable.NewRow();

                row["id"] = DBNull.Value;
                row["employeeCode"] = (object)loa.EmployeeCode ?? DBNull.Value;
                row["commitmentTypeCode"] = "L";
                row["startDate"] = (object)loa.StartDate.Value.Date ?? DBNull.Value;
                row["endDate"] = (object)loa.EndDate.Value.Date ?? DBNull.Value;
                row["allocation"] = DBNull.Value; // Allocation need not to be saved for LOA
                row["notes"] = (object)loa.Description ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-OverrideFlagForWDLoA";

                commitmentmasterDataTable.Rows.Add(row);
            }

            return commitmentmasterDataTable;

        }

        private DataTable CovertToResourceTransactionDTO(IEnumerable<ResourceTransaction> futureTerminations)
        {
            DataTable resourceTransactionsDataTable = new DataTable();

            resourceTransactionsDataTable.Columns.Add("employeeCode", typeof(string));
            resourceTransactionsDataTable.Columns.Add("employeeStatusCode", typeof(short));
            resourceTransactionsDataTable.Columns.Add("startDate", typeof(DateTime));
            resourceTransactionsDataTable.Columns.Add("endDate", typeof(DateTime));
            resourceTransactionsDataTable.Columns.Add("type", typeof(string));

            foreach (var resource in futureTerminations)
            {
                var row = resourceTransactionsDataTable.NewRow();

                row["employeeCode"] = (object)resource.EmployeeCode ?? DBNull.Value;
                row["employeeStatusCode"] = (object)((int)Constants.EmployeeStatus.Terminated) ?? DBNull.Value;
                row["startDate"] = resource.StartDate;
                row["endDate"] = resource.EndDate.HasValue ? (object)resource.EndDate : DBNull.Value;
                row["type"] = resource.Type;

                resourceTransactionsDataTable.Rows.Add(row);
            }

            return resourceTransactionsDataTable;
        }
        private IList<ResourceTransaction> ConvertToResourceTransactionModel(List<Resource> terminatedEmployees)
        {
            return terminatedEmployees.Select(e => new ResourceTransaction
            {
                EmployeeCode = e.EmployeeCode,
                BillCode = e.BillCode,
                EndDate = e.TerminationDate,
                FTE = e.Fte,
                LevelGrade = e.LevelGrade,
                OperatingOffice = e.SchedulingOffice != null ? new Models.Office
                {
                    OfficeAbbreviation = e.SchedulingOffice.OfficeAbbreviation,
                    OfficeCode = e.SchedulingOffice.OfficeCode,
                    OfficeName = e.SchedulingOffice.OfficeName
                } : null,
                Position = new Position
                {
                    PositionCode = e.Position?.PositionCode,
                    PositionName = e.Position?.PositionName,
                    PositionGroupName = e.Position?.PositionGroupName
                },
                StartDate = e.StartDate,
                Type = "Termination"
            }).ToList();
        }

        private async Task<string> RemoveTerminatedEmployeesFromSecurityUsers(List<Resource> terminatedEmployees)
        {
            var bossSecurityUsersTask = _staffingApiClient.GetBossSecurityUsers();
            var bossSecurityUsers = bossSecurityUsersTask.Result;

            var teminatedBossSecurityUsers = bossSecurityUsers.Join(terminatedEmployees,
                                                                bossUser => bossUser.EmployeeCode,
                                                                terminatedEmployee => terminatedEmployee.EmployeeCode,
                                                                (bossUser, terminatedEmployee) => bossUser)
                                                                .ToList();

            var employeeCodes = teminatedBossSecurityUsers.Select(user => user.EmployeeCode).ToList();
            string commaSeparatedEmployeeCodes = string.Join(",", employeeCodes);

            if(commaSeparatedEmployeeCodes!="")
            await _staffingApiClient.DeleteBossSecurityUsers(commaSeparatedEmployeeCodes);
            return commaSeparatedEmployeeCodes;
        }

        #endregion

        #region Unused Methods
        //This method is not being used as we are using CCM offices instead of Workday Offices
        //public async Task<IEnumerable<WorkdayOffice>> SaveWorkdayOfficeFlatListForTableau()
        //{
        //    var officeFlatList = await
        //           _workdayRedisConnectorApiClient.GetOfficeFlatListForTableauFromRedis();

        //    if (officeFlatList.Any())
        //    {
        //        var officeDataTable = ConvertToOfficeDTO(officeFlatList);

        //        var savedOffices = await _workdayPollingRepository.SaveWorkdayOfficeFlatListForTableau(officeDataTable);
        //        return savedOffices;
        //    }
        //    return Enumerable.Empty<WorkdayOffice>();
        //}

        //private DataTable ConvertToOfficeDTO(IEnumerable<WorkdayOffice> offices)
        //{
        //    var officeDataTable = new DataTable();
        //    officeDataTable.Columns.Add("level1", typeof(string));
        //    officeDataTable.Columns.Add("level2", typeof(string));
        //    officeDataTable.Columns.Add("level3", typeof(string));
        //    officeDataTable.Columns.Add("level4", typeof(string));
        //    officeDataTable.Columns.Add("level5", typeof(string));
        //    officeDataTable.Columns.Add("level6", typeof(string));
        //    officeDataTable.Columns.Add("officeCode", typeof(string));
        //    officeDataTable.Columns.Add("officeAbbreviation", typeof(string));
        //    officeDataTable.Columns.Add("lastUpdatedBy", typeof(string));

        //    foreach (var office in offices)
        //    {
        //        var row = officeDataTable.NewRow();
        //        row["level1"] = (object)office.Level1 ?? DBNull.Value;
        //        row["level2"] = (object)office.Level2 ?? DBNull.Value;
        //        row["level3"] = (object)office.Level3 ?? DBNull.Value;
        //        row["level4"] = (object)office.Level4 ?? DBNull.Value;
        //        row["level5"] = (object)office.Level5 ?? DBNull.Value;
        //        row["level6"] = (object)office.Level6 ?? DBNull.Value;
        //        row["officeCode"] = (object)office.OfficeCode ?? DBNull.Value;
        //        row["officeAbbreviation"] = (object)office.OfficeAbbreviation ?? DBNull.Value;
        //        row["lastUpdatedBy"] = "Auto-WDOffice";

        //        officeDataTable.Rows.Add(row);
        //    }

        //    return officeDataTable;
        //}
        #endregion
    }
}