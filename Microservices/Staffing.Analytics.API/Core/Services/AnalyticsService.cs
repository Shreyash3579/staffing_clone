using Hangfire;
using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using Staffing.Analytics.API.Models.Workday;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microservices.Common.Core.Helpers;
using System.Text.RegularExpressions;

namespace Staffing.Analytics.API.Core.AnalyticsService
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IResourceAllocationRepository _resourceAllocationRepository;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IResourceAvailabilityRepository _resourceAvailabilityRepository;
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IPipelineApiClient _pipelineApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IBasisApiClient _basisApiClient;
        private readonly IBvuApiClient _bvuApiClient;
        private readonly IVacationApiClient _vacationApiClient;
        private readonly IPollMasterRepository _pollMasterRepository;

        public AnalyticsService(IResourceAllocationRepository resourceAllocationRepository,
            IResourceApiClient resourceApiClient,
            IResourceAvailabilityRepository resourceAvailabilityRepository,
            IAnalyticsRepository analyticsRepository,
            ICCMApiClient ccmApiClient,
            IPipelineApiClient pipelineApiClient,
            IStaffingApiClient staffingApiClient,
            IBasisApiClient basisApiClient,
            IBvuApiClient bvuApiClient,
            IVacationApiClient vacationApiClient,
            IBackgroundJobClient backgroundJobClient,
            IPollMasterRepository pollMasterRepository)
        {
            _resourceAllocationRepository = resourceAllocationRepository;
            _resourceApiClient = resourceApiClient;
            _resourceAvailabilityRepository = resourceAvailabilityRepository;
            _backgroundJobClient = backgroundJobClient;
            _analyticsRepository = analyticsRepository;
            _pipelineApiClient = pipelineApiClient;
            _ccmApiClient = ccmApiClient;
            _staffingApiClient = staffingApiClient;
            _basisApiClient = basisApiClient;
            _bvuApiClient = bvuApiClient;
            _vacationApiClient = vacationApiClient;
            _pollMasterRepository = pollMasterRepository;
        }

        public async Task<IEnumerable<string>> CreateAnalyticsReport(string scheduleIds)
        {
            var allocations = new List<ResourceAllocation>();

            var scheduleIdList = scheduleIds.Split(',');
            var totalRecords = scheduleIdList.Count();
            var batchSize = 50;
            var skipRecords = 0;

            var processedIds = new List<string>();

            while (skipRecords < totalRecords)
            {
                var selectedScheduleIdList = scheduleIdList.OrderBy(x => x).Skip(skipRecords).Take(batchSize);

                var selectedScheduleIds = string.Join(",", selectedScheduleIdList);

                var resourceAllocationsWithDetails = await GetResourceAllocationWithDetails(selectedScheduleIds);

                if (!resourceAllocationsWithDetails.Any())
                {
                    skipRecords += batchSize;
                    continue;
                }

                var employeeCodes = string.Join(",", resourceAllocationsWithDetails.Select(r => r.EmployeeCode).Distinct());

                await SplitAllocationsForHistoricalTransactionOrLevelGradeChange(resourceAllocationsWithDetails, allocations, employeeCodes);

                resourceAllocationsWithDetails = allocations.Any() ? allocations : resourceAllocationsWithDetails;

                allocations = new List<ResourceAllocation>();

                await SplitAllocationsForPendingTransactionOrLoA(resourceAllocationsWithDetails, allocations, employeeCodes);

                allocations = await UpdateCommitmentsInAllocations(allocations);

                var resourcesAllocationsWithBillRateForTableau = await GetResourcesAllocationsWithBillRate(allocations);

                resourcesAllocationsWithBillRateForTableau = await VerifyProcessedAllocationExistsInDB(resourcesAllocationsWithBillRateForTableau);

                processedIds = resourcesAllocationsWithBillRateForTableau.Select(x => x.Id.ToString()).Distinct().ToList();

                var resourceAllocationsForTableauDataTable =
                    CreateAssignmentDataTable(resourcesAllocationsWithBillRateForTableau);

                var allocationRangeBeforeUpdateForResources = await _resourceAllocationRepository.UpsertAnalyticsReportData(resourceAllocationsForTableauDataTable);

                var dateRangeToUpsertAvailabilityFor = GetAvailabilityDateRangeToUpsertFromPreviousAndNewAllocationDates(allocationRangeBeforeUpdateForResources, resourcesAllocationsWithBillRateForTableau);

                // Trigger Hangfire jobs to update dependent Tables like Resource Availability
                var parentJobId = _backgroundJobClient.Enqueue<IAnalyticsService>(x =>
                    x.UpsertAvailabilityDataBetweenDateRange(dateRangeToUpsertAvailabilityFor));

                _backgroundJobClient.ContinueJobWith<IAnalyticsService>(parentJobId,
                    x => x.UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));

                skipRecords += batchSize;
            }


            return processedIds;
        }

        public async Task<IEnumerable<string>> CorrectAnalyticsData()
        {
            var schedueleIdList = await _analyticsRepository.GetScheduleIdsIncorrectlyProcessedInAnalytics();
            var totalRecords = schedueleIdList.Count();
            var batchSize = 50;
            var skipRecords = 0;
            var correctedScheduleIds = new List<string>();
            while (skipRecords < totalRecords)
            {
                var scheduleIdsToCorrect = schedueleIdList.OrderBy(x => x).Skip(skipRecords).Take(batchSize);
                correctedScheduleIds.AddRange(await CreateAnalyticsReport(string.Join(",", scheduleIdsToCorrect)));
                skipRecords += batchSize;
            }
            return correctedScheduleIds;
        }

        /**
         * Occassionaly when allocation is created and deleted in quick succession, 
         * Analytics API takes time to process the allocation that results in delete request complete first and then processed data for analytics gets inserted in DB
         * We want to avoid this situation and hence reverifying if allocation exists in DB before saving the analytics data.
         */
        private async Task<IEnumerable<ResourceAllocation>> VerifyProcessedAllocationExistsInDB(IEnumerable<ResourceAllocation> resourceAllocationsWitBillRate)
        {
            var distinctScheduleIds = resourceAllocationsWitBillRate.Select(r => r.Id.ToString().ToLower()).Distinct();

            var resourceAllocations = await _staffingApiClient.GetResourceAllocationsByScheduleIds(string.Join(",", distinctScheduleIds));

            var scheduleIdsExistsInDB = resourceAllocations.Select(r => r.Id.ToString().ToLower()).Distinct();

            resourceAllocationsWitBillRate = resourceAllocationsWitBillRate.Where(x => scheduleIdsExistsInDB.ToList().Contains(x.Id.ToString().ToLower()));
            return resourceAllocationsWitBillRate;
        }

        public async Task UpsertCapacityAnalysisDaily(bool? fullLoad, DateTime? loadAfterLastUpdated)
        {
            await _analyticsRepository.UpsertCapacityAnalysisDaily(fullLoad, loadAfterLastUpdated);
            if (fullLoad.HasValue && !!fullLoad.Value)
            {
                await UpsertCapacityAnalysisMonthly(fullLoad);
            }
        }

        public async Task UpdateCapacityAnalysisDailyForChangeInCaseAttribute(DateTime? updateAfterTimeStamp)
        {
            await _analyticsRepository.UpdateCapacityAnalysisDailyForChangeInCaseAttribute(updateAfterTimeStamp);

        }

        public async Task UpsertCapacityAnalysisMonthly(bool? fullLoad)
        {
            await _analyticsRepository.UpsertCapacityAnalysisMonthly(fullLoad);
        }

        public async Task<List<ResourceAllocation>> UpdateCommitmentsInAllocations(List<ResourceAllocation> allocations)
        {
            var employeeCodes = string.Join(",", allocations.Select(r => r.EmployeeCode).Distinct());
            var fromDate = allocations.Min(a => a.StartDate);
            var toDate = allocations.Max(a => a.EndDate);
            var commitments = await GetCommitmentsFromBOSSAndExternalSystems(employeeCodes, fromDate, toDate);

            var resourcesCommitments = GetCommitmentsSplittedDayWise(commitments);

            List<ResourceCommitment> resourcesCommitmentsWithDateRange = GetAggregatedCommitments(resourcesCommitments);

            allocations = splitAllocationsForCommitments(allocations, resourcesCommitmentsWithDateRange);

            return allocations;


        }

        private async Task<IEnumerable<CommitmentViewModel>> GetCommitmentsFromBOSSAndExternalSystems(string employeeCodes, DateTime fromDate, DateTime? toDate)
        {
            var commitmentsFromBOSSTask = _staffingApiClient.GetResourceCommitmentsWithinDateRangeByEmployees(employeeCodes, fromDate, toDate, null);
            var trainingsFromBVUTask = _bvuApiClient.GetTrainingsWithinDateRangeByEmployeeCodes(employeeCodes, fromDate, toDate);
            var vacationsFromVRSTask = _vacationApiClient.GetVacationsWithinDateRangeByEmployeeCodes(employeeCodes, fromDate, toDate);
            var timeOffsTask = _resourceApiClient.GetEmployeesTimeoffs(employeeCodes, fromDate, toDate);
            var holidaysTask = _analyticsRepository.GetHolidayWithinDateRangeByEmployees(employeeCodes, fromDate, toDate);
            var commitmentTypesTask = _staffingApiClient.GetCommitmentTypeList(true);

            await Task.WhenAll(commitmentsFromBOSSTask, trainingsFromBVUTask, vacationsFromVRSTask, timeOffsTask,
                holidaysTask, commitmentTypesTask);

            var commitmentsFromBOSS = commitmentsFromBOSSTask.Result;
            var trainingsFromBVU = trainingsFromBVUTask.Result;
            var vacationsFromVRS = vacationsFromVRSTask.Result;
            var timeOffsFromWorkday = timeOffsTask.Result;
            var officeHolidays = holidaysTask.Result;
            var commitmentTypes = commitmentTypesTask.Result;

            var commitments = new List<CommitmentViewModel>();

            if (commitmentsFromBOSS.Any())
            {
                commitments.AddRange(commitmentsFromBOSS);
            }

            if (trainingsFromBVU.Any())
            {

                var trainingCommitmentType = commitmentTypes.FirstOrDefault(ct => ct.CommitmentTypeCode == "T");

                var trainings = trainingsFromBVU.Select(item => new CommitmentViewModel
                {
                    EmployeeCode = item.EmployeeCode,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    IsSourceStaffing = false,
                    IsOverridenInSource = false,
                    CommitmentTypeCode = trainingCommitmentType.CommitmentTypeCode,
                    CommitmentTypeName = trainingCommitmentType.CommitmentTypeName,
                    Precdence = trainingCommitmentType.Precedence,
                    ReportingPrecdence = trainingCommitmentType.ReportingPrecedence,
                    IsStaffingTag = trainingCommitmentType.IsStaffingTag
                }).ToList();

                commitments.AddRange(trainings);
            }

            if (vacationsFromVRS.Any())
            {

                var vacationCommitmentType = commitmentTypes.FirstOrDefault(ct => ct.CommitmentTypeCode == "V");

                var vacations = vacationsFromVRS.Select(item => new CommitmentViewModel
                {
                    EmployeeCode = item.EmployeeCode,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    IsSourceStaffing = false,
                    IsOverridenInSource = false,
                    CommitmentTypeCode = vacationCommitmentType.CommitmentTypeCode,
                    CommitmentTypeName = vacationCommitmentType.CommitmentTypeName,
                    Precdence = vacationCommitmentType.Precedence,
                    ReportingPrecdence = vacationCommitmentType.ReportingPrecedence,
                    IsStaffingTag = vacationCommitmentType.IsStaffingTag
                }).ToList();

                commitments.AddRange(vacations);
            }

            if (timeOffsFromWorkday.Any())
            {

                var vacationCommitmentType = commitmentTypes.FirstOrDefault(ct => ct.CommitmentTypeCode == "V");

                var timeOffs = timeOffsFromWorkday.Select(item => new CommitmentViewModel
                {
                    EmployeeCode = item.EmployeeCode,
                    StartDate = (DateTime)item.StartDate,
                    EndDate = (DateTime)item.EndDate,
                    IsSourceStaffing = false,
                    IsOverridenInSource = false,
                    CommitmentTypeCode = vacationCommitmentType.CommitmentTypeCode,
                    CommitmentTypeName = vacationCommitmentType.CommitmentTypeName,
                    Precdence = vacationCommitmentType.Precedence,
                    ReportingPrecdence = vacationCommitmentType.ReportingPrecedence,
                    IsStaffingTag = vacationCommitmentType.IsStaffingTag
                }).ToList();

                commitments.AddRange(timeOffs);
            }

            if (officeHolidays.Any())
            {

                var holidayCommitmentType = commitmentTypes.FirstOrDefault(ct => ct.CommitmentTypeCode == "H");

                var holidays = officeHolidays.Select(item => new CommitmentViewModel
                {
                    EmployeeCode = item.EmployeeCode,
                    StartDate = (DateTime)item.StartDate,
                    EndDate = (DateTime)item.EndDate,
                    IsSourceStaffing = false,
                    IsOverridenInSource = false,
                    CommitmentTypeCode = holidayCommitmentType.CommitmentTypeCode,
                    CommitmentTypeName = holidayCommitmentType.CommitmentTypeName,
                    Precdence = holidayCommitmentType.Precedence,
                    ReportingPrecdence = holidayCommitmentType.ReportingPrecedence,
                    IsStaffingTag = holidayCommitmentType.IsStaffingTag
                }).ToList();

                commitments.AddRange(holidays);
            }

            return commitments;

        }

        private static List<ResourceCommitment> GetAggregatedCommitments(List<ResourceCommitment> resourcesCommitments)
        {
            var resourcesCommitmentsWithDateRange = new List<ResourceCommitment>();

            foreach (var groupedCommitments in resourcesCommitments.GroupBy(g => new { g.EmployeeCode, g.CommitmentTypeCodes }).Select(grp => grp.ToList().OrderBy(o => o.StartDate)))
            {
                var orderedGroupedCommitments = groupedCommitments.OrderBy(o => o.StartDate);
                var resourceCommitment = orderedGroupedCommitments.FirstOrDefault();
                var startDate = resourceCommitment.StartDate;
                var endDate = orderedGroupedCommitments.LastOrDefault().EndDate;
                var targetedDate = startDate;
                var splitRequired = false;
                foreach (var commitment in orderedGroupedCommitments)
                {
                    splitRequired = true;
                    if (targetedDate == commitment.StartDate)
                    {
                        targetedDate = targetedDate.AddDays(1);
                        splitRequired = false;
                        continue;
                    }
                    var clonedResourceCommitment = resourceCommitment.Clone();
                    clonedResourceCommitment.EndDate = targetedDate.AddDays(-1);
                    resourcesCommitmentsWithDateRange.Add(clonedResourceCommitment);
                    resourceCommitment.StartDate = commitment.StartDate;
                    targetedDate = commitment.StartDate.AddDays(1);
                }
                if (!splitRequired || resourcesCommitmentsWithDateRange.LastOrDefault()?.StartDate != resourceCommitment.StartDate)
                {
                    resourceCommitment.EndDate = endDate;
                    resourcesCommitmentsWithDateRange.Add(resourceCommitment);
                }

            }

            return resourcesCommitmentsWithDateRange;
        }

        private static List<ResourceCommitment> GetCommitmentsSplittedDayWise(IEnumerable<CommitmentViewModel> commitments)
        {
            var splittedCommitments = new List<CommitmentViewModel>();
            foreach (var commitment in commitments)
            {
                for (var date = commitment.StartDate; date <= commitment.EndDate; date = date.AddDays(1))
                {
                    var clonedCommitment = commitment.Clone();
                    clonedCommitment.StartDate = date;
                    clonedCommitment.EndDate = date;
                    splittedCommitments.Add(clonedCommitment);
                }
            }

            var resourcesCommitments = new List<ResourceCommitment>();

            foreach (var groupedCommitments in splittedCommitments.GroupBy(g => new { g.EmployeeCode, g.StartDate }).Select(grp => grp.ToList()))
            {
                var priorityCommitment = groupedCommitments.Where(g => !(bool)g.IsStaffingTag)?.OrderBy(c => c.ReportingPrecdence).FirstOrDefault();
                var priorityRingfence = groupedCommitments.Where(g => (bool)g.IsStaffingTag)?.OrderBy(c => c.ReportingPrecdence).FirstOrDefault();
                var commitmentTypeCodes = string.Join(",", groupedCommitments.Select(g => g.CommitmentTypeCode).Distinct().OrderBy(x => x).ToList());
                var resourceCommitment = new ResourceCommitment
                {
                    EmployeeCode = priorityCommitment?.EmployeeCode ?? priorityRingfence.EmployeeCode,
                    StartDate = priorityCommitment?.StartDate ?? priorityRingfence.StartDate,
                    EndDate = priorityCommitment?.EndDate ?? priorityRingfence.EndDate,
                    Allocation = priorityCommitment?.Allocation ?? priorityRingfence?.Allocation,
                    IsSourceStaffing = priorityCommitment?.IsSourceStaffing,
                    IsOverridenInSource = priorityCommitment?.IsOverridenInSource,
                    PriorityCommitmentTypeCode = priorityCommitment?.CommitmentTypeCode,
                    PriorityCommitmentTypeName = priorityCommitment?.CommitmentTypeName,
                    CommitmentTypeCodes = commitmentTypeCodes,
                    CommitmentTypeReasonCode = priorityCommitment?.CommitmentTypeReasonCode,
                    CommitmentTypeReasonName = priorityCommitment?.CommitmentTypeReasonName,
                    Ringfence = priorityRingfence?.CommitmentTypeName,
                    isStaffingTag = priorityRingfence?.IsStaffingTag
                };
                resourcesCommitments.Add(resourceCommitment);
            }

            return resourcesCommitments;
        }

        private List<ResourceAllocation> splitAllocationsForCommitments(List<ResourceAllocation> allocations, List<ResourceCommitment> resourcesCommitments)
        {
            var resourceAllocations = new List<ResourceAllocation>();
            foreach (var resourceCommitment in resourcesCommitments)
            {
                var allocationsOverlappedWithCommitment = allocations
                    .Where(x => x.EmployeeCode == resourceCommitment.EmployeeCode
                        && x.StartDate <= resourceCommitment.EndDate && x.EndDate >= resourceCommitment.StartDate);
                resourceAllocations.AddRange(allocations.Except(allocationsOverlappedWithCommitment));
                foreach (var allocation in allocationsOverlappedWithCommitment)
                {
                    updateAnlayticsDataForCommitment(resourceCommitment, resourceAllocations, allocation);
                }
                allocations = resourceAllocations;
                resourceAllocations = new List<ResourceAllocation>();
            }
            return allocations;
        }

        private void updateAnlayticsDataForCommitment(ResourceCommitment resourceCommitment, List<ResourceAllocation> resourceAllocations, ResourceAllocation allocation)
        {
            if (IsCommitmentOverlapsAllocationDateRange(resourceCommitment, allocation))
            {
                UpdateAllocationWithCommitment(resourceAllocations, resourceCommitment, allocation);
                return;
            }

            else if (IsCommitmentStartingBeforeAllocationDateRange(resourceCommitment, allocation))
            {
                var commitmentStartedBeforeAllocation = resourceCommitment;

                var startDate = allocation.StartDate;
                var endDate = commitmentStartedBeforeAllocation.EndDate.Date;

                var clonedResourceAllocation =
                    CloneResourceAllocation(allocation, startDate, endDate);

                UpdateAllocationWithCommitment(resourceAllocations, resourceCommitment, clonedResourceAllocation);

                if (endDate < allocation.EndDate)
                {
                    clonedResourceAllocation =
                        CloneResourceAllocation(allocation, commitmentStartedBeforeAllocation.EndDate.Date.AddDays(1), allocation.EndDate);
                    resourceAllocations.Add(clonedResourceAllocation);
                }
            }
            else if (IsCommitmentEndingInAllocationDateRange(resourceCommitment, allocation))
            {
                var startDate = allocation.StartDate;
                var endDate = resourceCommitment.EndDate.Date;

                var clonedResourceAllocation =
                    CloneResourceAllocation(allocation, startDate, endDate);

                UpdateAllocationWithCommitment(resourceAllocations, resourceCommitment, clonedResourceAllocation);

                clonedResourceAllocation =
                    CloneResourceAllocation(allocation, resourceCommitment.EndDate.Date.AddDays(1), allocation.EndDate);
                resourceAllocations.Add(clonedResourceAllocation);

            }

            else if (IsCommitmentLiesInAllocationDateRange(resourceCommitment, allocation))
            {
                UpdateAllocationWithCommitmentLiesInAllocationDateRange(resourceAllocations, allocation, resourceCommitment);
            }

            else if (IsCommitmentStartedInAllocationDateRange(resourceCommitment, allocation))
            {
                var commitment = resourceCommitment;

                var startDate = allocation.StartDate;
                var endDate = commitment.StartDate.Date.AddDays(-1);
                var allocationNotCoveredByCommitment =
                    CloneResourceAllocation(allocation, startDate, endDate);
                resourceAllocations.Add(allocationNotCoveredByCommitment);

                var clonedResourceAllocation =
                    CloneResourceAllocation(allocation, commitment.StartDate.Date, allocation.EndDate);

                UpdateAllocationWithCommitment(resourceAllocations, resourceCommitment, clonedResourceAllocation);

            }
        }

        private static void UpdateAllocationWithCommitment(IList<ResourceAllocation> resourceAllocations,
            ResourceCommitment resourceCommitment, ResourceAllocation allocation)
        {
            allocation.PriorityCommitmentTypeCode = resourceCommitment.PriorityCommitmentTypeCode;
            allocation.PriorityCommitmentTypeName = resourceCommitment.PriorityCommitmentTypeName;
            allocation.CommitmentTypeCodes = resourceCommitment.CommitmentTypeCodes;
            allocation.CommitmentTypeReasonCode = resourceCommitment.CommitmentTypeReasonCode;
            allocation.CommitmentTypeReasonName = resourceCommitment.CommitmentTypeReasonName;
            allocation.isOverriddenInSource = resourceCommitment.IsOverridenInSource;
            allocation.Ringfence = resourceCommitment.Ringfence;
            allocation.isStaffingTag = resourceCommitment.isStaffingTag;
            resourceAllocations.Add(allocation);
        }

        private async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationWithDetails(string scheduleIds)
        {
            if (string.IsNullOrEmpty(scheduleIds))
            {
                return Enumerable.Empty<ResourceAllocation>();
            }

            var distinctScheduleIds = scheduleIds.Split(",").Select(r => r.ToString().ToLower()).Distinct();

            var resourceAllocations = await _staffingApiClient.GetResourceAllocationsByScheduleIds(string.Join(",", distinctScheduleIds));
            var scheduleIdsExistsInDB = resourceAllocations.Select(r => r.Id.ToString().ToLower()).Distinct();

            var scheduleIdsToDelete = distinctScheduleIds.Except(scheduleIdsExistsInDB);

            var casesTask = Task.FromResult<IEnumerable<CaseViewModel>>(new List<CaseViewModel>());
            var opportunitiesTask = Task.FromResult<IEnumerable<OpportunityDetailsViewModel>>(new List<OpportunityDetailsViewModel>());
            var employeesTask = Task.FromResult<IEnumerable<Resource>>(new List<Resource>());
            var caseRoleTypesTask = Task.FromResult<IEnumerable<CaseRoleType>>(new List<CaseRoleType>());
            var investmentCategoriesTask = Task.FromResult<IEnumerable<InvestmentCategory>>(new List<InvestmentCategory>());
            var serviceLinesTask = Task.FromResult<IEnumerable<ServiceLine>>(new List<ServiceLine>());
            var ccmOfficesTask = Task.FromResult<IEnumerable<Models.Office>>(new List<Models.Office>());
            var deletedAllocationsTask = Task.CompletedTask;

            if (resourceAllocations.Any())
            {

                var employeeCodes = string.Join(",", resourceAllocations.Select(r => r.EmployeeCode).Distinct());
                var oldCaseCodes = string.Join(",", resourceAllocations.Select(r => r.OldCaseCode).Distinct());
                var pipelineIds = string.Join(",", resourceAllocations.Select(r => r.PipelineId).Distinct());
                casesTask = _ccmApiClient.GetCaseDetailsByCaseCodes(oldCaseCodes);
                opportunitiesTask = _pipelineApiClient.GetOpportunityDetailsByPipelineIds(pipelineIds);
                employeesTask = _resourceApiClient.GetEmployeesIncludingTerminated();
                caseRoleTypesTask = _staffingApiClient.GetCaseRoleTypeLookupList();
                investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryLookupList();
                serviceLinesTask = _resourceApiClient.GetServiceLineList();
                ccmOfficesTask = _ccmApiClient.GetOfficeList();
            }

            if (scheduleIdsToDelete.Any())
            {
                deletedAllocationsTask = DeleteAnalyticsDataForDeletedAllocationByScheduleIds(string.Join(",", scheduleIdsToDelete));
            }

            await Task.WhenAll(casesTask, opportunitiesTask, employeesTask, caseRoleTypesTask,
                investmentCategoriesTask, serviceLinesTask, ccmOfficesTask, deletedAllocationsTask);

            if (!resourceAllocations.Any())
            {
                return Enumerable.Empty<ResourceAllocation>();
            }

            var cases = casesTask.Result ?? Enumerable.Empty<CaseViewModel>();
            var opportunities = opportunitiesTask.Result ?? Enumerable.Empty<OpportunityDetailsViewModel>();
            var employees = employeesTask.Result ?? Enumerable.Empty<Resource>();
            var caseRoleTypes = caseRoleTypesTask.Result ?? Enumerable.Empty<CaseRoleType>();
            var investmentCategories = investmentCategoriesTask.Result ?? Enumerable.Empty<InvestmentCategory>();
            var serviceLines = serviceLinesTask.Result ?? Enumerable.Empty<ServiceLine>();
            var offices = ccmOfficesTask.Result ?? Enumerable.Empty<Models.Office>();

            var resourceAllocationsMappedToCasesAndOpportunities = (from allocation in resourceAllocations
                                                                    join c in cases on allocation.OldCaseCode equals c.OldCaseCode into allocCaseGroups
                                                                    from caseItem in allocCaseGroups.DefaultIfEmpty()
                                                                    join opp in opportunities on allocation.PipelineId equals opp.PipelineId into allocOppGroups
                                                                    from opportunityItem in allocOppGroups.DefaultIfEmpty()
                                                                    join emp in employees on allocation.EmployeeCode equals emp.EmployeeCode into allocEmployeeGroups
                                                                    from resource in allocEmployeeGroups.DefaultIfEmpty()
                                                                    select new ResourceAllocation
                                                                    {
                                                                        // Common to case and opp
                                                                        ClientCode = caseItem?.ClientCode ?? opportunityItem.ClientCode,
                                                                        ClientName = caseItem?.ClientName ?? opportunityItem.ClientName,
                                                                        ClientGroupCode = caseItem?.ClientGroupCode ?? opportunityItem.ClientGroupCode,
                                                                        ClientGroupName = caseItem?.ClientGroupName ?? opportunityItem.ClientGroupName,
                                                                        // Opp related info
                                                                        PipelineId = opportunityItem?.PipelineId,
                                                                        OpportunityName = opportunityItem?.OpportunityName,
                                                                        // Case related info
                                                                        OldCaseCode = caseItem?.OldCaseCode,
                                                                        CaseCode = caseItem?.CaseCode,
                                                                        CaseName = caseItem?.CaseName,
                                                                        CaseTypeCode = caseItem?.CaseTypeCode,
                                                                        CaseTypeName = caseItem?.CaseType,
                                                                        ManagingOfficeCode = caseItem?.ManagingOfficeCode,
                                                                        ManagingOfficeAbbreviation = caseItem?.ManagingOfficeAbbreviation,
                                                                        ManagingOfficeName = caseItem?.ManagingOfficeName,
                                                                        BillingOfficeCode = caseItem?.BillingOfficeCode,
                                                                        BillingOfficeAbbreviation = caseItem?.BillingOfficeAbbreviation,
                                                                        BillingOfficeName = caseItem?.BillingOfficeName,
                                                                        PegCase = caseItem?.CaseServedByRingfence,
                                                                        PrimaryIndustryTermCode = caseItem?.PrimaryIndustryTermCode,
                                                                        PrimaryIndustryTagId = caseItem?.PrimaryIndustryTagId,
                                                                        PrimaryIndustry = caseItem?.PrimaryIndustry,
                                                                        PracticeAreaIndustryCode = caseItem?.IndustryPracticeAreaCode,
                                                                        PracticeAreaIndustry = caseItem?.IndustryPracticeArea,
                                                                        PrimaryCapabilityTermCode = caseItem?.PrimaryCapabilityTermCode,
                                                                        PrimaryCapabilityTagId = caseItem?.PrimaryCapabilityTagId,
                                                                        PrimaryCapability = caseItem?.PrimaryCapability,
                                                                        PracticeAreaCapabilityCode = caseItem?.CapabilityPracticeAreaCode,
                                                                        PracticeAreaCapability = caseItem?.CapabilityPracticeArea,
                                                                        // Employee related info
                                                                        EmployeeCode = allocation.EmployeeCode,
                                                                        EmployeeName = resource?.FullName,
                                                                        EmployeeStatusCode = Constants.EmployeeStatus.Active,
                                                                        Fte = (decimal)resource?.Fte,
                                                                        PositionCode = resource?.Position?.PositionCode,
                                                                        PositionName = resource?.Position?.PositionName,
                                                                        PositionGroupName = resource?.Position?.PositionGroupName,
                                                                        BillCode = (decimal)resource?.BillCode,
                                                                        HireDate = (DateTime)resource?.StartDate,
                                                                        TerminationDate = resource?.TerminationDate,
                                                                        InternetAddress = resource?.InternetAddress,
                                                                        CurrentLevelGrade = allocation.CurrentLevelGrade,
                                                                        OperatingOfficeCode = (int)allocation.OperatingOfficeCode,
                                                                        OperatingOfficeAbbreviation = offices.FirstOrDefault(o => o.OfficeCode == allocation.OperatingOfficeCode)?.OfficeAbbreviation,
                                                                        OperatingOfficeName = offices.FirstOrDefault(o => o.OfficeCode == allocation.OperatingOfficeCode)?.OfficeName,
                                                                        ServiceLineCode = allocation.ServiceLineCode,
                                                                        ServiceLineName = serviceLines.FirstOrDefault(s => s.ServiceLineCode == allocation.ServiceLineCode)?.ServiceLineName,
                                                                        // Staffing related info
                                                                        Id = (Guid)allocation.Id,
                                                                        Allocation = (int)allocation.Allocation,
                                                                        StartDate = (DateTime)allocation.StartDate,
                                                                        EndDate = (DateTime)allocation.EndDate,
                                                                        InvestmentCode = allocation.InvestmentCode,
                                                                        InvestmentName = investmentCategories.FirstOrDefault(ic => ic.InvestmentCode == allocation.InvestmentCode)?.InvestmentName,
                                                                        CaseRoleCode = allocation.CaseRoleCode,
                                                                        CaseRoleName = caseRoleTypes.FirstOrDefault(crt => crt.CaseRoleCode == allocation.CaseRoleCode)?.CaseRoleName,
                                                                        Notes = allocation.Notes,
                                                                        LastUpdatedBy = allocation.LastUpdatedBy,
                                                                    }).ToList();

            return resourceAllocationsMappedToCasesAndOpportunities;


        }

        public async Task SplitAllocationsForPendingTransactionOrLoA(IEnumerable<ResourceAllocation> resourceAllocations, List<ResourceAllocation> allocations, string employeeCodes)
        {
            /* 
            * Always get pending transactions because user can allocate resource to a case starting after effective date of pending transactions 
            * Example: User "Bob" is having level grade change M8 --> M9 on 01-Jun-2020.
            * On 25-May-2020 , Bob (current level grade M8) is assinged on a case from 01-jul-2020
            * In this case anlytics data should contains point-in time information i.e. M9 level grade
           */
            var employeesPendingStaffingTransactions =
                await _resourceApiClient.GetPendingTransactionsByEmployeeCodes(employeeCodes);

            var allLoAs =
                await _resourceApiClient.GetEmployeesLoATransactions(employeeCodes);
            foreach (var resourceAllocation in resourceAllocations)
            {
                var AllocationSplitters = new List<ResourceAllocation>();
                var resourcePendingTransactions = employeesPendingStaffingTransactions
                    .Where(e => e.EmployeeCode == resourceAllocation.EmployeeCode).ToList();
                AllocationSplitters.AddRange(
                    resourcePendingTransactions.Any()
                        ? SplitAllocationForpendingTransactions(resourceAllocation, resourcePendingTransactions)
                        : Enumerable.Repeat(resourceAllocation, 1));

                var resourceLoAs = allLoAs
                    .Where(x => x.EmployeeCode == resourceAllocation.EmployeeCode
                        && x.StartDate <= resourceAllocation.EndDate
                        && x.EndDate >= resourceAllocation.StartDate
                    );

                allocations.AddRange(resourceLoAs.Any()
                    ? SplitAllocationForLoAs(AllocationSplitters, resourceLoAs)
                    : AllocationSplitters);
            }
        }

        public async Task SplitAllocationsForHistoricalTransactionOrLevelGradeChange(IEnumerable<ResourceAllocation> resourceAllocations, List<ResourceAllocation> allocations, string employeeCodes)
        {
            var allTransactions =
                await _resourceApiClient.GetEmployeesStaffingTransactions(employeeCodes);

            var activeEmployees = await _resourceApiClient.GetEmployees();

            /* 
            * Get user point in time info as per allocation date range
            * Example: User "Bob" is assinged on a case for duratin 01-Mar-2020 to 30-Jun-2020
            * Today is 25-May-2020 and Bob got promoted (M8 --> M9) on 01-Apr-2020 i.e. current level grade is M9
            * Stafing user updated Bob's asignment, say, end date updated to 25-Jun-2020.
            * In this case analytics data should contain point-in time info i.e. M8 for duration 01-Mar-2020 to 31-mar-2020
            * and M9 for the rest of duration (01-Apr-2020 to 25-Jun-2020)
           */
            foreach (var allocation in resourceAllocations)
            {
                var employeeAllTransactions = allTransactions.Where(x => x.EmployeeCode == allocation.EmployeeCode);
                List<EmployeeTransaction> transactions = GetEmployeeTransactionsBetweenAllocationDateRange(employeeAllTransactions, allocation);

                // In case user's level grade as of today is different than level grade at the time of allocation
                // Analytics data needs to be splitted
                var employee = activeEmployees.FirstOrDefault(x => x.EmployeeCode == allocation.EmployeeCode);

                if (employee?.LevelGrade != allocation.CurrentLevelGrade)
                {
                    var PDGradeTransactionEffectiveNearAllocationDateRange = employeeAllTransactions
                        .Where(x =>
                            x.TransactionStatus == "Successfully Completed"
                            && x.Transaction.PdGradeCurrent != x.Transaction.PdGradeProposed
                            &&
                            (
                                (x.EffectiveDate.Value.Date >= allocation.StartDate &&
                                x.EffectiveDate.Value.Date <= allocation.EndDate
                                )
                                || x.EffectiveDate.Value.Date <= allocation.StartDate
                            ))
                        .OrderBy(o => o.EffectiveDate)
                        .ThenByDescending(o => o.LastModifiedDate).LastOrDefault();

                    if (allocation.StartDate >= PDGradeTransactionEffectiveNearAllocationDateRange?.EffectiveDate)
                    {
                        allocation.CurrentLevelGrade = PDGradeTransactionEffectiveNearAllocationDateRange.Transaction?.PdGradeProposed ?? PDGradeTransactionEffectiveNearAllocationDateRange.Transaction?.PdGradeCurrent;
                    }
                }

                if (!transactions.Any())
                {
                    var transactionBeforeAllocationStartDate = employeeAllTransactions?
                            .Where(x => x.TransactionStatus == "Successfully Completed"
                                    && x.EffectiveDate.Value.Date < allocation.StartDate)
                            .OrderBy(o => o.EffectiveDate)
                            .ThenBy(o => o.LastModifiedDate)
                            .LastOrDefault();
                    if (transactionBeforeAllocationStartDate != null)
                    {
                        allocation.BillCode = transactionBeforeAllocationStartDate.Transaction.BillCodeProposed;
                    }
                    allocations.Add(allocation);
                    continue;
                };

                var allocationSplitter =
                    SplitAllocationForHistoricalTransactions(allocation, transactions);
                allocations.AddRange(allocationSplitter);
            }
        }

        //public async Task UpsertAvailabilityData(string employeeCodes)
        //{
        //    await _analyticsRepository.UpsertAvailabilityData(employeeCodes);
        //}

        public async Task UpsertAvailabilityDataBetweenDateRange(IEnumerable<AvailabilityDateRange> availabilityDateRangeForEmployees)
        {
            var dataTable = CreateEmployeeAvailabilityDateRangeDataTable(availabilityDateRangeForEmployees);
            await _analyticsRepository.UpsertAvailabilityDataBetweenDateRange(dataTable);
        }

        public async Task<IEnumerable<ResourceAvailability>> UpdateCostForResourcesAvailableInFullCapacity(
            string employeeCodes = null)
        {
            var resourcesFullAvailability =
                await _resourceAllocationRepository.GetResourcesFullAvailabilityDateRange(employeeCodes);

            if (resourcesFullAvailability.ToList().Count < 1) return Enumerable.Empty<ResourceAvailability>();

            var employees = await _resourceApiClient.GetEmployeesIncludingTerminated();

            //var resourcesFullAvailabilityWithHireDate = GetRecordsWithUpdatedResourceData(resourcesFullAvailability, employees);

            var totalRecords = resourcesFullAvailability.Count();
            var batchSize = 100;
            var skipRecords = 0;

            var availabilityModel = new List<ResourceAvailability>();

            while (skipRecords < totalRecords)
            {
                var resourcesFullAvailabilityBatch = resourcesFullAvailability.OrderBy(r => r.EmployeeCode).Skip(skipRecords).Take(batchSize);

                var employeesAllTransactions = await
                    _resourceApiClient.GetEmployeesStaffingTransactions(string.Join(",", resourcesFullAvailabilityBatch.Select(x => x.EmployeeCode).Distinct()));

                var employeesWithFirstHireTransactions = employeesAllTransactions
                                .Where(x => x.TransactionStatus == "Successfully Completed"
                                        && x.BusinessProcessName == "Hire"
                                        && !x.BusinessProcessReason.Contains("Pre-Hire Bonus Pay Out"))
                                .GroupBy(comparer => comparer.EmployeeCode)
                                .Select(grp => grp.OrderBy(y => y.EffectiveDate)
                                                .ThenBy(o => o.LastModifiedDate)
                                                .FirstOrDefault());

                var resourcesFullAvailabilityWithHireDateBatch = GetRecordsWithUpdatedResourceData(resourcesFullAvailabilityBatch, employees, employeesWithFirstHireTransactions);

                var employeesStaffingTransactions = new List<EmployeeTransaction>();

                // Get employee staffing trasnactions happen for the duration employee is available
                foreach (var resource in resourcesFullAvailabilityWithHireDateBatch)
                {
                    var employeeAllTransactions = employeesAllTransactions.Where(x => x.EmployeeCode == resource.EmployeeCode);
                    List<EmployeeTransaction> transactions = GetEmployeeTransactionsBetweenAllocationDateRange(employeeAllTransactions, resource);

                    if (!transactions.Any())
                    {
                        var transactionBeforeAllocationStartDate = employeeAllTransactions?
                                .Where(x => x.TransactionStatus == "Successfully Completed"
                                        && x.EffectiveDate.Value.Date < resource.StartDate)
                                .OrderBy(o => o.EffectiveDate)
                                .ThenBy(o => o.LastModifiedDate)
                                .LastOrDefault();

                        if (transactionBeforeAllocationStartDate == null)
                        {
                            var transactionForHire = employeeAllTransactions
                                    .Where(x => x.BusinessProcessName == "Hire"
                                        && x.TransactionStatus == "Successfully Completed"
                                        && !x.BusinessProcessReason.Contains("Pre-Hire Bonus Pay Out")
                                        && x.EffectiveDate.Value.Date <= resource.EndDate)
                                    .OrderByDescending(x => x.EffectiveDate)
                                    .ThenByDescending(o => o.LastModifiedDate).FirstOrDefault();

                            if (transactionForHire == null) continue;

                            if (transactionForHire.Transaction != null
                                && transactionForHire.Transaction.PdGradeCurrent == null
                                && transactionForHire.Transaction.PdGradeProposed == null)
                            {
                                transactionForHire.Transaction.PdGradeProposed = resource.CurrentLevelGrade;
                            }

                            resource.StartDate = transactionForHire.EffectiveDate.Value.Date;
                            employeesStaffingTransactions.Add(transactionForHire);
                            continue;
                        }
                        else
                        {
                            employeesStaffingTransactions.Add(transactionBeforeAllocationStartDate);
                        }
                    }
                    else
                    {
                        employeesStaffingTransactions.AddRange(transactions);
                    }
                }

                var resourceAvailabilityData = new List<ResourceAllocation>();
                foreach (var resource in resourcesFullAvailabilityWithHireDateBatch.ToList())
                {
                    var employeeJobChanges = employeesStaffingTransactions?.Where(x => x?.EmployeeCode == resource.EmployeeCode)
                        .GroupBy(x => x.EffectiveDate.Value.Date)
                        .Select(grp => grp.OrderByDescending(x => x.LastModifiedDate)
                                        .FirstOrDefault());

                    if (employeeJobChanges?.Count() > 0)
                    {
                        var resourceInfo = GetResourcePointInTimeInfo(employeeJobChanges, resource);
                        resourceAvailabilityData.AddRange(resourceInfo);
                    }
                    else
                    {
                        resource.LastUpdatedBy = "Enqueue Job";
                        resourceAvailabilityData.Add(resource);
                    }
                }

                if (resourceAvailabilityData.Count() < 1) return Enumerable.Empty<ResourceAvailability>();

                var resourceAvailabilityUpdatedByCommitments = await UpdateCommitmentsInAllocations(resourceAvailabilityData);
                var resourceAvailabilityWithCost = await GetResourcesAllocationsWithBillRate(resourceAvailabilityUpdatedByCommitments);
                var recordsWithBillRateUpdatedByLoA = await GetRecordsWithLoAUpdated(resourceAvailabilityWithCost);

                availabilityModel = ConvertToResourceAvailability(recordsWithBillRateUpdatedByLoA).ToList();
                var resourceAvailabilityWithCostDto = CreateAvailabilityDataTable(availabilityModel);
                await _resourceAvailabilityRepository.UpdateCostForResourcesAvailableInFullCapacity(
                    resourceAvailabilityWithCostDto);

                skipRecords += batchSize;
            }

            /**
            * Trigger job to update records in capacity analysis daily table
            * fot the changes happen in SMD and RA
            * */
            //_backgroundJobClient.Enqueue<IAnalyticsService>(x => x.UpsertCapacityAnalysisDaily(false, null));

            return availabilityModel.Take(1);
        }

        public async Task UpdateAnlayticsDataForUpsertedCommitment(string commitmentIds)
        {
            var distinctCommitmentIds = commitmentIds.Split(",").Select(r => r.ToString().ToLower()).Distinct();

            var commitments = await _staffingApiClient.GetResourceCommitmentsByCommitmentIds(commitmentIds);
            var commitmentIdsExistsInDB = commitments.Select(r => r.Id.ToString().ToLower()).Distinct();
            var deletedCommitmentIds = distinctCommitmentIds.Except(commitmentIdsExistsInDB);

            if (commitments.Any())
            {
                var employeeCodes = string.Join(",", commitments.Select(c => c.EmployeeCode).Distinct());
                var minCommitmentStartDate = commitments.Min(c => c.StartDate);
                var maxCommitmentEndDate = commitments.Max(c => c.EndDate);

                var employeesCommitments = await GetCommitmentsFromBOSSAndExternalSystems(employeeCodes, minCommitmentStartDate, maxCommitmentEndDate);

                var splittedCommitments = GetCommitmentsSplittedDayWise(employeesCommitments);
                var splittedCommitmentsWithinMinMaxDates = splittedCommitments.Where(x => x.StartDate <= maxCommitmentEndDate && x.EndDate >= minCommitmentStartDate).ToList();
                var aggregatedCommitments = GetAggregatedCommitments(splittedCommitmentsWithinMinMaxDates);

                var resourceCommitmentDataTable = ConvertResourceCommitmentsToDataTable(aggregatedCommitments);
                await _analyticsRepository.UpdateAnalyticsDataForCommitments(resourceCommitmentDataTable);

            }

            if (deletedCommitmentIds.Any())
            {
                await UpdateAnlayticsDataForDeletedCommitment(string.Join(",", deletedCommitmentIds));
            }

        }

        public async Task UpdateAnlayticsDataForUpsertedExternalCommitment(DateTime? updatedAfter)
        {
            var processName = Constants.ExternalCommitments;
            var lastTimePolled = updatedAfter.HasValue
                ? updatedAfter.Value
                : await _pollMasterRepository.GetLastPolledTimeStamp(processName);

            var externalCommitments = await _analyticsRepository.GetExternalCommitments(lastTimePolled);
            if (!externalCommitments.Any())
            {
                return;
            }
            var employeeCodes = string.Join(",", externalCommitments.Select(x => x.EmployeeCode).Distinct());
            var fromDate = externalCommitments.Min(x => x.StartDate);
            var toDate = externalCommitments.Max(x => x.EndDate);
            var pollTimestampToUpdate = externalCommitments.Max(x => x.LastUpdated);

            var minFromDate = Convert.ToDateTime(ConfigurationUtility.GetValue("AnalyticsMinDate"));
            if (fromDate < minFromDate)
            {
                fromDate = minFromDate;
            }
            if (toDate < fromDate)
            {
                toDate = fromDate;
            }

            // Get the commitments to recalculate priority commitments
            var employeesCommitments = await GetCommitmentsFromBOSSAndExternalSystems(employeeCodes, fromDate, toDate);

            var splittedCommitments = GetCommitmentsSplittedDayWise(employeesCommitments);
            var splittedCommitmentsWithinMinMaxDates = splittedCommitments.Where(x => x.StartDate <= toDate && x.EndDate >= fromDate).ToList();
            var aggregatedCommitments = GetAggregatedCommitments(splittedCommitmentsWithinMinMaxDates);

            var resourceCommitmentDataTable = ConvertResourceCommitmentsToDataTable(aggregatedCommitments);
            await _analyticsRepository.UpdateAnalyticsDataForCommitments(resourceCommitmentDataTable);

            await _pollMasterRepository.UpsertPollMaster(processName, pollTimestampToUpdate.Value);
        }

        public async Task UpdateAnlayticsDataForDeletedCommitment(string commitmentids)
        {
            var deletedCommitments = await _staffingApiClient.GetResourceCommitmentsByDeletedCommitmentIds(commitmentids);
            var employeeCodes = string.Join(",", deletedCommitments.Select(c => c.EmployeeCode).Distinct());

            var fromDate = deletedCommitments.Min(x => x.StartDate);// Convert.ToDateTime(ConfigurationUtility.GetValue("AnalyticsMinDate"));
            var toDate = deletedCommitments.Max(x => x.EndDate);
            var commitments = await GetCommitmentsFromBOSSAndExternalSystems(employeeCodes, fromDate, toDate);

            var splittedCommitments = GetCommitmentsSplittedDayWise(commitments);
            var splittedCommitmentsWithinMinMaxDates = splittedCommitments.Where(x => x.StartDate <= toDate && x.EndDate >= fromDate).ToList();
            var aggregatedCommitments = GetAggregatedCommitments(splittedCommitmentsWithinMinMaxDates);
            var resourceCommitmentDataTable = ConvertResourceCommitmentsToDataTable(aggregatedCommitments);
            await _analyticsRepository.UpdateAnalyticsDataForCommitments(resourceCommitmentDataTable);

            /**
            * Trigger job to update records in capacity analysis daily table
            * fot the changes happen in SMD and RA
            * */
            //_backgroundJobClient.Enqueue<IAnalyticsService>(x => x.UpsertCapacityAnalysisDaily(false, null));
        }

        private DataTable ConvertResourceCommitmentsToDataTable(List<ResourceCommitment> resourceCommitments)
        {
            var resourceCommitmentDataTable = new DataTable();
            resourceCommitmentDataTable.Columns.Add("employeeCode", typeof(string));
            resourceCommitmentDataTable.Columns.Add("startDate", typeof(DateTime));
            resourceCommitmentDataTable.Columns.Add("endDate", typeof(DateTime));
            resourceCommitmentDataTable.Columns.Add("allocation", typeof(Int16));
            resourceCommitmentDataTable.Columns.Add("priorityCommitmentTypeCode", typeof(string));
            resourceCommitmentDataTable.Columns.Add("priorityCommitmentTypeName", typeof(string));
            resourceCommitmentDataTable.Columns.Add("commitmentTypeCodes", typeof(string));
            resourceCommitmentDataTable.Columns.Add("commitmentTypeReasonCode", typeof(string));
            resourceCommitmentDataTable.Columns.Add("commitmentTypeReasonName", typeof(string));
            resourceCommitmentDataTable.Columns.Add("ringfence", typeof(string));
            resourceCommitmentDataTable.Columns.Add("isStaffingTag", typeof(bool));
            resourceCommitmentDataTable.Columns.Add("isOverriddenInSource", typeof(bool));
            resourceCommitmentDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var resourceCommitment in resourceCommitments)
            {
                var row = resourceCommitmentDataTable.NewRow();

                row["employeeCode"] = (object)resourceCommitment.EmployeeCode ?? DBNull.Value;
                row["startDate"] = (object)resourceCommitment.StartDate.Date ?? DBNull.Value;
                row["endDate"] = (object)resourceCommitment.EndDate.Date ?? DBNull.Value;
                row["allocation"] = (object)resourceCommitment.Allocation ?? DBNull.Value;
                row["priorityCommitmentTypeCode"] = (object)resourceCommitment.PriorityCommitmentTypeCode ?? DBNull.Value;
                row["priorityCommitmentTypeName"] = (object)resourceCommitment.PriorityCommitmentTypeName ?? DBNull.Value;
                row["commitmentTypeCodes"] = (object)resourceCommitment.CommitmentTypeCodes ?? DBNull.Value;
                row["commitmentTypeReasonCode"] = (object)resourceCommitment.CommitmentTypeReasonCode ?? DBNull.Value;
                row["commitmentTypeReasonName"] = (object)resourceCommitment.CommitmentTypeReasonName ?? DBNull.Value;
                row["ringfence"] = (object)resourceCommitment.Ringfence ?? DBNull.Value;
                row["isStaffingTag"] = (object)resourceCommitment.isStaffingTag ?? DBNull.Value;
                row["isOverriddenInSource"] = (object)resourceCommitment.IsOverridenInSource ?? DBNull.Value;
                row["lastUpdatedBy"] = "Boss-Commitment";

                resourceCommitmentDataTable.Rows.Add(row);
            }

            return resourceCommitmentDataTable;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourcesAllocationsWithBillRate(
            IEnumerable<ResourceAllocation> resourcesAllocations)
        {
            var officeCodes = string.Join(",", resourcesAllocations.Select(pt => pt.OperatingOfficeCode).Distinct());
            var billRates = await _ccmApiClient.GetBillRateByOffices(officeCodes);

            var resourcesAllocationsWithBillRate = GetAllocationsDividedByBillRate(billRates, resourcesAllocations);

            var resourcesAllocationsWithBillRateWithUSDConversion = await ConvertCostToUSD(resourcesAllocationsWithBillRate);

            return resourcesAllocationsWithBillRateWithUSDConversion;
        }

        private async Task<IEnumerable<ResourceAllocation>> ConvertCostToUSD(IEnumerable<ResourceAllocation> resourcesAllocations)
        {
            var currencyCodes = string.Join(",", resourcesAllocations.Select(ra => ra.BillRateCurrency)?.Distinct());
            var currencyRateTypeCodes = "B";
            var effectiveFromDate = resourcesAllocations.Min(ra => ra.StartDate);
            var effectiveToDate = resourcesAllocations.Max(Random => Random.EndDate);
            if (string.IsNullOrEmpty(currencyCodes))
                return resourcesAllocations;

            var curencyRates = await _basisApiClient.GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(currencyCodes, currencyRateTypeCodes,
                new DateTime(effectiveFromDate.Year, 1, 1), new DateTime(effectiveToDate.Value.Year, 1, 1));
            var resourcesAllocationsWithUSDCost = resourcesAllocations.Select(r =>
            {
                var currencyRate = curencyRates.FirstOrDefault(c => c.CurrencyCode.Trim() == r.BillRateCurrency?.Trim()
                && c.EffectiveDate.Year == r.StartDate.Year);
                r.CostInUSD = r.ActualCost * currencyRate?.UsdRate;
                r.CostUSDEffectiveYear = currencyRate?.EffectiveDate.Year;
                r.UsdRate = currencyRate?.UsdRate;
                return r;
            }).ToList();
            return resourcesAllocationsWithUSDCost;
        }

        public async Task<Guid> DeleteAnalyticsDataForDeletedAllocationByScheduleId(Guid deletedAllocationId)
        {
            var employeeCode = await _resourceAllocationRepository.DeleteAnalyticsDataByScheduleId(deletedAllocationId);

            _backgroundJobClient.Enqueue<IAnalyticsService>(x =>
               x.UpdateCostForResourcesAvailableInFullCapacity(employeeCode));

            return deletedAllocationId;
        }

        public async Task DeleteAnalyticsDataForDeletedAllocationByScheduleIds(string deletedAllocationIds)
        {
            var employees = await _resourceAllocationRepository.DeleteAnalyticsDataByScheduleIds(deletedAllocationIds);
            var employeeCodes = string.Join(",", employees);

            _backgroundJobClient.Enqueue<IAnalyticsService>(x =>
               x.UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));

            return;
        }

        public async Task<List<ResourceAvailabilityViewModel>> InsertDailyAvailabilityTillNextYearForAll(string employeeCodes = null)
        {
            var resourcesWithNoAvailabilityRecords =
                await UpdateAvailabilityDataForResourcesWithNoAvailabilityRecords(employeeCodes);
            await InsertAvailabilityTillNextYearForAllActiveEmployees();

            /**
            * Trigger job to update records in capacity analysis daily table
            * fot the changes happen in SMD and RA
            * */
            //_backgroundJobClient.Enqueue<IAnalyticsService>(x => x.UpsertCapacityAnalysisDaily(false, null));

            return resourcesWithNoAvailabilityRecords;
        }

        //TODO: Delete after 2019 data population or keep for future data population between date ranges
        //NOTE: This is a 1 time job for data populatio.
        // It should be run after recurring UpdateAvailabilityDataForResourcesWithNoAvailabilityRecords job
        public async Task<List<ResourceAvailabilityViewModel>> InsertAvailabilityDataForForResourcesWithNoDataBetweenDateRange(string employeeCodes = null)
        {
            var minAvailabilityStartDate = Convert.ToDateTime("01-01-2019").Date;
            var maxAvailabilityEndDate = Convert.ToDateTime("2019-12-31").Date;

            var allEmployeesActiveIn2019 = await _resourceApiClient.GetEmployeesIncludingTerminated();

            //allEmployees who were either active in 2019 or terminated in 2019
            allEmployeesActiveIn2019 = allEmployeesActiveIn2019.Where(x => x.StartDate <= maxAvailabilityEndDate && (x.TerminationDate == null || x.TerminationDate >= minAvailabilityStartDate));

            if (string.IsNullOrEmpty(employeeCodes))
            {
                employeeCodes = string.Join(',', allEmployeesActiveIn2019.Select(x => x.EmployeeCode).ToList());
            }

            var employeeCodesWithNoAvailabilityRecords = await _resourceAllocationRepository.GetResourcesWithNoAvailabilityRecordsBetweenDateRange(employeeCodes);

            var resourcesWithNoAvailabilityRecords = (from employee in allEmployeesActiveIn2019
                                                      join employeeCode in employeeCodesWithNoAvailabilityRecords
                                                          on employee.EmployeeCode equals employeeCode
                                                      select new ResourceAvailabilityViewModel
                                                      {
                                                          EmployeeCode = employee.EmployeeCode,
                                                          EmployeeStatusCode = Constants.EmployeeStatus.Active,
                                                          EmployeeName = employee.FullName,
                                                          Fte = employee.Fte,
                                                          OperatingOfficeCode = employee.Office.OfficeCode,
                                                          OperatingOfficeName = employee.Office.OfficeName,
                                                          OperatingOfficeAbbreviation = employee.Office.OfficeAbbreviation,
                                                          CurrentLevelGrade = employee.LevelGrade,
                                                          ServiceLineCode = employee.ServiceLine?.ServiceLineCode,
                                                          ServiceLineName = employee.ServiceLine?.ServiceLineName,
                                                          PositionCode = employee.Position?.PositionCode,
                                                          PositionName = employee.Position?.PositionName,
                                                          PositionGroupName = employee.Position?.PositionGroupName,
                                                          Position = employee.Position?.PositionGroupName, // TODO: Rmeove once user start using positionGroupName
                                                          BillCode = employee.BillCode,
                                                          HireDate = employee.StartDate,
                                                          TerminationDate = employee.TerminationDate,
                                                          StartDate = GetStartDateForResourceDataPopulationInRA(employee, minAvailabilityStartDate),
                                                          EndDate = GetEndDateForResourceDataPopulationInRA(employee, maxAvailabilityEndDate),
                                                          Availability = 100, // Storing 100 so that it can be updated while calculating cost via Enqueue Job
                                                          EffectiveAvailability = 100,// Storing 100 so that it can be updated while calculating cost via Enqueue Job, // TODO: Rmeove once user start using positionGroupName
                                                          LastUpdatedBy = "Auto-ResWithNoAlloc"
                                                      }).ToList();

            //filter records that are hired or re-hired after 1 year to avoid the condition where start date > end date. Open to updates
            resourcesWithNoAvailabilityRecords = resourcesWithNoAvailabilityRecords.Where(x => x.StartDate <= x.EndDate).ToList();


            //filter records that are hired or re-hired after 1 year to avoid the condition where start date > end date. Open to updates
            resourcesWithNoAvailabilityRecords = resourcesWithNoAvailabilityRecords.Where(x => x.StartDate <= x.EndDate).ToList();

            if (resourcesWithNoAvailabilityRecords.Count > 0)
            {
                var resourceAvailabilityTable = GetResourceAvailabilityAnalyticsDTO(resourcesWithNoAvailabilityRecords);
                await _analyticsRepository.UpdateAvailabilityDataForResourcesWithNoAvailabilityRecordsBetweenDateRange(
                    resourceAvailabilityTable);

                var distinctEmployeeCodes =
                    string.Join(",", resourcesWithNoAvailabilityRecords.Select(x => x.EmployeeCode).Distinct());

                _backgroundJobClient.Enqueue(() => UpdateCostForResourcesAvailableInFullCapacity(distinctEmployeeCodes));

            }

            return resourcesWithNoAvailabilityRecords;
        }

        public async Task<List<ResourceAvailabilityViewModel>> UpdateAvailabilityDataForResourcesWithNoAvailabilityRecords(string employeeCodes = null)
        {
            DateTime lockInDate = DateTime.Now.AddMonths(-3);
            lockInDate = new DateTime(lockInDate.Year, lockInDate.Month, 1);

            var minAvailabilityStartDate = lockInDate;
            var dataAvailableForDays = GetThresholdForResourcesWithNoAvailabilityRecords();
            var maxAvailabilityEndDate = DateTime.Today.AddDays(dataAvailableForDays).Date;

            var allEmployees = await _resourceApiClient.GetEmployeesIncludingTerminated();

            //allEmployees who were either active after the min availability date in RA table
            var allEmployeesActiveOnOrAfterMinavailabilityDate =
                    allEmployees.Where(x => (x.TerminationDate == null || x.TerminationDate >= minAvailabilityStartDate));

            if (string.IsNullOrEmpty(employeeCodes))
            {
                employeeCodes = string.Join(',', allEmployeesActiveOnOrAfterMinavailabilityDate.Select(x => x.EmployeeCode).ToList());
            }

            var employeeCodesWithNoAvailabilityRecords = await _resourceAllocationRepository.GetResourcesWithNoAvailabilityRecords(employeeCodes);

            var resourcesWithNoAvailabilityRecords = (from employee in allEmployeesActiveOnOrAfterMinavailabilityDate
                                                      join employeeCode in employeeCodesWithNoAvailabilityRecords
                                                          on employee.EmployeeCode equals employeeCode
                                                      select new ResourceAvailabilityViewModel
                                                      {
                                                          EmployeeCode = employee.EmployeeCode,
                                                          EmployeeStatusCode = Constants.EmployeeStatus.Active,
                                                          EmployeeName = employee.FullName,
                                                          Fte = employee.Fte,
                                                          OperatingOfficeCode = employee.Office.OfficeCode,
                                                          OperatingOfficeName = employee.Office.OfficeName,
                                                          OperatingOfficeAbbreviation = employee.Office.OfficeAbbreviation,
                                                          CurrentLevelGrade = employee.LevelGrade,
                                                          ServiceLineCode = employee.ServiceLine?.ServiceLineCode,
                                                          ServiceLineName = employee.ServiceLine?.ServiceLineName,
                                                          PositionCode = employee.Position?.PositionCode,
                                                          PositionName = employee.Position?.PositionName,
                                                          PositionGroupName = employee.Position?.PositionGroupName,
                                                          Position = employee.Position?.PositionGroupName, // TODO: Rmeove once user start using positionGroupName
                                                          BillCode = employee.BillCode,
                                                          HireDate = employee.StartDate,
                                                          TerminationDate = employee.TerminationDate,
                                                          StartDate = GetStartDateForResourceDataPopulationInRA(employee, minAvailabilityStartDate),
                                                          EndDate = GetEndDateForResourceDataPopulationInRA(employee, maxAvailabilityEndDate),
                                                          Availability = 100, // Storing 100 so that it can be updated while calculating cost via Enqueue Job
                                                          EffectiveAvailability = 100,// Storing 100 so that it can be updated while calculating cost via Enqueue Job, // TODO: Rmeove once user start using positionGroupName
                                                          LastUpdatedBy = "Auto-ResWithNoAlloc"
                                                      }).ToList();

            //filter records that are hired or re-hired after 1 year to avoid the condition where start date > end date. Open to updates
            resourcesWithNoAvailabilityRecords = resourcesWithNoAvailabilityRecords.Where(x => x.StartDate <= x.EndDate).ToList();
            //filter records that do not have level grade or FTE as then their cost data could not be calculated
            resourcesWithNoAvailabilityRecords = resourcesWithNoAvailabilityRecords.Where(x => x.CurrentLevelGrade != null ||  x.Fte != decimal.Zero).ToList();
            if (resourcesWithNoAvailabilityRecords.Count > 0)
            {
                var resourceAvailabilityTable = GetResourceAvailabilityAnalyticsDTO(resourcesWithNoAvailabilityRecords);
                await _analyticsRepository.UpdateAvailabilityDataForResourcesWithNoAvailabilityRecords(
                    resourceAvailabilityTable);

                var distinctEmployeeCodes =
                    string.Join(",", resourcesWithNoAvailabilityRecords.Select(x => x.EmployeeCode).Distinct());

                _backgroundJobClient.Enqueue(() => UpdateCostForResourcesAvailableInFullCapacity(distinctEmployeeCodes));
            }

            return resourcesWithNoAvailabilityRecords;
        }

        private DateTime GetStartDateForResourceDataPopulationInRA(Resource resource, DateTime minAvailabilityStartDate)
        {
            return resource.StartDate < minAvailabilityStartDate ? minAvailabilityStartDate : resource.StartDate;
        }

        private DateTime GetEndDateForResourceDataPopulationInRA(Resource resource, DateTime maxAvailabilityEndDate)
        {
            return (resource.TerminationDate?.Date >= resource.StartDate && resource.TerminationDate?.Date <= maxAvailabilityEndDate) ? resource.TerminationDate.Value.Date : maxAvailabilityEndDate;
        }
        private async Task<List<ResourceAvailabilityViewModel>> InsertAvailabilityTillNextYearForAllActiveEmployees()
        {
            var noOfDays = GetThresholdForResourcesWithNoAvailabilityRecords();
            var lastDayForAvailability = DateTime.Today.Date.AddDays(noOfDays).Date;

            var activeEmployeesTask = _resourceApiClient.GetEmployees();
            var eCodesWithPartialAvailabilityOnDateTask =
                _resourceAllocationRepository.GetECodesWithPartialAvailabilityOnDate(lastDayForAvailability);

            await Task.WhenAll(activeEmployeesTask, eCodesWithPartialAvailabilityOnDateTask);

            var activeEmployees = activeEmployeesTask.Result;
            var eCodesWithPartialAvailability = eCodesWithPartialAvailabilityOnDateTask.Result;

            var activeEmployeesWithoutParialAvailability =
                activeEmployees.Where(e1 => !eCodesWithPartialAvailability.Any(e2 => e2 == e1.EmployeeCode) && e1.Fte != decimal.Zero);

            var resourcesToAddAvailabilityTillNextYear = (from employee in activeEmployeesWithoutParialAvailability
                                                          select new ResourceAvailabilityViewModel
                                                          {
                                                              EmployeeCode = employee.EmployeeCode,
                                                              EmployeeName = employee.FullName,
                                                              EmployeeStatusCode = Constants.EmployeeStatus.Active,
                                                              Fte = employee.Fte,
                                                              OperatingOfficeCode = employee.Office.OfficeCode,
                                                              OperatingOfficeName = employee.Office.OfficeName,
                                                              OperatingOfficeAbbreviation = employee.Office.OfficeAbbreviation,
                                                              CurrentLevelGrade = employee.LevelGrade,
                                                              ServiceLineCode = employee.ServiceLine?.ServiceLineCode,
                                                              ServiceLineName = employee.ServiceLine?.ServiceLineName,
                                                              PositionCode = employee.Position?.PositionCode,
                                                              PositionName = employee.Position?.PositionName,
                                                              PositionGroupName = employee.Position?.PositionGroupName,
                                                              Position = employee.Position?.PositionGroupName, // TODO: Remove once user starts using positonGroupName
                                                              BillCode = employee.BillCode,
                                                              HireDate = employee.StartDate,
                                                              TerminationDate = employee.TerminationDate,
                                                              StartDate = lastDayForAvailability,
                                                              EndDate = lastDayForAvailability,
                                                              Availability = 100, // Storing 100 so that it can be updated while calculating cost via Enqueue Job
                                                              EffectiveAvailability = 100,// Storing 100 so that it can be updated while calculating cost via Enqueue Job // TODO: Rmeove once user starts using employeestatuscode
                                                              LastUpdatedBy = "PollingAPI ResWithNoAlloc"
                                                          }).ToList();

            //filter records that do not have level grade as then their cost data could not be calculated
            resourcesToAddAvailabilityTillNextYear = resourcesToAddAvailabilityTillNextYear.Where(x => x.CurrentLevelGrade != null && x.HireDate <= x.EndDate).ToList();
            if (resourcesToAddAvailabilityTillNextYear.Count > 0)
            {
                var resourceAvailabilityTable = GetResourceAvailabilityAnalyticsDTO(resourcesToAddAvailabilityTillNextYear);
                await _resourceAllocationRepository.InsertAvailabilityTillNextYear(resourceAvailabilityTable);

                var distinctEmployeeCodes =
                    string.Join(",", resourcesToAddAvailabilityTillNextYear.Select(x => x.EmployeeCode).Distinct());

                _backgroundJobClient.Enqueue(() => UpdateCostForResourcesAvailableInFullCapacity(distinctEmployeeCodes));
            }
            return resourcesToAddAvailabilityTillNextYear;
        }

        public async Task<string> UpdateCostAndAvailabilityDataByScheduleId(string scheduleIds)
        {
            if (string.IsNullOrEmpty(scheduleIds))
            {
                return string.Empty;
            }

            await _analyticsRepository.UpdateCostAndAvailabilityDataByScheduleId(scheduleIds);

            _backgroundJobClient.Enqueue(() => UpdateCostForResourcesAvailableInFullCapacity(null));

            return scheduleIds;
        }

        public async Task<AnalyticsViewModel> GetResourcesAllocationAndAvailabilityByDateRange(DateTime? startDate, DateTime? endDate, DateTime? lastUpdatedFrom, DateTime? lastUpdatedTo, string action, string sourceTable, short pageNumber, int pageSize)
        {
            if (((startDate != null && endDate != null) || (lastUpdatedFrom != null && lastUpdatedTo != null)) && action != null)
            {
                if (!(lastUpdatedFrom > lastUpdatedTo))
                {
                    var analyticsData = await _analyticsRepository.GetResourcesAllocationAndAvailabilityByDateRange(startDate, endDate, lastUpdatedFrom, lastUpdatedTo, action, sourceTable, pageNumber, pageSize);
                    return analyticsData;
                }
                else
                {
                    throw new ArgumentException("'Last Updated From' date should be less than 'Last Updated To' date provided");
                }
            }
            else
            {
                throw new ArgumentException("(Date Range (start date, end date) or Last Updated Range (last updated from, last updated to)) and action should be provided");
            }
        }

        #region Private methods

        private DataTable CovertToCommitmentMasterDTO(IEnumerable<Commitment> commitments)
        {
            var commitmentmasterDataTable = new DataTable();
            commitmentmasterDataTable.Columns.Add("id", typeof(Guid));
            commitmentmasterDataTable.Columns.Add("employeeCode", typeof(string));
            commitmentmasterDataTable.Columns.Add("commitmentTypeCode", typeof(string));
            commitmentmasterDataTable.Columns.Add("startDate", typeof(DateTime));
            commitmentmasterDataTable.Columns.Add("endDate", typeof(DateTime));
            commitmentmasterDataTable.Columns.Add("allocation", typeof(short));
            commitmentmasterDataTable.Columns.Add("notes", typeof(string));
            commitmentmasterDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var commitment in commitments)
            {
                var row = commitmentmasterDataTable.NewRow();

                row["id"] = DBNull.Value;
                row["employeeCode"] = (object)commitment.EmployeeCode ?? DBNull.Value;
                row["commitmentTypeCode"] = (object)commitment.CommitmentType.CommitmentTypeCode ?? DBNull.Value;
                row["startDate"] = (object)commitment.StartDate.Date ?? DBNull.Value;
                row["endDate"] = (object)commitment.EndDate.Date ?? DBNull.Value;
                row["allocation"] = DBNull.Value;
                row["notes"] = (object)commitment.Notes ?? DBNull.Value;
                row["lastUpdatedBy"] = "Enqueue Job";

                commitmentmasterDataTable.Rows.Add(row);
            }

            return commitmentmasterDataTable;

        }
        private static List<EmployeeTransaction> GetEmployeeTransactionsBetweenAllocationDateRange(IEnumerable<EmployeeTransaction> allTransactions, ResourceAllocation allocation)
        {
            var transactions = new List<EmployeeTransaction>();
            var promotionsAndTransfers = allTransactions.Where(x =>
                    x.BusinessProcessName.Contains("Change Job")
                    && x.TransactionStatus == "Successfully Completed"
                    && x.EffectiveDate.Value.Date >= allocation.StartDate &&
                    x.EffectiveDate.Value.Date <= allocation.EndDate);

            transactions.AddRange(promotionsAndTransfers);

            var otherTransactions = allTransactions.Where(x =>
                    x.BusinessProcessName.Contains("Change Organization Assignments for Worker")
                    && x.TransactionStatus == "Successfully Completed"
                    && x.EffectiveDate.Value.Date >= allocation.StartDate &&
                    x.EffectiveDate.Value.Date <= allocation.EndDate.Value);

            transactions.AddRange(otherTransactions);

            var transitions = allTransactions.Where(x =>
                (x.BusinessProcessType == "Termination" || x.BusinessProcessType.StartsWith("Change Organization Assignments"))
                && x.TransactionStatus == "Successfully Completed")
            .Where(t => t.Transaction.TransitionStartDate != null &&
                        t.Transaction.CostCenterProposed.CostCenterId.StartsWith("0300_") &&
                         t.Transaction.TransitionStartDate.Value.Date <= allocation.EndDate &&
                         t.Transaction.TransitionEndDate.Value.Date >= allocation.StartDate);

            var termination = allTransactions.Where(x => x.BusinessProcessType == "Termination"
                && x.TransactionStatus == "Successfully Completed"
                && x.TerminationEffectiveDate.Value.Date >= allocation.StartDate
                && x.TerminationEffectiveDate.Value.Date <= allocation.EndDate);

            transactions.AddRange(UpdateEffectiveDateForTransition(transitions.ToList()));
            transactions.AddRange(UpdateEffectiveDateForTermination(termination.ToList()));

            transactions = transactions?
                .OrderBy(o => o.EffectiveDate)
                .ThenByDescending(o => o.LastModifiedDate)
                .GroupBy(g => g.EffectiveDate).Select(x => x.FirstOrDefault()).ToList();

            transactions = transactions
                .Where(x => (x.EffectiveDate.Value.Date >= allocation.StartDate && x.EffectiveDate.Value.Date <= allocation.EndDate)
                        || (x.Transaction.TransitionEndDate?.Date.Date >= allocation.StartDate && x.Transaction.TransitionStartDate?.Date.Date <= allocation.EndDate)).ToList();

            return transactions;
        }

        private static List<EmployeeTransaction> UpdateEffectiveDateForTransition(List<EmployeeTransaction> transactions)
        {
            var updatedTransactions = new List<EmployeeTransaction>();
            foreach (var transaction in transactions)
            {
                if (transaction.Transaction.TransitionStartDate != null)
                {
                    transaction.EffectiveDate = transaction.Transaction.TransitionStartDate;
                }
                updatedTransactions.Add(transaction);
            }
            return updatedTransactions;
        }

        private static List<EmployeeTransaction> UpdateEffectiveDateForTermination(List<EmployeeTransaction> terminations)
        {
            var updatedTransactions = new List<EmployeeTransaction>();
            foreach (var transaction in terminations)
            {
                if (transaction.TerminationEffectiveDate != null)
                {
                    transaction.EffectiveDate = transaction.TerminationEffectiveDate;
                }
                updatedTransactions.Add(transaction);
            }
            return updatedTransactions;
        }

        private IEnumerable<ResourceAllocation> GetRecordsWithUpdatedResourceData(IEnumerable<ResourceAllocation> resourcesFullAvailability,
            IEnumerable<Resource> resources, IEnumerable<EmployeeTransaction> resourcesFirstHireTransactions)
        {
            var recordsWithHireDate = (from allocation in resourcesFullAvailability
                                       join res in resources on allocation.EmployeeCode?.Trim() equals res.EmployeeCode?.Trim() into recordEmployee
                                       from resource in recordEmployee.DefaultIfEmpty()
                                       join hrt in resourcesFirstHireTransactions on allocation.EmployeeCode?.Trim() equals hrt.EmployeeCode?.Trim() into hireTransactionEmployee
                                       from hireTransaction in hireTransactionEmployee.DefaultIfEmpty()
                                       
                                       select new ResourceAllocation()
                                       {
                                           EmployeeCode = allocation.EmployeeCode,
                                           EmployeeStatusCode = allocation.EmployeeStatusCode,
                                           EmployeeName = resource?.FullName,
                                           Fte = resource?.Fte ?? allocation.Fte,
                                           OperatingOfficeCode = (resource?.OperatingOffice?.OfficeCode ?? resource?.Office?.OfficeCode) ?? allocation.OperatingOfficeCode,
                                           OperatingOfficeAbbreviation = (resource?.OperatingOffice?.OfficeAbbreviation ?? resource?.Office?.OfficeAbbreviation) ?? allocation.OperatingOfficeAbbreviation,
                                           OperatingOfficeName = (resource?.OperatingOffice?.OfficeName ?? resource?.Office?.OfficeName) ?? allocation.OperatingOfficeName,
                                           CurrentLevelGrade = resource?.LevelGrade ?? allocation.CurrentLevelGrade,
                                           BillCode = resource?.BillCode ?? default,
                                           ServiceLineCode = resource?.ServiceLine?.ServiceLineCode ?? allocation.ServiceLineCode,
                                           ServiceLineName = resource?.ServiceLine?.ServiceLineName ?? allocation.ServiceLineName,
                                           PositionCode = resource?.Position?.PositionCode ?? allocation.PositionCode,
                                           PositionName = resource?.Position?.PositionName ?? allocation.PositionName,
                                           PositionGroupName = resource?.Position?.PositionGroupName ?? allocation.PositionGroupName,
                                           Availability = allocation.Availability,
                                           EffectiveAvailability = allocation.EffectiveAvailability,
                                           HireDate = hireTransaction?.EffectiveDate ?? allocation.StartDate,
                                           StartDate = GetStartDateForAllocationUsingHireDate(allocation, hireTransaction?.EffectiveDate),
                                           EndDate = allocation.EndDate,
                                           TerminationDate = resource?.TerminationDate,
                                           // TODO: Delete
                                           Position = resource?.Position?.PositionGroupName ?? allocation.Position
                                       }).ToList();

            return recordsWithHireDate;
        }

        private static DateTime GetStartDateForAllocationUsingHireDate(ResourceAllocation allocation, DateTime? originalHireDate)
        {
            ////TODO: this is done to handle re-hire scenarios where terminated and then rehired in future. Think about refactoring this logic later 
            //if (resource?.TerminationDate < resource?.HireDate)
            //    return allocation.StartDate;

            return originalHireDate?.Date >= allocation.StartDate && originalHireDate?.Date <= allocation.EndDate ? originalHireDate.Value : allocation.StartDate;
        }

        private async Task<IEnumerable<ResourceAllocation>> GetRecordsWithLoAUpdated(
            IEnumerable<ResourceAllocation> records)
        {
            var recordsToUpdateWithLoA = records.ToList();
            var listEmployeeCodes = string.Join(",", recordsToUpdateWithLoA.Select(r => r.EmployeeCode).Distinct());
            var resourcesLoAs =
                await _resourceApiClient.GetEmployeesLoATransactions(listEmployeeCodes);

            var recordsWithLoAUpdated = new List<ResourceAllocation>();

            foreach (var employee in listEmployeeCodes.Split(','))
            {
                var resourceLoAs = resourcesLoAs.Where(l => l.EmployeeCode == employee);
                var resourceRecordsToUpdateWithLoA = records.Where(x => x.EmployeeCode == employee).ToList();
                if (!resourceLoAs.Any())
                {
                    recordsWithLoAUpdated.AddRange(resourceRecordsToUpdateWithLoA);
                    continue;
                }

                recordsWithLoAUpdated.AddRange(SplitAllocationForLoAs(resourceRecordsToUpdateWithLoA, resourceLoAs));

            }

            return recordsWithLoAUpdated;
        }

        private IEnumerable<ResourceAvailability> ConvertToResourceAvailability(
            IEnumerable<ResourceAllocation> resourceAvailabilityWithCost)
        {
            var result = resourceAvailabilityWithCost.Select(r => new ResourceAvailability
            {
                // Employee related info
                EmployeeCode = r.EmployeeCode,
                EmployeeName = r.EmployeeName,
                EmployeeStatusCode = r.EmployeeStatusCode,
                HireDate = r.HireDate,
                TerminationDate = r.TerminationDate,
                BillCode = r.BillCode,
                CurrentLevelGrade = r.CurrentLevelGrade,
                OperatingOfficeCode = r.OperatingOfficeCode,
                OperatingOfficeAbbreviation = r.OperatingOfficeAbbreviation,
                OperatingOfficeName = r.OperatingOfficeName,
                ServiceLineCode = r.ServiceLineCode,
                ServiceLineName = r.ServiceLineName,
                PositionCode = r.PositionCode,
                PositionName = r.PositionName,
                PositionGroupName = r.PositionGroupName,
                Fte = r.Fte.Value,
                // staffing related info
                Availability = r.Availability,
                StartDate = r.StartDate,
                EndDate = (DateTime)r.EndDate,
                LastUpdatedBy = r.LastUpdatedBy,
                // Commitment Related Info
                PriorityCommitmentTypeCode = r.PriorityCommitmentTypeCode,
                PriorityCommitmentTypeName = r.PriorityCommitmentTypeName,
                CommitmentTypeCodes = r.CommitmentTypeCodes,
                CommitmentTypeReasonCode = r.CommitmentTypeReasonCode,
                CommitmentTypeReasonName = r.CommitmentTypeReasonName,
                isOverriddenInSource = r.isOverriddenInSource,
                Ringfence = r.Ringfence,
                isStaffingTag = r.isStaffingTag,
                // Finance related info
                BillRate = r.BillRate,
                BillRateCurrency = r.BillRateCurrency,
                BillRateType = r.BillRateType,
                OpportunityCost = r.OpportunityCost,
                OpportunityCostInUSD = r.CostInUSD,
                CostUSDEffectiveYear = r.CostUSDEffectiveYear,
                UsdRate = r.UsdRate,
                //TODO: Delete
                Position = r.Position,
                EffectiveAvailability = r.EffectiveAvailability,
                EffectiveAvailabilityReason = r.EffectiveAvailabilityReason,
                EffectiveOpportunityCost = r.EffectiveOpportunityCost,
                EffectiveOpportunityCostReason = r.EffectiveOpportunityCostReason
            });

            return result;
        }

        private IEnumerable<ResourceAllocation> GetResourcePointInTimeInfo(
            IEnumerable<EmployeeTransaction> staffingTransactions,
            ResourceAllocation resource)
        {
            var employeeWDTransaction = staffingTransactions.OrderBy(x => x.EffectiveDate)
                .ThenByDescending(x => x.LastModifiedDate).FirstOrDefault();

            var effectiveServiceLineName = resource.StartDate >= employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.ServiceLineProposed?.ServiceLineName : (employeeWDTransaction.BusinessProcessName == "Hire" ? employeeWDTransaction.Transaction.ServiceLineProposed?.ServiceLineName : employeeWDTransaction.Transaction.ServiceLineCurrent?.ServiceLineName);
            var effectiveServiceLineCode = resource.StartDate >= employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.ServiceLineProposed?.ServiceLineId : (employeeWDTransaction.BusinessProcessName == "Hire" ? employeeWDTransaction.Transaction.ServiceLineProposed?.ServiceLineId : employeeWDTransaction.Transaction.ServiceLineCurrent?.ServiceLineId);
            var effectiveStartDate = resource.StartDate;
            var employeeCode = resource.EmployeeCode;
            var hireDate = resource.HireDate;
            var terminationDate = resource.TerminationDate;
            var effectiveLevelGrade = (resource.StartDate >= employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.PdGradeProposed : (employeeWDTransaction.Transaction.PdGradeCurrent ?? employeeWDTransaction.Transaction.PdGradeProposed)) ?? resource.CurrentLevelGrade;
            var effectiveOperatingOffice = resource.StartDate >= employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.SchedulingOfficeProposed : (employeeWDTransaction.Transaction.SchedulingOfficeCurrent ?? employeeWDTransaction.Transaction.SchedulingOfficeProposed);
            var effectiveFte = resource.StartDate >= employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.FteProposed : (employeeWDTransaction.BusinessProcessName == "Hire" ? employeeWDTransaction.Transaction.FteProposed : employeeWDTransaction.Transaction.FteCurrent);
            var effectiveBillCode = resource.StartDate >= employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.BillCodeProposed : (employeeWDTransaction.BusinessProcessName == "Hire" ? employeeWDTransaction.Transaction.BillCodeProposed : employeeWDTransaction.Transaction.BillCodeCurrent);
            var effectivePosition = resource.StartDate >= employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.PositionProposed : employeeWDTransaction.Transaction.PositionCurrent ?? employeeWDTransaction.Transaction.PositionProposed;
            var effectiveEmployeeStatusCode = GetEmployeePointInTimeStatus(employeeWDTransaction, effectiveStartDate);
            var resourcePointInTimeInfo = new List<ResourceAllocation>();

            foreach (var transaction in staffingTransactions)
            {
                if (transaction.EffectiveDate.Value.Date > resource.EndDate) break;

                var effectiveEndDate = effectiveStartDate >= transaction.EffectiveDate.Value.Date ? effectiveStartDate :
                    transaction.EffectiveDate.Value.Date.AddDays(-1);

                if (transaction.TerminationEffectiveDate != null)
                {
                    effectiveEndDate = effectiveStartDate >= transaction.TerminationEffectiveDate.Value.Date.AddDays(-1) ? effectiveStartDate :
                    transaction.TerminationEffectiveDate.Value.Date.AddDays(-2);
                }

                resourcePointInTimeInfo.Add(CloneResourceAllocation(employeeCode, resource.EmployeeName,
                    effectiveLevelGrade, effectiveFte, effectiveBillCode, resource.Availability, resource.EffectiveAvailability, effectiveOperatingOffice, effectivePosition?.PositionGroupName ?? resource.Position,
                    effectivePosition?.PositionCode ?? resource.PositionCode, effectivePosition?.PositionName ?? resource.PositionName, effectivePosition?.PositionGroupName ?? resource.PositionGroupName,
                    effectiveServiceLineCode, effectiveServiceLineName, hireDate, terminationDate, effectiveStartDate, effectiveEndDate, effectiveEmployeeStatusCode));

                effectiveStartDate = effectiveStartDate >= transaction.EffectiveDate.Value.Date ? effectiveStartDate.AddDays(1) : transaction.EffectiveDate.Value.Date;
                if (transaction.TerminationEffectiveDate != null)
                {
                    effectiveStartDate = effectiveStartDate >= transaction.TerminationEffectiveDate.Value.Date.AddDays(-1) ? effectiveStartDate.AddDays(1) : transaction.TerminationEffectiveDate.Value.Date.AddDays(-1);
                }

                if (effectiveStartDate.Date > resource.EndDate)
                    break;

                effectiveLevelGrade = transaction.Transaction.PdGradeProposed ?? resource.CurrentLevelGrade;
                effectiveOperatingOffice = transaction.Transaction.SchedulingOfficeProposed;
                effectiveFte = transaction.Transaction.FteProposed;
                effectiveBillCode = transaction.Transaction.BillCodeProposed;
                effectivePosition = transaction.Transaction.PositionProposed;
                effectiveServiceLineName = transaction.Transaction.ServiceLineProposed?.ServiceLineName;
                effectiveServiceLineCode = transaction.Transaction.ServiceLineProposed?.ServiceLineId;
                effectiveEmployeeStatusCode = GetEmployeePointInTimeStatus(transaction, effectiveStartDate);
            }

            if (effectiveStartDate.Date <= resource.EndDate)
            {
                resourcePointInTimeInfo.Add(CloneResourceAllocation(employeeCode, resource.EmployeeName,
                    effectiveLevelGrade, effectiveFte, effectiveBillCode, resource.Availability, resource.EffectiveAvailability, effectiveOperatingOffice, effectivePosition?.PositionGroupName ?? resource.Position,
                        effectivePosition?.PositionCode ?? resource.PositionCode, effectivePosition?.PositionName ?? resource.PositionName, effectivePosition?.PositionGroupName ?? resource.PositionGroupName,
                        effectiveServiceLineCode, effectiveServiceLineName, hireDate, terminationDate, effectiveStartDate, (DateTime)resource.EndDate, effectiveEmployeeStatusCode));
            }
            return resourcePointInTimeInfo;
        }

        private IEnumerable<ResourceAllocation> GetAllocationsDividedByBillRate(IEnumerable<BillRate> billRates,
            IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var allocationsWithBillRates = new List<ResourceAllocation>();
            resourceAllocations = DivideAllocationsForMultipleBillRatesLiesInAllocationDateRange(billRates, resourceAllocations);
            foreach (var resourceAllocation in resourceAllocations)
            {
                var filteredBillRates = GetBillRatesBySelectedValues(billRates,
                    resourceAllocation.CurrentLevelGrade, "S", resourceAllocation.BillCode,
                    "M", resourceAllocation.OperatingOfficeCode.ToString());

                if (!filteredBillRates.Any())
                {
                    UpdateCostDataForAllocationsWithNoBillRate(resourceAllocation);
                    allocationsWithBillRates.Add(resourceAllocation);
                    continue;
                }

                var billRate = filteredBillRates.FirstOrDefault(billRate =>
                    billRate.StartDate <= resourceAllocation.EndDate &&
                    (billRate.EndDate == null || billRate.EndDate >= resourceAllocation.StartDate));

                if (billRate == null)
                {
                    UpdateCostDataForAllocationsWithNoBillRate(resourceAllocation);
                    allocationsWithBillRates.Add(resourceAllocation);
                }
                else
                {
                    allocationsWithBillRates.AddRange(UpdateBillCostForAllocation(resourceAllocation, billRate));
                }
            }

            return allocationsWithBillRates;


        }

        private IEnumerable<ResourceAllocation> DivideAllocationsForMultipleBillRatesLiesInAllocationDateRange(IEnumerable<BillRate> billRates, IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var resourceAllocationsSplitForBillRates = new List<ResourceAllocation>();
            foreach (var allocation in resourceAllocations)
            {
                var filteredBillRates = GetBillRatesBySelectedValues(billRates, allocation.CurrentLevelGrade, "S",
                    allocation.BillCode, "M", allocation.OperatingOfficeCode.ToString());
                var billRatesDateRangeExistsInAllocationDateRange = filteredBillRates.Where(x =>
                        x.StartDate <= allocation.EndDate && (x.EndDate == null || x.EndDate >= allocation.StartDate));
                if (billRatesDateRangeExistsInAllocationDateRange == null)
                {
                    resourceAllocationsSplitForBillRates.Add(allocation);
                    continue;
                }

                foreach (var billRate in billRatesDateRangeExistsInAllocationDateRange.OrderBy(x => x.StartDate))
                {
                    if (billRate.StartDate > allocation.StartDate &&
                        (billRate.EndDate == null || billRate.EndDate >= allocation.EndDate))
                    {
                        SplitAllocationForBillRateStartedBetweenAllocationDateRange(allocation, resourceAllocationsSplitForBillRates, billRate);
                        continue;
                    }

                    if (billRate.StartDate > allocation.StartDate && billRate.EndDate != null &&
                        billRate.EndDate < allocation.EndDate)
                    {
                        SplitAllocationForBillRateLiesWithinAllocationDateRange(allocation, resourceAllocationsSplitForBillRates, billRate);
                        continue;
                    }
                    var startDate = allocation.StartDate;
                    var endDate = (billRate.EndDate == null || billRate.EndDate >= allocation.EndDate) ? allocation.EndDate : billRate.EndDate;
                    resourceAllocationsSplitForBillRates.Add(CloneResourceAllocation(allocation, startDate, endDate));
                    allocation.StartDate = endDate.Value.AddDays(1);
                }

                if (allocation.StartDate <= allocation.EndDate)
                {
                    resourceAllocationsSplitForBillRates.Add(allocation);
                }

            }

            return resourceAllocationsSplitForBillRates;
        }

        private void SplitAllocationForBillRateLiesWithinAllocationDateRange(ResourceAllocation allocation, List<ResourceAllocation> resourceAllocationsSplitForBillRates, BillRate billRate)
        {
            var startDate = allocation.StartDate;
            var endDate = billRate.StartDate.AddDays(-1);
            resourceAllocationsSplitForBillRates.Add(CloneResourceAllocation(allocation, startDate, endDate));
            resourceAllocationsSplitForBillRates.Add(CloneResourceAllocation(allocation, billRate.StartDate, billRate.EndDate));
            allocation.StartDate = billRate.EndDate.Value.AddDays(1);
        }

        private void SplitAllocationForBillRateStartedBetweenAllocationDateRange(ResourceAllocation allocation, List<ResourceAllocation> resourceAllocationsSplitForBillRates, BillRate billRate)
        {
            var startDate = allocation.StartDate;
            var endDate = billRate.StartDate.AddDays(-1);
            resourceAllocationsSplitForBillRates.Add(CloneResourceAllocation(allocation, startDate, endDate));
            resourceAllocationsSplitForBillRates.Add(CloneResourceAllocation(allocation, billRate.StartDate, allocation.EndDate));
            allocation.StartDate = allocation.EndDate.Value.AddDays(1);
        }

        private void UpdateCostDataForAllocationsWithNoBillRate(ResourceAllocation resourceAllocation)
        {
            resourceAllocation.ActualCost = 0M;
            resourceAllocation.CostInUSD = 0M;
            resourceAllocation.BillRate = null;
            resourceAllocation.UsdRate = null;
            resourceAllocation.BillRateCurrency = null;
            resourceAllocation.BillRateType = null;
            resourceAllocation.EffectiveCostReason = "Rate NA";
            resourceAllocation.EffectiveOpportunityCostReason = "Rate NA";
        }

        private IEnumerable<BillRate> GetBillRatesBySelectedValues(IEnumerable<BillRate> billRates, string levelGrade,
            string billRateType, decimal billCode, string breakdown, string officeCode)
        {
            levelGrade = Regex.Replace(levelGrade, @"\s+", string.Empty);

            var filteredBillRates = billRates.Where(br =>
                string.Equals(br.LevelGrade, levelGrade.Trim(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(br.Type, billRateType, StringComparison.OrdinalIgnoreCase) &&
                br.BillCode == billCode &&
                br.OfficeCode == officeCode &&
                string.Equals(br.Breakdown, breakdown, StringComparison.OrdinalIgnoreCase));

            return filteredBillRates;
        }

        private void GetAllocationsForHistoricalBillRates(IEnumerable<BillRate> billRates,
            ResourceAllocation resourceAllocation, List<ResourceAllocation> allocationsWithBillRates)
        {
            var historicalBillRate = billRates.FirstOrDefault(x => resourceAllocation.EndDate >= x.StartDate && resourceAllocation.EndDate <= x.EndDate);


            if (historicalBillRate == null)
            {
                UpdateCostDataForAllocationsWithNoBillRate(resourceAllocation);
                allocationsWithBillRates.Add(resourceAllocation);
            }

            else if (IsAllocationWithinBillRateDateRange(resourceAllocation.StartDate, resourceAllocation.EndDate, historicalBillRate))
            {
                allocationsWithBillRates.AddRange(UpdateBillCostForAllocation(resourceAllocation, historicalBillRate));
            }
            else
            {
                var allocationDateRangePrecedesCurrentBillRate = CloneResourceAllocation(resourceAllocation, resourceAllocation.StartDate, historicalBillRate.StartDate.AddDays(-1));
                allocationsWithBillRates.AddRange(UpdateBillCostForAllocation(resourceAllocation, historicalBillRate, historicalBillRate.StartDate, resourceAllocation.EndDate));
                GetAllocationsForHistoricalBillRates(billRates, allocationDateRangePrecedesCurrentBillRate, allocationsWithBillRates);
            }
        }

        private void GetAllocationsForFutureBillRates(IEnumerable<BillRate> filteredBillRates,
            ResourceAllocation resourceAllocation, List<ResourceAllocation> allocationsWithBillRates)
        {
            var futureBillRate = filteredBillRates.FirstOrDefault(x =>
                x.StartDate <= resourceAllocation.StartDate &&
                (x.EndDate == null || x.EndDate >= resourceAllocation.StartDate));


            if (futureBillRate == null)
            {
                UpdateCostDataForAllocationsWithNoBillRate(resourceAllocation);
                allocationsWithBillRates.Add(resourceAllocation);
            }
            else if (IsAllocationWithinBillRateDateRange(resourceAllocation.StartDate, resourceAllocation.EndDate,
                futureBillRate))
            {
                allocationsWithBillRates.AddRange(UpdateBillCostForAllocation(resourceAllocation, futureBillRate));
            }
            else
            {
                var allocationDateRangeExceededCurrentBillRate = CloneResourceAllocation(resourceAllocation,
                    futureBillRate.EndDate.Value.AddDays(1), resourceAllocation.EndDate);
                allocationsWithBillRates.AddRange(UpdateBillCostForAllocation(resourceAllocation, futureBillRate,
                    resourceAllocation.StartDate, futureBillRate.EndDate));
                GetAllocationsForFutureBillRates(filteredBillRates, allocationDateRangeExceededCurrentBillRate,
                    allocationsWithBillRates);
            }
        }

        private static bool IsAllocationWithinBillRateDateRange(DateTime startDate, DateTime? endDate,
            BillRate billRate)
        {
            return billRate.StartDate <= startDate && (billRate.EndDate == null || endDate <= billRate.EndDate);
        }

        private static bool IsAllocationEndDateExceedsBillRateEndDate(DateTime startDate, DateTime? endDate,
            BillRate billRate)
        {
            return billRate.EndDate != null && billRate.EndDate >= startDate && billRate.StartDate <= startDate &&
                   endDate > billRate.EndDate;
        }

        private static bool IsAllocationStartDatePrecedesBillRateStartDate(DateTime startDate, DateTime? endDate,
            BillRate billRate)
        {
            return startDate < billRate.StartDate && endDate >= billRate.StartDate &&
                   (billRate.EndDate == null || endDate <= billRate.EndDate);
        }

        private static bool IsAllocationDateRangePrecedesBillRateStartDate(DateTime startDate, DateTime? endDate,
            BillRate billRate)
        {
            return startDate < billRate.StartDate && endDate < billRate.StartDate;
        }

        private static bool IsAllocationDateRangeExceedsBillRateEndDate(DateTime startDate, DateTime? endDate,
            BillRate billRate)
        {
            return startDate > billRate.EndDate;
        }

        private IEnumerable<ResourceAllocation> UpdateBillCostForAllocation(ResourceAllocation resourceAllocation,
           BillRate billRate, DateTime? startDate = null, DateTime? endDate = null)
        {
            var allocationsWithBillRates = new List<ResourceAllocation>();
            var currentStartDate = startDate ?? resourceAllocation.StartDate;
            var currentEndDate = endDate ?? resourceAllocation.EndDate;
            while (currentStartDate <= currentEndDate)
            {
                var currentMonthStartDate = new DateTime(currentStartDate.Year, currentStartDate.Month, 1);
                var currentMonthEndDate = currentMonthStartDate.AddMonths(1).AddDays(-1);
                var allocationStartDate = currentStartDate;
                var allocationEndDate = currentEndDate < currentMonthEndDate ? currentEndDate : currentMonthEndDate;
                var workingDaysInMonth = GetWorkingDaysInMonth(allocationStartDate.Month, allocationStartDate.Year);
                var clonedResourceAllocation =
                    CloneResourceAllocation(resourceAllocation, allocationStartDate, allocationEndDate);
                clonedResourceAllocation.BillRateType = billRate.Type;
                clonedResourceAllocation.BillRate = billRate.Rate / workingDaysInMonth;
                clonedResourceAllocation.ActualCost =
                    resourceAllocation.Allocation * billRate.Rate / workingDaysInMonth / 100;
                clonedResourceAllocation.EffectiveCost =
                    resourceAllocation.TransactionType == "Transition" || resourceAllocation.InvestmentCode == 5
                        ? 0
                        : resourceAllocation.Allocation * billRate.Rate / workingDaysInMonth / 100;
                clonedResourceAllocation.EffectiveCostReason = resourceAllocation.TransactionType == "Transition"
                    ? "Transition"
                    : resourceAllocation.InvestmentCode == 5
                        ? "Investment - Internal PD"
                        : null;
                clonedResourceAllocation.BillRateCurrency = billRate.Currency.Trim();
                clonedResourceAllocation.OpportunityCost =
                    resourceAllocation.Availability * billRate.Rate / workingDaysInMonth / 100;
                clonedResourceAllocation.EffectiveOpportunityCost =
                    resourceAllocation.EffectiveAvailability * billRate.Rate / workingDaysInMonth / 100;

                allocationsWithBillRates.Add(clonedResourceAllocation);

                currentStartDate = allocationEndDate.Value.AddDays(1);
            }

            return allocationsWithBillRates;
        }

        private static int GetWorkingDaysInMonth(int month, int year)
        {
            var weekends = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var businessDaysInMonth = Enumerable.Range(1, daysInMonth)
                .Where(d => !weekends.Contains(new DateTime(year, month, d).DayOfWeek));

            return businessDaysInMonth.Count();
        }

        private IEnumerable<ResourceAllocation> SplitAllocationForHistoricalTransactions(
            ResourceAllocation resourceAllocation, IEnumerable<EmployeeTransaction> employeeStaffingTransactions)
        {
            var employeeWDTransaction = employeeStaffingTransactions.OrderBy(x => x.EffectiveDate)
                .ThenByDescending(t => t.LastModifiedDate)
                .FirstOrDefault();
            var resourceAllocations = new List<ResourceAllocation>();
            var effectiveStartDate = resourceAllocation.StartDate;
            var effectiveLevelGrade = resourceAllocation.StartDate == employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.PdGradeProposed : (employeeWDTransaction.Transaction.PdGradeCurrent ?? employeeWDTransaction.Transaction.PdGradeProposed);
            var effectiveOperatingOffice = resourceAllocation.StartDate == employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.SchedulingOfficeProposed : (employeeWDTransaction.Transaction.SchedulingOfficeCurrent ?? employeeWDTransaction.Transaction.SchedulingOfficeProposed);
            var effectiveFte = resourceAllocation.StartDate == employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.FteProposed : ((decimal?)employeeWDTransaction.Transaction.FteCurrent ?? employeeWDTransaction.Transaction.FteProposed);
            var effectiveBillCode = resourceAllocation.StartDate == employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.BillCodeProposed : ((decimal?)employeeWDTransaction.Transaction.BillCodeCurrent ?? employeeWDTransaction.Transaction.BillCodeProposed);
            var effectiveServiceLine = resourceAllocation.StartDate == employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.ServiceLineProposed : (employeeWDTransaction.Transaction.ServiceLineCurrent ?? employeeWDTransaction.Transaction.ServiceLineProposed);
            var effectivePosition = resourceAllocation.StartDate == employeeWDTransaction.EffectiveDate.Value.Date ? employeeWDTransaction.Transaction.PositionProposed : (employeeWDTransaction.Transaction.PositionCurrent ?? employeeWDTransaction.Transaction.PositionProposed);
            //TODO: Update logic to cater transition date range scenarios
            var effectiveEmployeeStatusCode = GetEmployeePointInTimeStatus(employeeWDTransaction, effectiveStartDate);
            foreach (var transaction in employeeStaffingTransactions)
            {
                DateTime effectiveEndDate = GetEffectiveEndDate(resourceAllocation, effectiveStartDate, transaction);
                var resourceAllocationWithPointInTime =
                    CloneResourceAllocation(resourceAllocation, effectiveStartDate, effectiveEndDate);
                resourceAllocationWithPointInTime.CurrentLevelGrade = effectiveLevelGrade;
                resourceAllocationWithPointInTime.Fte = effectiveFte;
                resourceAllocationWithPointInTime.BillCode = effectiveBillCode;
                int.TryParse(effectiveOperatingOffice?.OfficeCode, out var currentOfficeCode);
                resourceAllocationWithPointInTime.OperatingOfficeCode = currentOfficeCode;
                resourceAllocationWithPointInTime.OperatingOfficeName = effectiveOperatingOffice?.OfficeName;
                resourceAllocationWithPointInTime.OperatingOfficeAbbreviation =
                    effectiveOperatingOffice?.OfficeAbbreviation;
                resourceAllocationWithPointInTime.ServiceLineCode = effectiveServiceLine?.ServiceLineId;
                resourceAllocationWithPointInTime.ServiceLineName = effectiveServiceLine?.ServiceLineName;
                resourceAllocationWithPointInTime.PositionCode = effectivePosition?.PositionCode;
                resourceAllocationWithPointInTime.PositionName = effectivePosition?.PositionName;
                resourceAllocationWithPointInTime.PositionGroupName = effectivePosition?.PositionGroupName;
                resourceAllocationWithPointInTime.Position = effectivePosition?.PositionGroupName;
                resourceAllocationWithPointInTime.EmployeeStatusCode = effectiveEmployeeStatusCode;

                resourceAllocations.Add(resourceAllocationWithPointInTime);

                effectiveStartDate = GetEffectiveStartDate(resourceAllocation, effectiveStartDate, transaction);
                effectiveLevelGrade = transaction.Transaction.PdGradeProposed;
                effectiveOperatingOffice = transaction.Transaction.SchedulingOfficeProposed;
                effectiveFte = transaction.Transaction.FteProposed;
                effectiveBillCode = transaction.Transaction.BillCodeProposed;
                effectiveServiceLine = transaction.Transaction.ServiceLineProposed;
                effectivePosition = transaction.Transaction.PositionProposed;
                effectiveEmployeeStatusCode = GetEmployeePointInTimeStatus(transaction, effectiveStartDate);
            }

            // Avoid split of single day allocation into 2 that results in duplicate records
            if (resourceAllocation.StartDate.Date == resourceAllocation.EndDate.Value.Date)
            {
                return resourceAllocations;
            }

            var resourceAllocationWithCurrentData = CloneResourceAllocation(resourceAllocation,
                effectiveStartDate, resourceAllocation.EndDate.Value.Date);
            resourceAllocationWithCurrentData.CurrentLevelGrade = effectiveLevelGrade;
            int.TryParse(effectiveOperatingOffice?.OfficeCode, out var proposedOfficeCode);
            resourceAllocationWithCurrentData.OperatingOfficeCode = proposedOfficeCode;
            resourceAllocationWithCurrentData.OperatingOfficeName = effectiveOperatingOffice?.OfficeName;
            resourceAllocationWithCurrentData.OperatingOfficeAbbreviation = effectiveOperatingOffice?.OfficeAbbreviation;
            resourceAllocationWithCurrentData.Fte = effectiveFte;
            resourceAllocationWithCurrentData.BillCode = effectiveBillCode;
            resourceAllocationWithCurrentData.ServiceLineCode = effectiveServiceLine?.ServiceLineId;
            resourceAllocationWithCurrentData.ServiceLineName = effectiveServiceLine?.ServiceLineName;
            resourceAllocationWithCurrentData.PositionCode = effectivePosition?.PositionCode;
            resourceAllocationWithCurrentData.PositionName = effectivePosition?.PositionName;
            resourceAllocationWithCurrentData.PositionGroupName = effectivePosition?.PositionGroupName;
            resourceAllocationWithCurrentData.Position = effectivePosition?.PositionGroupName;
            resourceAllocationWithCurrentData.EmployeeStatusCode = effectiveEmployeeStatusCode;

            // TODO: Need to revist this code for a better solution. Here we are checking if the allocation object is already added.
            if (!isAllocationExists(resourceAllocations, resourceAllocationWithCurrentData))
            {
                resourceAllocations.Add(resourceAllocationWithCurrentData);
            }
            return resourceAllocations;
        }

        private bool isAllocationExists(List<ResourceAllocation> resourceAllocations, ResourceAllocation resourceAllocationWithCurrentData)
        {
            return resourceAllocations.Any(x =>
            x.Id == resourceAllocationWithCurrentData.Id
            && x.EmployeeCode == resourceAllocationWithCurrentData.EmployeeCode
            && x.StartDate.Date == resourceAllocationWithCurrentData.StartDate.Date
            && x.EndDate.Value.Date == resourceAllocationWithCurrentData.EndDate.Value.Date);
        }

        private static DateTime GetEffectiveStartDate(ResourceAllocation resourceAllocation, DateTime startDate, EmployeeTransaction transaction)
        {
            var effectiveStartDate = startDate;

            if (startDate > transaction.EffectiveDate.Value.Date
                   && transaction.Transaction.TransitionStartDate != null
                   && startDate >= transaction.Transaction.TransitionStartDate.Value.Date)
            {
                effectiveStartDate = transaction.Transaction.TransitionEndDate.Value.Date.AddDays(1);
            }
            if (startDate > transaction.EffectiveDate.Value.Date
                && transaction.TerminationEffectiveDate != null
                && startDate < transaction.TerminationEffectiveDate.Value.Date)
            {
                effectiveStartDate = transaction.TerminationEffectiveDate.Value.Date;
            }
            if (startDate < transaction.EffectiveDate.Value.Date)
            {
                effectiveStartDate = transaction.EffectiveDate.Value.Date;
            }
            if (startDate == transaction.EffectiveDate.Value.Date)
            {
                effectiveStartDate = transaction.EffectiveDate.Value.AddDays(1).Date;
            }

            return effectiveStartDate < resourceAllocation.EndDate.Value.Date
                ? effectiveStartDate
                : resourceAllocation.EndDate.Value.Date;

        }

        private static DateTime GetEffectiveEndDate(ResourceAllocation resourceAllocation, DateTime effectiveStartDate, EmployeeTransaction transaction)
        {
            DateTime effectiveEndDate = effectiveStartDate;

            if (effectiveStartDate > transaction.EffectiveDate.Value.Date
                && transaction.Transaction.TransitionStartDate != null
                && effectiveStartDate >= transaction.Transaction.TransitionStartDate.Value.Date)
            {
                effectiveEndDate = transaction.Transaction.TransitionEndDate.Value.Date;
            }
            if (effectiveStartDate > transaction.EffectiveDate.Value.Date
                && transaction.TerminationEffectiveDate != null
                && effectiveStartDate < transaction.TerminationEffectiveDate.Value.Date)
            {
                effectiveEndDate = transaction.TerminationEffectiveDate.Value.Date.AddDays(-1);
            }
            if (effectiveStartDate < transaction.EffectiveDate.Value.Date)
            {
                effectiveEndDate = transaction.EffectiveDate.Value.Date.AddDays(-1);
            }
            return resourceAllocation.EndDate < effectiveEndDate
                ? resourceAllocation.EndDate.Value.Date.AddDays(-1)
                : effectiveEndDate;

        }

        private Constants.EmployeeStatus GetEmployeePointInTimeStatus(EmployeeTransaction transaction, DateTime startDate)
        {
            if (transaction.TerminationEffectiveDate != null && transaction.TerminationEffectiveDate.Value.Date <= startDate.Date)
            {
                return Constants.EmployeeStatus.Terminated;
            }
            if (transaction.Transaction.TransitionStartDate != null
                && transaction.Transaction.TransitionStartDate.Value.Date <= startDate.Date
                && transaction.Transaction.TransitionEndDate.Value.Date >= startDate.Date)
            {
                return Constants.EmployeeStatus.Transition;
            }
            return Constants.EmployeeStatus.Active;

        }

        private ResourceAllocation CloneResourceAllocation(ResourceAllocation resourceAllocation, DateTime? startDate,
            DateTime? endDate)
        {
            return new ResourceAllocation
            {
                // Common to case and opp
                ClientCode = resourceAllocation.ClientCode,
                ClientName = resourceAllocation.ClientName,
                ClientGroupCode = resourceAllocation.ClientGroupCode,
                ClientGroupName = resourceAllocation.ClientGroupName,
                // Opp related info
                PipelineId = resourceAllocation.PipelineId,
                OpportunityName = resourceAllocation.OpportunityName,
                // Case related info
                OldCaseCode = resourceAllocation.OldCaseCode,
                CaseCode = resourceAllocation.CaseCode,
                CaseName = resourceAllocation.CaseName,
                CaseTypeCode = resourceAllocation.CaseTypeCode,
                CaseTypeName = resourceAllocation.CaseTypeName,
                ManagingOfficeCode = resourceAllocation.ManagingOfficeCode,
                ManagingOfficeAbbreviation = resourceAllocation.ManagingOfficeAbbreviation,
                ManagingOfficeName = resourceAllocation.ManagingOfficeName,
                BillingOfficeCode = resourceAllocation.BillingOfficeCode,
                BillingOfficeAbbreviation = resourceAllocation.BillingOfficeAbbreviation,
                BillingOfficeName = resourceAllocation.BillingOfficeName,
                PegCase = resourceAllocation.PegCase,
                PrimaryIndustryTermCode = resourceAllocation.PrimaryIndustryTermCode,
                PrimaryIndustryTagId = resourceAllocation.PrimaryIndustryTagId,
                PrimaryIndustry = resourceAllocation.PrimaryIndustry,
                PracticeAreaIndustryCode = resourceAllocation.PracticeAreaIndustryCode,
                PracticeAreaIndustry = resourceAllocation.PracticeAreaIndustry,
                PrimaryCapabilityTermCode = resourceAllocation.PrimaryCapabilityTermCode,
                PrimaryCapabilityTagId = resourceAllocation.PrimaryCapabilityTagId,
                PrimaryCapability = resourceAllocation.PrimaryCapability,
                PracticeAreaCapabilityCode = resourceAllocation.PracticeAreaCapabilityCode,
                PracticeAreaCapability = resourceAllocation.PracticeAreaCapability,
                // Employee related info
                EmployeeCode = resourceAllocation.EmployeeCode,
                EmployeeName = resourceAllocation.EmployeeName,
                EmployeeStatusCode = resourceAllocation.EmployeeStatusCode,
                Fte = resourceAllocation.Fte,
                ServiceLineCode = resourceAllocation.ServiceLineCode,
                ServiceLineName = resourceAllocation.ServiceLineName,
                PositionCode = resourceAllocation.PositionCode,
                PositionName = resourceAllocation.PositionName,
                PositionGroupName = resourceAllocation.PositionGroupName,
                BillCode = resourceAllocation.BillCode,
                HireDate = resourceAllocation.HireDate,
                TerminationDate = resourceAllocation.TerminationDate,
                InternetAddress = resourceAllocation.InternetAddress,
                CurrentLevelGrade = resourceAllocation.CurrentLevelGrade,
                OperatingOfficeCode = resourceAllocation.OperatingOfficeCode,
                OperatingOfficeAbbreviation = resourceAllocation.OperatingOfficeAbbreviation,
                OperatingOfficeName = resourceAllocation.OperatingOfficeName,
                // Staffing related info
                Id = resourceAllocation.Id,
                Allocation = resourceAllocation.Allocation,
                StartDate = startDate ?? resourceAllocation.StartDate,
                EndDate = endDate ?? resourceAllocation.EndDate,
                Availability = resourceAllocation.Availability,
                InvestmentCode = resourceAllocation.InvestmentCode,
                InvestmentName = resourceAllocation.InvestmentName,
                CaseRoleCode = resourceAllocation.CaseRoleCode,
                CaseRoleName = resourceAllocation.CaseRoleName,
                TransactionType = resourceAllocation.TransactionType,
                // Finance related info
                BillRate = resourceAllocation.BillRate,
                BillRateType = resourceAllocation.BillRateType,
                BillRateCurrency = resourceAllocation.BillRateCurrency,
                ActualCost = resourceAllocation.ActualCost,
                OpportunityCost = resourceAllocation.OpportunityCost,
                CostInUSD = resourceAllocation.CostInUSD,
                CostUSDEffectiveYear = resourceAllocation.CostUSDEffectiveYear,
                UsdRate = resourceAllocation.UsdRate,
                LastUpdatedBy = resourceAllocation.LastUpdatedBy,
                // Commitment Related Info
                PriorityCommitmentTypeCode = resourceAllocation.PriorityCommitmentTypeCode,
                PriorityCommitmentTypeName = resourceAllocation.PriorityCommitmentTypeName,
                CommitmentTypeCodes = resourceAllocation.CommitmentTypeCodes,
                isOverriddenInSource = resourceAllocation.isOverriddenInSource,
                Ringfence = resourceAllocation.Ringfence,
                isStaffingTag = resourceAllocation.isStaffingTag,
                // TODO: Delete once Analplan is provided with API
                Position = resourceAllocation.Position, //TODO: remove after communication with Anaplan
                EffectiveCost = resourceAllocation.EffectiveCost,
                EffectiveCostReason = resourceAllocation.EffectiveCostReason,
                EffectiveOpportunityCost = resourceAllocation.EffectiveOpportunityCost,
                EffectiveOpportunityCostReason = resourceAllocation.EffectiveOpportunityCostReason,
                EffectiveAvailability = resourceAllocation.EffectiveAvailability,
                EffectiveAvailabilityReason = resourceAllocation.EffectiveAvailabilityReason,
                //Planning Card Related info
                PlanningCardId = resourceAllocation.PlanningCardId,
                PlanningCardName = resourceAllocation.PlanningCardName,
                IsPlaceholderAllocation = resourceAllocation.IsPlaceholderAllocation,
                IncludeInCapacityReporting = resourceAllocation.IncludeInCapacityReporting
            };
        }

        private ResourceAllocation CloneResourceAllocation(string employeeCode, string employeeName, string pdGrade, decimal fte,
            decimal billCode, int availability, int effectiveAvailability, Models.Workday.Office schedulingOffice, string position, string positionCode, string positionName, string positionGroupName,
            string serviceLineCode, string serviceLineName, DateTime hireDate, DateTime? terminationDate,
            DateTime startDate, DateTime endDate, Constants.EmployeeStatus employeeStatusCode)
        {
            int.TryParse(schedulingOffice?.OfficeCode, out var officeCode);
            return new ResourceAllocation
            {
                EmployeeCode = employeeCode,
                EmployeeStatusCode = employeeStatusCode,
                EmployeeName = employeeName,
                Fte = fte,
                PositionCode = positionCode,
                PositionName = positionName,
                PositionGroupName = positionGroupName,
                ServiceLineCode = serviceLineCode,
                ServiceLineName = serviceLineName,
                CurrentLevelGrade = pdGrade,
                BillCode = billCode,
                OperatingOfficeCode = officeCode,
                OperatingOfficeAbbreviation = schedulingOffice?.OfficeAbbreviation,
                OperatingOfficeName = schedulingOffice?.OfficeName,
                HireDate = hireDate,
                TerminationDate = terminationDate,
                StartDate = startDate,
                EndDate = endDate,
                Availability = availability,
                EffectiveAvailability = effectiveAvailability,
                LastUpdatedBy = "Enqueue Job",
                // TODO: Delete once Analplan is provided with API
                Position = position
            };
        }

        private IEnumerable<ResourceAllocation> SplitAllocationForpendingTransactions(
            ResourceAllocation resourceAllocation, IEnumerable<ResourceTransaction> resourcePendingTransactions)
        {
            var resourceAllocations = SplitAllocationForPendingPromotion(Enumerable.Repeat(resourceAllocation, 1),
                resourcePendingTransactions.FirstOrDefault(x => x.Type == "Promotion" && x.StartDate != null));
            resourceAllocations = SplitAllocationForPendingTransfers(resourceAllocations,
                resourcePendingTransactions.FirstOrDefault(x => x.Type == "Transfer" && x.StartDate != null));
            resourceAllocations = SplitAllocationForPendingTransitions(resourceAllocations,
                resourcePendingTransactions.FirstOrDefault(x => x.Type == "Transition" && x.StartDate != null));
            resourceAllocations = SplitAllocationForPendingTermination(resourceAllocations,
                resourcePendingTransactions.FirstOrDefault(x => x.Type == "Termination" && x.EndDate != null));
            return resourceAllocations;
        }

        private IEnumerable<ResourceAllocation> SplitAllocationForPendingPromotion(
            IEnumerable<ResourceAllocation> resourceAllocations, ResourceTransaction pendingPromotion)
        {
            if (pendingPromotion == null)
                return resourceAllocations;
            var allocations = new List<ResourceAllocation>();
            foreach (var allocation in resourceAllocations)
                if (IsPromotionEffectiveInAllocationDateRange(pendingPromotion, allocation))
                {
                    var startDatePriorPromotion = allocation.StartDate;
                    var endDatePriorPromotion = pendingPromotion.StartDate.Value.AddDays(-1);
                    var clonedResourceAllocation =
                        CloneResourceAllocation(allocation, startDatePriorPromotion, endDatePriorPromotion);
                    allocations.Add(clonedResourceAllocation);

                    var promotionStartDate = pendingPromotion.StartDate.Value.Date;
                    var effectiveEndDate = allocation.EndDate;
                    clonedResourceAllocation =
                        CloneResourceAllocation(allocation, promotionStartDate, effectiveEndDate);
                    clonedResourceAllocation.CurrentLevelGrade = pendingPromotion.LevelGrade;
                    clonedResourceAllocation.BillCode = pendingPromotion.BillCode;
                    clonedResourceAllocation.Fte = pendingPromotion.FTE;
                    clonedResourceAllocation.OperatingOfficeAbbreviation =
                        pendingPromotion.OperatingOffice.OfficeAbbreviation;
                    clonedResourceAllocation.OperatingOfficeCode = pendingPromotion.OperatingOffice.OfficeCode;
                    clonedResourceAllocation.OperatingOfficeName = pendingPromotion.OperatingOffice.OfficeName;
                    clonedResourceAllocation.ServiceLineCode = pendingPromotion.ServiceLine?.ServiceLineCode;
                    clonedResourceAllocation.ServiceLineName = pendingPromotion.ServiceLine?.ServiceLineName;
                    clonedResourceAllocation.PositionCode = pendingPromotion.Position?.PositionCode;
                    clonedResourceAllocation.PositionName = pendingPromotion.Position?.PositionName;
                    clonedResourceAllocation.PositionGroupName = pendingPromotion.Position?.PositionGroupName;
                    clonedResourceAllocation.Position = pendingPromotion.Position?.PositionGroupName;
                    clonedResourceAllocation.TransactionType = pendingPromotion.Type;
                    allocations.Add(clonedResourceAllocation);
                }
                else if (IsPromotionStartedBeforeAllocationDateRange(pendingPromotion, allocation))
                {
                    allocation.OperatingOfficeAbbreviation = pendingPromotion.OperatingOffice.OfficeAbbreviation;
                    allocation.OperatingOfficeCode = pendingPromotion.OperatingOffice.OfficeCode;
                    allocation.OperatingOfficeName = pendingPromotion.OperatingOffice.OfficeName;
                    allocation.CurrentLevelGrade = pendingPromotion.LevelGrade;
                    allocation.BillCode = pendingPromotion.BillCode;
                    allocation.Fte = pendingPromotion.FTE;
                    allocation.ServiceLineCode = pendingPromotion.ServiceLine?.ServiceLineCode;
                    allocation.ServiceLineName = pendingPromotion.ServiceLine?.ServiceLineName;
                    allocation.PositionCode = pendingPromotion.Position?.PositionCode;
                    allocation.PositionName = pendingPromotion.Position?.PositionName;
                    allocation.PositionGroupName = pendingPromotion.Position?.PositionGroupName;
                    allocation.Position = pendingPromotion.Position?.PositionGroupName;
                    allocation.TransactionType = pendingPromotion.Type;
                    allocations.Add(allocation);
                }
                else
                {
                    allocations.Add(allocation);
                }

            return allocations;
        }

        private static bool IsPromotionEffectiveInAllocationDateRange(ResourceTransaction pendingPromotion,
            ResourceAllocation allocation)
        {
            return pendingPromotion.StartDate.Value.Date >= allocation.StartDate &&
                   pendingPromotion.StartDate.Value.Date <= allocation.EndDate;
        }

        private static bool IsPromotionStartedBeforeAllocationDateRange(ResourceTransaction pendingPromotion,
            ResourceAllocation allocation)
        {
            return pendingPromotion.StartDate.Value.Date < allocation.StartDate;
        }

        private List<ResourceAllocation> SplitAllocationForLoAs(List<ResourceAllocation> allocations,
            IEnumerable<ResourceLoA> resourceLoAs)
        {
            var resourceAllocations = new List<ResourceAllocation>();
            foreach (var resourceLoA in resourceLoAs)
            {
                var allocationsOverlappedWithLoA = allocations
                    .Where(x => x.StartDate <= resourceLoA.EndDate && x.EndDate >= resourceLoA.StartDate);
                resourceAllocations.AddRange(allocations.Except(allocationsOverlappedWithLoA));
                foreach (var allocation in allocationsOverlappedWithLoA)
                {
                    updateAnlayticsDataForLoA(resourceLoA, resourceAllocations, allocation);
                }
                allocations = resourceAllocations;
                resourceAllocations = new List<ResourceAllocation>();
            }
            return allocations;
        }

        private void updateAnlayticsDataForLoA(ResourceLoA resourceLoA, List<ResourceAllocation> resourceAllocations, ResourceAllocation allocation)
        {
            if (!IsLoAOverlapsWithAllocation(resourceLoA, allocation))
            {
                resourceAllocations.Add(allocation);
                return;
            }

            if (IsLoAOverlapsAllocationDateRange(resourceLoA, allocation))
            {
                UpdateAllocationWithLoA(resourceAllocations, allocation, resourceLoA);
            }

            else if (IsLoAStartingBeforeAllocationDateRange(resourceLoA, allocation))
            {
                var loaStartedBeforeAllocation = resourceLoA;

                var startDate = allocation.StartDate;
                var endDate = loaStartedBeforeAllocation.EndDate.Value.Date;

                var clonedResourceAllocation =
                    CloneResourceAllocation(allocation, startDate, endDate);

                UpdateAllocationWithLoA(resourceAllocations, clonedResourceAllocation, loaStartedBeforeAllocation);

                if (endDate < allocation.EndDate)
                {
                    clonedResourceAllocation =
                        CloneResourceAllocation(allocation, loaStartedBeforeAllocation.EndDate.Value.Date.AddDays(1), allocation.EndDate);
                    updateAnlayticsDataForLoA(resourceLoA, resourceAllocations, clonedResourceAllocation);
                }
            }

            else if (IsLoALiesInAllocationDateRange(resourceLoA, allocation))
            {
                UpdateAllocationWithLoALiesInAllocationDateRange(resourceAllocations, allocation, resourceLoA);
            }

            else if (IsLoAStartedInAllocationDateRange(resourceLoA, allocation))
            {
                var loa = resourceLoA;

                var startDate = allocation.StartDate;
                var endDate = loa.StartDate.Value.Date.AddDays(-1);
                var allocationNotCoveredByLoA =
                    CloneResourceAllocation(allocation, startDate, endDate);
                resourceAllocations.Add(allocationNotCoveredByLoA);

                var clonedResourceAllocation =
                    CloneResourceAllocation(allocation, loa.StartDate.Value.Date, allocation.EndDate);

                UpdateAllocationWithLoA(resourceAllocations, clonedResourceAllocation, loa);

            }
        }

        private static bool IsLoAOverlapsWithAllocation(ResourceLoA resourceLoA, ResourceAllocation allocation)
        {
            return resourceLoA.StartDate <= allocation.EndDate && resourceLoA.EndDate >= allocation.StartDate;
        }

        private void UpdateAllocationWithLoALiesInAllocationDateRange(List<ResourceAllocation> resourceAllocations,
            ResourceAllocation allocation, ResourceLoA resourceLoA)
        {
            var loALiesInAllocation = resourceLoA;
            var clonedResourceAllocationLiesInloaRange = new ResourceAllocation();

            if (allocation.StartDate < loALiesInAllocation.StartDate.Value.Date)
            {
                clonedResourceAllocationLiesInloaRange =
                CloneResourceAllocation(allocation, allocation.StartDate, loALiesInAllocation.StartDate.Value.Date.AddDays(-1));
                resourceAllocations.Add(clonedResourceAllocationLiesInloaRange);
            }

            clonedResourceAllocationLiesInloaRange =
            CloneResourceAllocation(allocation, loALiesInAllocation.StartDate.Value.Date, loALiesInAllocation.EndDate.Value.Date);

            UpdateAllocationWithLoA(resourceAllocations, clonedResourceAllocationLiesInloaRange, loALiesInAllocation);

            if (allocation.EndDate > loALiesInAllocation.EndDate.Value.Date)
            {
                allocation =
                    CloneResourceAllocation(allocation, loALiesInAllocation.EndDate.Value.Date.AddDays(1), allocation.EndDate);
            }
            else
            {
                allocation = null;
            }


            if (allocation != null)
            {
                updateAnlayticsDataForLoA(resourceLoA, resourceAllocations, allocation);
            }
        }

        private static void UpdateAllocationWithLoA(List<ResourceAllocation> resourceAllocations,
            ResourceAllocation clonedResourceAllocation, ResourceLoA resourceLoA)
        {
            clonedResourceAllocation.EffectiveCost = 0;
            clonedResourceAllocation.EffectiveCostReason = clonedResourceAllocation.EffectiveCostReason.ConcatenateIfNotExists("LOA");
            clonedResourceAllocation.EffectiveOpportunityCost = 0;
            clonedResourceAllocation.EffectiveAvailability = 0;
            clonedResourceAllocation.EffectiveOpportunityCostReason = clonedResourceAllocation.EffectiveOpportunityCostReason.ConcatenateIfNotExists("LOA");
            clonedResourceAllocation.EffectiveAvailabilityReason = clonedResourceAllocation.EffectiveAvailabilityReason.ConcatenateIfNotExists("LOA");
            // Employee status code updated as per precedence. 1 > 2 > 3 > 4 > 5 > 6
            var effectiveEmployeeStatus = string.IsNullOrEmpty(resourceLoA.Description) || resourceLoA.Description.Contains("Unpaid")
                ? Constants.EmployeeStatus.LOAUnpaid
                : Constants.EmployeeStatus.LOAPaid;
            clonedResourceAllocation.EmployeeStatusCode = (int)effectiveEmployeeStatus < (int)clonedResourceAllocation.EmployeeStatusCode
                ? effectiveEmployeeStatus : clonedResourceAllocation.EmployeeStatusCode;

            resourceAllocations.Add(clonedResourceAllocation);
        }

        private static bool IsLoALiesInAllocationDateRange(ResourceLoA resourceLoA,
            ResourceAllocation allocation)
        {
            return resourceLoA.StartDate.Value.Date >= allocation.StartDate && resourceLoA.EndDate.Value.Date <= allocation.EndDate;
        }

        private static bool IsLoAStartingBeforeAllocationDateRange(ResourceLoA resourceLoA,
            ResourceAllocation allocation)
        {
            return resourceLoA.StartDate.Value.Date < allocation.StartDate && resourceLoA.EndDate.Value.Date >= allocation.StartDate &&
                resourceLoA.EndDate.Value.Date < allocation.EndDate;
        }

        private static bool IsLoAOverlapsAllocationDateRange(ResourceLoA resourceLoA,
            ResourceAllocation allocation)
        {
            return resourceLoA.StartDate.Value.Date <= allocation.StartDate && resourceLoA.EndDate.Value.Date >= allocation.EndDate;
        }

        private static bool IsLoAStartedInAllocationDateRange(ResourceLoA resourceLoA,
            ResourceAllocation allocation)
        {
            return resourceLoA.StartDate.Value.Date > allocation.StartDate && resourceLoA.StartDate.Value.Date <= allocation.EndDate &&
                resourceLoA.EndDate.Value.Date > allocation.EndDate;
        }

        private static bool IsCommitmentOverlapsAllocationDateRange(ResourceCommitment commitment, ResourceAllocation allocation)
        {
            return commitment.StartDate.Date <= allocation.StartDate && commitment.EndDate.Date >= allocation.EndDate;
        }
        private static bool IsCommitmentLiesInAllocationDateRange(ResourceCommitment commitment,
            ResourceAllocation allocation)
        {
            return commitment.StartDate.Date > allocation.StartDate && commitment.EndDate.Date < allocation.EndDate;
        }

        private static bool IsCommitmentStartingBeforeAllocationDateRange(ResourceCommitment commitment,
            ResourceAllocation allocation)
        {
            return commitment.StartDate.Date < allocation.StartDate && commitment.EndDate.Date >= allocation.StartDate &&
                commitment.EndDate.Date <= allocation.EndDate;
        }
        private static bool IsCommitmentStartedInAllocationDateRange(ResourceCommitment commitment,
          ResourceAllocation allocation)
        {
            return commitment.StartDate.Date > allocation.StartDate && commitment.StartDate.Date <= allocation.EndDate &&
                commitment.EndDate.Date >= allocation.EndDate;
        }

        private static bool IsCommitmentEndingInAllocationDateRange(ResourceCommitment commitment,
         ResourceAllocation allocation)
        {
            return commitment.StartDate.Date <= allocation.StartDate && commitment.EndDate.Date < allocation.EndDate;
        }

        private void UpdateAllocationWithCommitmentLiesInAllocationDateRange(List<ResourceAllocation> resourceAllocations,
           ResourceAllocation allocation, ResourceCommitment commitment)
        {
            var clonedResourceAllocationBeforeCommitmentStarts =
            CloneResourceAllocation(allocation, allocation.StartDate, commitment.StartDate.Date.AddDays(-1));
            resourceAllocations.Add(clonedResourceAllocationBeforeCommitmentStarts);

            var clonedResourceAllocation =
            CloneResourceAllocation(allocation, commitment.StartDate.Date, commitment.EndDate.Date);
            UpdateAllocationWithCommitment(resourceAllocations, commitment, clonedResourceAllocation);

            var clonedResourceAllocationAfterCommitmentEnds =
                   CloneResourceAllocation(allocation, commitment.EndDate.Date.AddDays(1), allocation.EndDate);
            resourceAllocations.Add(clonedResourceAllocationAfterCommitmentEnds);

        }

        private IEnumerable<ResourceAllocation> SplitAllocationForPendingTransfers(
            IEnumerable<ResourceAllocation> resourceAllocations, ResourceTransaction pendingTransfer)
        {
            if (pendingTransfer == null) return resourceAllocations;
            var allocations = new List<ResourceAllocation>();
            foreach (var allocation in resourceAllocations)
                if (IsTransferStartingInBetweenAllocationDateRange(pendingTransfer, allocation))
                {
                    var startDatePriorTransfer = allocation.StartDate;
                    var endDatePriorTransfer = pendingTransfer.StartDate.Value.AddDays(-1);
                    var clonedResourceAllocation =
                        CloneResourceAllocation(allocation, startDatePriorTransfer, endDatePriorTransfer);
                    allocations.Add(clonedResourceAllocation);

                    var transferStartDate = pendingTransfer.StartDate.Value.Date;
                    var effectiveEndDate = allocation.EndDate;
                    clonedResourceAllocation = CloneResourceAllocation(allocation, transferStartDate, effectiveEndDate);
                    clonedResourceAllocation.CurrentLevelGrade = pendingTransfer.LevelGrade;
                    clonedResourceAllocation.BillCode = pendingTransfer.BillCode;
                    clonedResourceAllocation.Fte = pendingTransfer.FTE;
                    clonedResourceAllocation.OperatingOfficeAbbreviation =
                        pendingTransfer.OperatingOffice.OfficeAbbreviation;
                    clonedResourceAllocation.OperatingOfficeCode = pendingTransfer.OperatingOffice.OfficeCode;
                    clonedResourceAllocation.OperatingOfficeName = pendingTransfer.OperatingOffice.OfficeName;
                    clonedResourceAllocation.TransactionType = pendingTransfer.Type;
                    allocations.Add(clonedResourceAllocation);
                }

                else if (IsTransferStartedBeforeAllocationDateRange(pendingTransfer, allocation))
                {
                    allocation.OperatingOfficeAbbreviation = pendingTransfer.OperatingOffice.OfficeAbbreviation;
                    allocation.OperatingOfficeCode = pendingTransfer.OperatingOffice.OfficeCode;
                    allocation.OperatingOfficeName = pendingTransfer.OperatingOffice.OfficeName;
                    allocation.TransactionType = pendingTransfer.Type;
                    allocations.Add(allocation);
                }
                else
                {
                    allocations.Add(allocation);
                }

            return allocations;
        }

        private static bool IsTransferStartingInBetweenAllocationDateRange(ResourceTransaction pendingTransfer,
            ResourceAllocation allocation)
        {
            return pendingTransfer.StartDate.Value.Date >= allocation.StartDate &&
                   pendingTransfer.StartDate.Value.Date <= allocation.EndDate;
        }

        private static bool IsTransferStartedBeforeAllocationDateRange(ResourceTransaction pendingTransfer,
            ResourceAllocation allocation)
        {
            return pendingTransfer.StartDate.Value.Date < allocation.StartDate;
        }

        private IEnumerable<ResourceAllocation> SplitAllocationForPendingTransitions(
            IEnumerable<ResourceAllocation> resourceAllocations, ResourceTransaction pendingTransition)
        {
            if (pendingTransition == null) return resourceAllocations;
            var allocations = new List<ResourceAllocation>();
            foreach (var allocation in resourceAllocations)
                if (IsTransitionStartedBetweenAllocationDateRange(pendingTransition, allocation))
                {
                    var startDatePriorTransition = allocation.StartDate;
                    var endDatePriorTransition = pendingTransition.StartDate.Value.AddDays(-1);
                    var clonedResourceAllocation =
                        CloneResourceAllocation(allocation, startDatePriorTransition, endDatePriorTransition);
                    allocations.Add(clonedResourceAllocation);

                    var transitionStartDate = pendingTransition.StartDate.Value.Date;
                    var effectiveEndDate = allocation.EndDate;
                    clonedResourceAllocation =
                        CloneResourceAllocation(allocation, transitionStartDate, effectiveEndDate);
                    clonedResourceAllocation.TransactionType = pendingTransition.Type;
                    clonedResourceAllocation.EmployeeStatusCode = Constants.EmployeeStatus.Transition;
                    allocations.Add(clonedResourceAllocation);
                }

                else if (IsTransitionStartedBeforeAllocationDateRange(pendingTransition, allocation))
                {
                    allocation.TransactionType = pendingTransition.Type;
                    allocation.EmployeeStatusCode = Constants.EmployeeStatus.Transition;
                    allocations.Add(allocation);
                }
                else
                {
                    allocations.Add(allocation);
                }

            return allocations;
        }

        private static bool IsTransitionStartedBetweenAllocationDateRange(ResourceTransaction pendingTransition,
            ResourceAllocation allocation)
        {
            return pendingTransition.StartDate.Value.Date >= allocation.StartDate &&
                   pendingTransition.StartDate.Value.Date <= allocation.EndDate;
        }

        private static bool IsTransitionStartedBeforeAllocationDateRange(ResourceTransaction pendingTransition,
            ResourceAllocation allocation)
        {
            return pendingTransition.StartDate.Value.Date < allocation.StartDate;
        }

        private IEnumerable<ResourceAllocation> SplitAllocationForPendingTermination(
           IEnumerable<ResourceAllocation> resourceAllocations, ResourceTransaction pendingTermination)
        {
            if (pendingTermination == null)
                return resourceAllocations;
            var allocations = new List<ResourceAllocation>();
            foreach (var allocation in resourceAllocations)
                if (IsTerminationEffectiveInAllocationDateRange(pendingTermination, allocation))
                {
                    var startDatePriorTermination = allocation.StartDate;
                    var endDatePriorTermination = pendingTermination.EndDate.Value.AddDays(-1);
                    var clonedResourceAllocation =
                        CloneResourceAllocation(allocation, startDatePriorTermination, endDatePriorTermination);
                    allocations.Add(clonedResourceAllocation);

                    var terminationStartDate = pendingTermination.EndDate.Value.Date;
                    var effectiveEndDate = allocation.EndDate;
                    clonedResourceAllocation =
                        CloneResourceAllocation(allocation, terminationStartDate, effectiveEndDate);
                    clonedResourceAllocation.TransactionType = pendingTermination.Type;
                    clonedResourceAllocation.EmployeeStatusCode = Constants.EmployeeStatus.Terminated;
                    allocations.Add(clonedResourceAllocation);
                }
                else if (IsTerminationStartedBeforeAllocationDateRange(pendingTermination, allocation))
                {
                    allocation.TransactionType = pendingTermination.Type;
                    allocation.EmployeeStatusCode = Constants.EmployeeStatus.Terminated;
                    allocations.Add(allocation);
                }
                else
                {
                    allocations.Add(allocation);
                }

            return allocations;
        }

        private static bool IsTerminationEffectiveInAllocationDateRange(ResourceTransaction pendingTermination,
            ResourceAllocation allocation)
        {
            return pendingTermination.EndDate.Value.Date >= allocation.StartDate &&
                   pendingTermination.EndDate.Value.Date <= allocation.EndDate;
        }

        private static bool IsTerminationStartedBeforeAllocationDateRange(ResourceTransaction pendingTermination,
            ResourceAllocation allocation)
        {
            return pendingTermination.EndDate.Value.Date < allocation.StartDate;
        }

        public IEnumerable<AvailabilityDateRange> GetAvailabilityDateRangeToUpsertFromPreviousAndNewAllocationDates(IEnumerable<ResourceAllocation> previousAllocationDateRange, IEnumerable<ResourceAllocation> newAllocationsData)
        {
            var combinedData = previousAllocationDateRange.Concat(newAllocationsData);

            // Get the minimum start date for each employee
            var dateRangeToUpsert = combinedData
                .GroupBy(e => e.EmployeeCode)
                .Select(g => new AvailabilityDateRange
                {
                    EmployeeCode = g.Key,
                    StartDate = g.Min(e => e.StartDate),
                    EndDate = g.Max(e => e.EndDate.Value),
                });

            return dateRangeToUpsert;
        }

        private static DataTable CreateAssignmentDataTable(IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var resourceAllocationsDataTable = new DataTable();
            // Common to case and opp
            resourceAllocationsDataTable.Columns.Add("clientCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("clientName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("clientGroupCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("clientGroupName", typeof(string));
            // Opp related info
            resourceAllocationsDataTable.Columns.Add("pipelineId", typeof(Guid));
            resourceAllocationsDataTable.Columns.Add("opportunityName", typeof(string));
            // Case related info
            resourceAllocationsDataTable.Columns.Add("oldCaseCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("caseName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseTypeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("caseTypeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("managingOfficeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("managingOfficeAbbreviation", typeof(string));
            resourceAllocationsDataTable.Columns.Add("managingOfficeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billingOfficeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("billingOfficeAbbreviation", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billingOfficeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("pegCase", typeof(bool));
            resourceAllocationsDataTable.Columns.Add("primaryIndustryTermCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("primaryIndustryTagId", typeof(string));
            resourceAllocationsDataTable.Columns.Add("primaryIndustry", typeof(string));
            resourceAllocationsDataTable.Columns.Add("practiceAreaIndustryCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("practiceAreaIndustry", typeof(string));
            resourceAllocationsDataTable.Columns.Add("primaryCapabilityTermCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("primaryCapabilityTagId", typeof(string));
            resourceAllocationsDataTable.Columns.Add("primaryCapability", typeof(string));
            resourceAllocationsDataTable.Columns.Add("practiceAreaCapabilityCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("practiceAreaCapability", typeof(string));
            // Employee related info
            resourceAllocationsDataTable.Columns.Add("employeeCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("employeeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("employeeStatusCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("fte", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("serviceLineCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("serviceLineName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionGroupName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("currentLevelGrade", typeof(string));
            resourceAllocationsDataTable.Columns.Add("operatingOfficeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            resourceAllocationsDataTable.Columns.Add("operatingOfficeName", typeof(string));
            // Staffing related info
            resourceAllocationsDataTable.Columns.Add("id", typeof(Guid));
            resourceAllocationsDataTable.Columns.Add("allocation", typeof(short));
            resourceAllocationsDataTable.Columns.Add("startDate", typeof(DateTime));
            resourceAllocationsDataTable.Columns.Add("endDate", typeof(DateTime));
            resourceAllocationsDataTable.Columns.Add("investmentCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("investmentName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseRoleCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseRoleName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("notes", typeof(string));
            resourceAllocationsDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            // Finance related info
            resourceAllocationsDataTable.Columns.Add("billRate", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("billCode", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("billRateType", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billRateCurrency", typeof(string));
            resourceAllocationsDataTable.Columns.Add("actualCost", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("actualCostInUSD", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("costUSDEffectiveYear", typeof(int));
            resourceAllocationsDataTable.Columns.Add("usdRate", typeof(decimal));
            // Commitment related info
            resourceAllocationsDataTable.Columns.Add("priorityCommitmentTypeCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("priorityCommitmentTypeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("commitmentTypeCodes", typeof(string));
            resourceAllocationsDataTable.Columns.Add("commitmentTypeReasonCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("commitmentTypeReasonName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("ringfence", typeof(string));
            resourceAllocationsDataTable.Columns.Add("isStaffingTag", typeof(bool));
            resourceAllocationsDataTable.Columns.Add("isOverriddenInSource", typeof(bool));
            // TODO: Delete
            resourceAllocationsDataTable.Columns.Add("position", typeof(string));//TODO: remove after anaplan changes
            resourceAllocationsDataTable.Columns.Add("effectiveCost", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("effectiveCostReason", typeof(string));

            foreach (var allocations in resourceAllocations)
            {
                var row = resourceAllocationsDataTable.NewRow();
                // Common to case and opp
                row["clientCode"] = allocations.ClientCode;
                row["clientName"] = (object)allocations.ClientName ?? DBNull.Value;
                row["clientGroupCode"] = (object)allocations.ClientGroupCode ?? DBNull.Value;
                row["clientGroupName"] = (object)allocations.ClientGroupName ?? DBNull.Value;
                // Opp related info
                row["pipelineId"] = (object)allocations.PipelineId ?? DBNull.Value;
                row["opportunityName"] = (object)allocations.OpportunityName ?? DBNull.Value;
                // Case related info
                row["oldCaseCode"] = (object)allocations.OldCaseCode ?? DBNull.Value;
                row["caseCode"] = (object)allocations.CaseCode ?? DBNull.Value;
                row["caseName"] = (object)allocations.CaseName ?? DBNull.Value;
                row["caseTypeCode"] = (object)allocations.CaseTypeCode ?? DBNull.Value;
                row["caseTypeName"] = (object)allocations.CaseTypeName ?? DBNull.Value;
                row["managingOfficeCode"] = (object)allocations.ManagingOfficeCode ?? DBNull.Value;
                row["managingOfficeAbbreviation"] = (object)allocations.ManagingOfficeAbbreviation ?? DBNull.Value;
                row["managingOfficeName"] = (object)allocations.ManagingOfficeName ?? DBNull.Value;
                row["billingOfficeCode"] = (object)allocations.BillingOfficeCode ?? DBNull.Value;
                row["billingOfficeAbbreviation"] = (object)allocations.BillingOfficeAbbreviation ?? DBNull.Value;
                row["billingOfficeName"] = (object)allocations.BillingOfficeName ?? DBNull.Value;
                row["pegCase"] = (object)allocations.PegCase ?? DBNull.Value;
                row["primaryIndustryTermCode"] = (object)allocations.PrimaryIndustryTermCode ?? DBNull.Value;
                row["primaryIndustryTagId"] = (object)allocations.PrimaryIndustryTagId ?? DBNull.Value;
                row["primaryIndustry"] = (object)allocations.PrimaryIndustry ?? DBNull.Value;
                row["practiceAreaIndustryCode"] = (object)allocations.PracticeAreaIndustryCode ?? DBNull.Value;
                row["practiceAreaIndustry"] = (object)allocations.PracticeAreaIndustry ?? DBNull.Value;
                row["primaryCapabilityTermCode"] = (object)allocations.PrimaryCapabilityTermCode ?? DBNull.Value;
                row["primaryCapabilityTagId"] = (object)allocations.PrimaryCapabilityTagId ?? DBNull.Value;
                row["primaryCapability"] = (object)allocations.PrimaryCapability ?? DBNull.Value;
                row["practiceAreaCapabilityCode"] = (object)allocations.PracticeAreaCapabilityCode ?? DBNull.Value;
                row["practiceAreaCapability"] = (object)allocations.PracticeAreaCapability ?? DBNull.Value;
                // Employee related info
                row["employeeCode"] = allocations.EmployeeCode;
                row["employeeName"] = (object)allocations.EmployeeName ?? DBNull.Value;
                row["employeeStatusCode"] = (object)((int)allocations.EmployeeStatusCode) ?? DBNull.Value;
                row["fte"] = allocations.Fte;
                row["serviceLineCode"] = (object)allocations.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)allocations.ServiceLineName ?? DBNull.Value;
                row["positionCode"] = (object)allocations.PositionCode ?? DBNull.Value;
                row["positionName"] = (object)allocations.PositionName ?? DBNull.Value;
                row["positionGroupName"] = (object)allocations.PositionGroupName ?? DBNull.Value;
                row["currentLevelGrade"] = allocations.CurrentLevelGrade;
                row["operatingOfficeCode"] = allocations.OperatingOfficeCode;
                row["operatingOfficeAbbreviation"] = (object)allocations.OperatingOfficeAbbreviation ?? DBNull.Value;
                row["operatingOfficeName"] = (object)allocations.OperatingOfficeName ?? DBNull.Value;
                // Staffing Related info
                row["id"] = (object)allocations.Id ?? DBNull.Value;
                row["allocation"] = allocations.Allocation;
                row["startDate"] = allocations.StartDate;
                row["endDate"] = allocations.EndDate;
                row["investmentCode"] = (object)allocations.InvestmentCode ?? DBNull.Value;
                row["investmentName"] = (object)allocations.InvestmentName ?? DBNull.Value;
                row["caseRoleCode"] = (object)allocations.CaseRoleCode ?? DBNull.Value;
                row["caseRoleName"] = (object)allocations.CaseRoleName ?? DBNull.Value;
                row["notes"] = allocations.Notes;
                row["lastUpdatedBy"] = allocations.LastUpdatedBy;
                // Finance related info
                row["billRate"] = (object)allocations.BillRate ?? DBNull.Value;
                row["billCode"] = (object)allocations.BillCode ?? DBNull.Value;
                row["billRateType"] = (object)allocations.BillRateType ?? DBNull.Value;
                row["billRateCurrency"] = (object)allocations.BillRateCurrency ?? DBNull.Value;
                row["actualCost"] = (object)allocations.ActualCost ?? DBNull.Value;
                row["actualCostInUSD"] = (object)allocations.CostInUSD ?? DBNull.Value;
                row["costUSDEffectiveYear"] = (object)allocations.CostUSDEffectiveYear ?? DBNull.Value;
                row["usdRate"] = (object)allocations.UsdRate ?? DBNull.Value;
                // Commitment related info
                row["priorityCommitmentTypeCode"] = (object)allocations.PriorityCommitmentTypeCode ?? DBNull.Value;
                row["priorityCommitmentTypeName"] = (object)allocations.PriorityCommitmentTypeName ?? DBNull.Value;
                row["commitmentTypeCodes"] = (object)allocations.CommitmentTypeCodes ?? DBNull.Value;
                row["commitmentTypeReasonCode"] = (object)allocations.CommitmentTypeReasonCode ?? DBNull.Value;
                row["commitmentTypeReasonName"] = (object)allocations.CommitmentTypeReasonName ?? DBNull.Value;
                row["ringfence"] = (object)allocations.Ringfence ?? DBNull.Value;
                row["isStaffingTag"] = (object)allocations.isStaffingTag ?? DBNull.Value;
                row["isOverriddenInSource"] = (object)allocations.isOverriddenInSource ?? DBNull.Value;
                //TODO : Delete
                row["position"] = (object)allocations.Position ?? DBNull.Value; //TODO: remove after Anaplan comunication
                row["effectiveCost"] = (object)allocations.EffectiveCost ?? DBNull.Value;
                row["effectiveCostReason"] = (object)allocations.EffectiveCostReason ?? DBNull.Value;
                resourceAllocationsDataTable.Rows.Add(row);
            }

            return resourceAllocationsDataTable;
        }

        private static DataTable CreateAvailabilityDataTable(IEnumerable<ResourceAvailability> resourceAvailabilities)
        {
            var availabilityDataTable = new DataTable();
            // Employee related info
            availabilityDataTable.Columns.Add("employeeCode", typeof(string));
            availabilityDataTable.Columns.Add("employeeName", typeof(string));
            availabilityDataTable.Columns.Add("employeeStatusCode", typeof(short));
            availabilityDataTable.Columns.Add("fte", typeof(decimal));
            availabilityDataTable.Columns.Add("operatingOfficeCode", typeof(short));
            availabilityDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            availabilityDataTable.Columns.Add("operatingOfficeName", typeof(string));
            availabilityDataTable.Columns.Add("currentLevelGrade", typeof(string));
            availabilityDataTable.Columns.Add("serviceLineCode", typeof(string));
            availabilityDataTable.Columns.Add("serviceLineName", typeof(string));
            availabilityDataTable.Columns.Add("positionCode", typeof(string));
            availabilityDataTable.Columns.Add("positionName", typeof(string));
            availabilityDataTable.Columns.Add("positionGroupName", typeof(string));
            availabilityDataTable.Columns.Add("hireDate", typeof(DateTime));
            availabilityDataTable.Columns.Add("terminationDate", typeof(DateTime));
            availabilityDataTable.Columns.Add("billCode", typeof(decimal));
            // Staffing related info
            availabilityDataTable.Columns.Add("startDate", typeof(DateTime));
            availabilityDataTable.Columns.Add("endDate", typeof(DateTime));
            availabilityDataTable.Columns.Add("availability", typeof(short));
            availabilityDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            // Finance related info
            availabilityDataTable.Columns.Add("billRate", typeof(decimal));
            availabilityDataTable.Columns.Add("billRateType", typeof(string));
            availabilityDataTable.Columns.Add("billRateCurrency", typeof(string));
            availabilityDataTable.Columns.Add("opportunityCost", typeof(decimal));
            availabilityDataTable.Columns.Add("opportunityCostInUSD", typeof(decimal));
            availabilityDataTable.Columns.Add("costUSDEffectiveYear", typeof(int));
            availabilityDataTable.Columns.Add("usdRate", typeof(decimal));
            // Commitment related info
            availabilityDataTable.Columns.Add("priorityCommitmentTypeCode", typeof(string));
            availabilityDataTable.Columns.Add("priorityCommitmentTypeName", typeof(string));
            availabilityDataTable.Columns.Add("commitmentTypeCodes", typeof(string));
            availabilityDataTable.Columns.Add("commitmentTypeReasonCode", typeof(string));
            availabilityDataTable.Columns.Add("commitmentTypeReasonName", typeof(string));
            availabilityDataTable.Columns.Add("ringfence", typeof(string));
            availabilityDataTable.Columns.Add("isStaffingTag", typeof(bool));
            availabilityDataTable.Columns.Add("isOverriddenInSource", typeof(bool));
            // TODO: Delete
            availabilityDataTable.Columns.Add("position", typeof(string));
            availabilityDataTable.Columns.Add("effectiveAvailability", typeof(short));
            availabilityDataTable.Columns.Add("effectiveAvailabilityReason", typeof(string));
            availabilityDataTable.Columns.Add("effectiveOpportunityCost", typeof(decimal));
            availabilityDataTable.Columns.Add("effectiveOpportunityCostReason", typeof(string));

            foreach (var availability in resourceAvailabilities)
            {
                var row = availabilityDataTable.NewRow();
                // Employee related info
                row["employeeCode"] = availability.EmployeeCode;
                row["employeeName"] = (object)availability.EmployeeName ?? DBNull.Value;
                row["employeeStatusCode"] = (object)((int)availability.EmployeeStatusCode) ?? DBNull.Value;
                row["fte"] = availability.Fte;
                row["operatingOfficeCode"] = availability.OperatingOfficeCode;
                row["operatingOfficeAbbreviation"] = (object)availability.OperatingOfficeAbbreviation ?? DBNull.Value;
                row["operatingOfficeName"] = (object)availability.OperatingOfficeName ?? DBNull.Value;
                row["currentLevelGrade"] = availability.CurrentLevelGrade;
                row["serviceLineCode"] = (object)availability.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)availability.ServiceLineName ?? DBNull.Value;
                row["positionCode"] = (object)availability.PositionCode ?? DBNull.Value;
                row["positionName"] = (object)availability.PositionName ?? DBNull.Value;
                row["positionGroupName"] = (object)availability.PositionGroupName ?? DBNull.Value;
                row["hireDate"] = availability.HireDate;
                row["terminationDate"] = (object)availability.TerminationDate ?? DBNull.Value;
                row["billCode"] = availability.BillCode;
                // Staffing related info
                row["startDate"] = availability.StartDate;
                row["endDate"] = availability.EndDate;
                row["availability"] = availability.Availability;
                row["lastUpdatedBy"] = availability.LastUpdatedBy;
                // Finance related info
                row["billRate"] = (object)availability.BillRate ?? DBNull.Value;
                row["billRateType"] = (object)availability.BillRateType ?? DBNull.Value;
                row["billRateCurrency"] = (object)availability.BillRateCurrency ?? DBNull.Value;
                row["opportunityCost"] = availability.OpportunityCost;
                row["opportunityCostInUSD"] = (object)availability.OpportunityCostInUSD ?? DBNull.Value;
                row["costUSDEffectiveYear"] = (object)availability.CostUSDEffectiveYear ?? DBNull.Value;
                row["usdRate"] = (object)availability.UsdRate ?? DBNull.Value;
                // Commitment related info
                row["priorityCommitmentTypeCode"] = (object)availability.PriorityCommitmentTypeCode ?? DBNull.Value;
                row["priorityCommitmentTypeName"] = (object)availability.PriorityCommitmentTypeName ?? DBNull.Value;
                row["commitmentTypeCodes"] = (object)availability.CommitmentTypeCodes ?? DBNull.Value;
                row["commitmentTypeReasonCode"] = (object)availability.CommitmentTypeReasonCode ?? DBNull.Value;
                row["commitmentTypeReasonName"] = (object)availability.CommitmentTypeReasonName ?? DBNull.Value;
                row["ringfence"] = (object)availability.Ringfence ?? DBNull.Value;
                row["isStaffingTag"] = (object)availability.isStaffingTag ?? DBNull.Value;
                row["isOverriddenInSource"] = (object)availability.isOverriddenInSource ?? DBNull.Value;
                // TODO: Delete
                row["position"] = (object)availability.Position ?? DBNull.Value;
                row["effectiveAvailability"] = availability.EffectiveAvailability;
                row["effectiveAvailabilityReason"] = (object)availability.EffectiveAvailabilityReason ?? DBNull.Value;
                row["effectiveOpportunityCost"] = availability.EffectiveOpportunityCost;
                row["effectiveOpportunityCostReason"] = (object)availability.EffectiveOpportunityCostReason ?? DBNull.Value;
                availabilityDataTable.Rows.Add(row);
            }

            return availabilityDataTable;
        }

        private int GetThresholdForResourcesWithNoAvailabilityRecords()
        {
            var dateToday = DateTime.Now.Date;
            var extraDaysToAdd = 0;
            //Create data only till 1 year + next weekend so that RA data is not recreated everyday but only 1 once per week even when this job runs daily for less updates in RA table
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)dateToday.DayOfWeek + 7) % 7;
            if(daysUntilSunday == 0)
            {
                extraDaysToAdd = 7; //add extra 7 days if job runs on Sunday. On every other day, do recreate data for employee whose data is already present till next years weekend
            }
            else
            {
                extraDaysToAdd = daysUntilSunday;
            }

            var numberOfDaysInYear = Convert.ToInt32(ConfigurationUtility.GetValue("Threshold:dataAvailableForDays")) + extraDaysToAdd;
            var numberOfDaysInLeapYear = numberOfDaysInYear >= 365 ? numberOfDaysInYear + 1 : numberOfDaysInYear;

            var dataAvailableForDays = numberOfDaysInYear;
            

            //Check if this year is leap year and the date is before 29-Feb
            if (DateTime.IsLeapYear(dateToday.Year))
            {
                if (dateToday < Convert.ToDateTime("01-03-" + dateToday.Year))
                {
                    dataAvailableForDays = numberOfDaysInLeapYear;
                }
            }
            //Check if next year is leap year and the date is after 29-Feb
            else if (DateTime.IsLeapYear(dateToday.Year + 1))
            {
                if (dateToday >= Convert.ToDateTime("01-03-" + dateToday.Year))
                {
                    dataAvailableForDays = numberOfDaysInLeapYear;
                }
            }
            return dataAvailableForDays;
        }


        private static DataTable GetResourceAvailabilityAnalyticsDTO(
            IEnumerable<ResourceAvailabilityViewModel> resourcesWithNoAllocations)
        {
            var resourceAvailabilityDataTable = new DataTable();
            // Employee related info
            resourceAvailabilityDataTable.Columns.Add("employeeCode", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("employeeName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("employeeStatusCode", typeof(int));
            resourceAvailabilityDataTable.Columns.Add("fte", typeof(decimal));
            resourceAvailabilityDataTable.Columns.Add("operatingOfficeCode", typeof(int));
            resourceAvailabilityDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("operatingOfficeName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("currentLevelGrade", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("serviceLineCode", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("serviceLineName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("positionCode", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("positionName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("positionGroupName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("hireDate", typeof(DateTime));
            resourceAvailabilityDataTable.Columns.Add("terminationDate", typeof(DateTime));
            resourceAvailabilityDataTable.Columns.Add("billCode", typeof(decimal));
            // Staffing related info
            resourceAvailabilityDataTable.Columns.Add("startDate", typeof(DateTime));
            resourceAvailabilityDataTable.Columns.Add("endDate", typeof(DateTime));
            resourceAvailabilityDataTable.Columns.Add("availability", typeof(int));
            resourceAvailabilityDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            // Finance related info
            resourceAvailabilityDataTable.Columns.Add("billRate", typeof(decimal));
            resourceAvailabilityDataTable.Columns.Add("billRateType", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("billRateCurrency", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("opportunityCost", typeof(decimal));
            resourceAvailabilityDataTable.Columns.Add("opportunityCostInUSD", typeof(decimal));
            resourceAvailabilityDataTable.Columns.Add("costUSDEffectiveYear", typeof(int));
            resourceAvailabilityDataTable.Columns.Add("usdRate", typeof(decimal));
            // Commitment related info
            resourceAvailabilityDataTable.Columns.Add("priorityCommitmentTypeCode", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("priorityCommitmentTypeName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("commitmentTypeCodes", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("commitmentTypeReasonCode", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("commitmentTypeReasonName", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("ringfence", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("isStaffingTag", typeof(bool));
            resourceAvailabilityDataTable.Columns.Add("isOverriddenInSource", typeof(bool));
            // TODO: Dlete          
            resourceAvailabilityDataTable.Columns.Add("position", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("effectiveAvailability", typeof(int));
            resourceAvailabilityDataTable.Columns.Add("effectiveAvailabilityReason", typeof(string));
            resourceAvailabilityDataTable.Columns.Add("effectiveOpportunityCost", typeof(decimal));
            resourceAvailabilityDataTable.Columns.Add("effectiveOpportunityCostReason", typeof(string));


            foreach (var resource in resourcesWithNoAllocations)
            {
                var row = resourceAvailabilityDataTable.NewRow();
                // Employee related info
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
                row["hireDate"] = resource.HireDate;
                row["terminationDate"] = (object)resource.TerminationDate ?? DBNull.Value;
                row["billCode"] = resource.BillCode;
                // Staffing related info
                row["startDate"] = resource.StartDate;
                row["endDate"] = resource.EndDate;
                row["availability"] = resource.Availability;
                row["lastUpdatedBy"] = (object)resource.LastUpdatedBy ?? DBNull.Value;
                // Finance related info
                row["billRate"] = (object)resource.BillRate ?? DBNull.Value;
                row["billRateType"] = (object)resource.BillRateType ?? DBNull.Value;
                row["billRateCurrency"] = (object)resource.BillRateCurrency ?? DBNull.Value;
                row["opportunityCost"] = (object)resource.OpportunityCost ?? DBNull.Value;
                row["opportunityCostInUSD"] = (object)resource.OpportunityCostInUSD ?? DBNull.Value;
                row["costUSDEffectiveYear"] = (object)resource.costUSDEffectiveYear ?? DBNull.Value;
                row["usdRate"] = (object)resource.UsdRate ?? DBNull.Value;
                // Commitment related info
                row["priorityCommitmentTypeCode"] = (object)resource.PriorityCommitmentTypeCode ?? DBNull.Value;
                row["priorityCommitmentTypeName"] = (object)resource.PriorityCommitmentTypeName ?? DBNull.Value;
                row["commitmentTypeCodes"] = (object)resource.CommitmentTypeCodes ?? DBNull.Value;
                row["commitmentTypeReasonCode"] = (object)resource.CommitmentTypeReasonCode ?? DBNull.Value;
                row["commitmentTypeReasonName"] = (object)resource.CommitmentTypeReasonName ?? DBNull.Value;
                row["ringfence"] = (object)resource.Ringfence ?? DBNull.Value;
                row["isStaffingTag"] = (object)resource.isStaffingTag ?? DBNull.Value;
                row["isOverriddenInSource"] = (object)resource.isOverriddenInSource ?? DBNull.Value;
                // TODO: Delete
                row["position"] = (object)resource.Position ?? DBNull.Value;
                row["effectiveAvailability"] = (object)resource.EffectiveAvailability ?? DBNull.Value;
                row["effectiveAvailabilityReason"] = (object)resource.EffectiveAvailabilityReason ?? DBNull.Value;
                row["effectiveOpportunityCost"] = (object)resource.EffectiveOpportunityCost ?? DBNull.Value;
                row["effectiveOpportunityCostReason"] = (object)resource.EffectiveOpportunityCostReason ?? DBNull.Value;

                resourceAvailabilityDataTable.Rows.Add(row);
            }

            return resourceAvailabilityDataTable;
        }

        private static DataTable CreateEmployeeAvailabilityDateRangeDataTable(IEnumerable<AvailabilityDateRange> availabilityDateRangeToUpdate)
        {
            var availabilityDataTable = new DataTable();
            // Employee related info
            availabilityDataTable.Columns.Add("employeeCode", typeof(string));
            availabilityDataTable.Columns.Add("startDate", typeof(DateTime));
            availabilityDataTable.Columns.Add("endDate", typeof(DateTime));

            foreach (var resource in availabilityDateRangeToUpdate)
            {
                var row = availabilityDataTable.NewRow();
                // Employee related info
                row["employeeCode"] = (object)resource.EmployeeCode ?? DBNull.Value;
                row["startDate"] = (object)resource.StartDate ?? DBNull.Value;
                row["endDate"] = (object)resource.EndDate ?? DBNull.Value;

                availabilityDataTable.Rows.Add(row);
            }

            return availabilityDataTable;
        }
        #endregion
    }
}
