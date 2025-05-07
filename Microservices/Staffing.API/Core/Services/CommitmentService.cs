using Hangfire;
using Newtonsoft.Json.Linq;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class CommitmentService : ICommitmentService
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public CommitmentService(ICommitmentRepository commitmentRepository,
            IBackgroundJobClient backgroundJobClient)
        {
            _commitmentRepository = commitmentRepository;
            _backgroundJobClient = backgroundJobClient;
        }
        public async Task<IEnumerable<CommitmentType>> GetCommitmentTypeLookupList(bool? showHidden)
        {
            return await _commitmentRepository.GetCommitmentTypeLookupList(showHidden);
        }

        public async Task<IEnumerable<Commitment>> GetResourceCommitments(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
                Enumerable.Empty<Commitment>();
            if (effectiveFromDate != null && effectiveToDate != null && effectiveFromDate > effectiveToDate)
                throw new ArgumentException("EffectiveToDate should be greater than EffectiveFromDate");
            return await _commitmentRepository.GetResourceCommitments(employeeCode, effectiveFromDate, effectiveToDate);
        }

        public async Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsByIds(string commitmentIds)
        {
            if (string.IsNullOrEmpty(commitmentIds))
                Enumerable.Empty<CommitmentViewModel>();
            var commitmentData = await _commitmentRepository.GetResourceCommitmentsByIds(commitmentIds);
            var commitments = ConvertToCommitmentViewModel(commitmentData);

            return commitments;
        }
        public async Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsByDeletedIds(string commitmentIds)
        {
            if (string.IsNullOrEmpty(commitmentIds))
                Enumerable.Empty<CommitmentViewModel>();
            var commitmentData = await _commitmentRepository.GetResourceCommitmentsByDeletedIds(commitmentIds);
            var commitments = ConvertToCommitmentViewModel(commitmentData);

            return commitments;
        }

        public async Task DeleteResourceCommitmentById(Guid Id, string lastUpdatedBy)
        {
            if (Id == null || Id == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be null or empty");
            }
            if (string.IsNullOrEmpty(lastUpdatedBy))
            {
                throw new ArgumentException("lastUpdatedBy cannot be null or empty");
            }
            await _commitmentRepository.DeleteResourceCommitmentById(Id, lastUpdatedBy);

            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x => x.UpdateAnlayticsDataForDeletedCommitment(Id.ToString()));
        }

        public async Task DeleteResourceCommitmentByIds(string commitmentIds, string lastUpdatedBy)
        {
            if (commitmentIds == null)
            {
                throw new ArgumentException("commitmentIds cannot be null or empty");
            }
            if (string.IsNullOrEmpty(lastUpdatedBy))
            {
                throw new ArgumentException("lastUpdatedBy cannot be null or empty");
            }
            await _commitmentRepository.DeleteResourceCommitmentByIds(commitmentIds, lastUpdatedBy);

            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x => x.UpdateAnlayticsDataForDeletedCommitment(commitmentIds));
        }

        public async Task<IEnumerable<CommitmentViewModel>> GetCommitmentsWithinDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate == DateTime.MinValue)
                throw new ArgumentException("Start Date can not be null");
            if (endDate == DateTime.MinValue)
                throw new ArgumentException("End Date can not be null");
            if (endDate < startDate)
                throw new ArgumentException("End date should be greater than start date");

            var commitmentData = await
                _commitmentRepository.GetCommitmentsWithinDateRange(startDate, endDate);

            var commitments = ConvertToCommitmentViewModel(commitmentData);

            return commitments;
        }

        public async Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsWithinDateRangeByEmployees(string employeeCodes, DateTime? startDate,
                DateTime? endDate, string commitmentTypeCode)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<CommitmentViewModel>();

            var commitmentData = await
                _commitmentRepository.GetResourceCommitmentsWithinDateRangeByEmployees(employeeCodes, startDate, endDate, commitmentTypeCode);

            var commitments = ConvertToCommitmentViewModel(commitmentData);

            return commitments;
        }
        public async Task<(string, IEnumerable<CommitmentViewModel>)> GetCommitmentBySelectedValues(string commitmentTypeCodes, string employeeCodes, DateTime? startDate,
            DateTime? endDate, bool? ringfenceCommitmentsOnly, string clientId)
        {
            if (string.IsNullOrEmpty(commitmentTypeCodes) &&
                string.IsNullOrEmpty(employeeCodes) &&
                startDate == null &&
                endDate == null)
            {
                return ("Atleast one parameter is mandatory", Enumerable.Empty<CommitmentViewModel>());
            }

            if (!ValidateParamValuesByClient(clientId, commitmentTypeCodes, employeeCodes, startDate, endDate, ringfenceCommitmentsOnly,
                Constants.ApiEndPoints.GetCommitmentBySelectedValues))
            {
                return ("Mandatory params not provided", Enumerable.Empty<CommitmentViewModel>());
            }

            if (startDate == null && endDate != null)
            {
                return ("Error while getting resource data. Start Date should be provided with end date", Enumerable.Empty<CommitmentViewModel>());
            }
            var commitmentData = await _commitmentRepository.GetCommitmentBySelectedValues(commitmentTypeCodes, employeeCodes, startDate, endDate, ringfenceCommitmentsOnly);

            //not using the common function here as we cannot send notes data to other teams as it's sensistive data
            var commitments = commitmentData.Select(item => new CommitmentViewModel
            {
                Id = item.Id,
                EmployeeCode = item.EmployeeCode,
                CommitmentTypeCode = item.CommitmentType.CommitmentTypeCode,
                CommitmentTypeName = item.CommitmentType.CommitmentTypeName,
                Allocation = item.Allocation,
                StartDate = item.StartDate,
                EndDate = item.EndDate
            });

            return (string.Empty, commitments);
        }


        public async Task<IEnumerable<Commitment>> UpsertResourcesCommitments(IList<Commitment> resourcesCommitments)
        {
            if (resourcesCommitments.Count() < 1)
            {
                return Enumerable.Empty<Commitment>();
            }

            var resourcesCommitmentsDataTable = CreateCommitmentsTable(resourcesCommitments);

            var savedResourcesCommitments =
                await _commitmentRepository.UpsertResourcesCommitments(resourcesCommitmentsDataTable);

            var commitmentIds = string.Join(",", savedResourcesCommitments.Select(x => x.Id).Distinct());

            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x => x.UpdateAnlayticsDataForUpsertedCommitment(commitmentIds));

            return savedResourcesCommitments;
        }

        public async Task<CommitmentType> UpsertCommitmentTypes(CommitmentType ringfenceCommitments)
        {

            var savedRingfenceCommitments =
                await _commitmentRepository.UpsertCommitmentTypes(ringfenceCommitments);

            return savedRingfenceCommitments;
        }

        public async Task<bool> IsSTACommitmentCreated(string oldCaseCode = null, Guid? opportunityId = null, Guid? planningCardId = null)
        {
            var isOldCaseCodeProvided = !string.IsNullOrEmpty(oldCaseCode);
            var isOpportunityIdProvided = opportunityId != null && opportunityId != Guid.Empty;
            var isPlanningCardIdProvided = planningCardId != null && planningCardId != Guid.Empty;

            if (!isOldCaseCodeProvided && !isOpportunityIdProvided && !isPlanningCardIdProvided)
            {
                throw new ArgumentException("Atleast one of 'oldCaseCode, opportunityId or planningCardId' is required");
            }

            var isSTACommitmentCreated = await _commitmentRepository.IsSTACommitmentCreated(oldCaseCode, opportunityId, planningCardId);

            return isSTACommitmentCreated;
        }

        public async Task<IEnumerable<CaseOppCommitmentViewModel>> GetProjectSTACommitmentDetails( string oldCaseCodes, string opportunityIds, string planningCardIds)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) &&
                string.IsNullOrEmpty(opportunityIds) &&
                string.IsNullOrEmpty(planningCardIds))
            {
                throw new ArgumentException("At least one of oldCaseCodes, opportunityIds, or planningCardIds must be provided.");
            }

            return await _commitmentRepository.GetProjectSTACommitmentDetails(oldCaseCodes, opportunityIds, planningCardIds);
        }

        public async Task<IEnumerable<CommitmentWithCaseOppInfo>> UpsertCaseOppCommitments(IEnumerable<InsertCaseOppCommitmentViewModel> insertCaseOppCommitments)
        {
            if (insertCaseOppCommitments.Count() < 1)
            {
                return Enumerable.Empty<CommitmentWithCaseOppInfo>(); ;
            }

            var firstItem = insertCaseOppCommitments.FirstOrDefault();

            // Upsert Commitments
            var commitments = new List<Commitment>();
            foreach (var item in insertCaseOppCommitments)
            {
                var commitment = new Commitment
                {
                    EmployeeCode = item.EmployeeCode,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Allocation = item.Allocation,
                    Notes = item.Notes,
                    //IsSourceStaffing = item.IsSourceStaffing,
                    CommitmentType = new CommitmentType
                    {
                        CommitmentTypeCode = item.CommitmentTypeCode,
                    },
                    CommitmentTypeReasonCode = item.CommitmentTypeReasonCode,
                    LastUpdatedBy = item.LastUpdatedBy
                };
                commitments.Add(commitment);
            }

            // Upsert CaseOppCommitments
            var savedCommitments = await UpsertResourcesCommitments(commitments);
            var caseOppCommitments = new List<CaseOppCommitment>();
            foreach (var item in insertCaseOppCommitments)
            {
                var savedcommitment = savedCommitments.FirstOrDefault(
                    x => x.EmployeeCode == item.EmployeeCode
                    && x.StartDate == item.StartDate
                    && x.EndDate == item.EndDate);

                var caseOppCommitment = new CaseOppCommitment
                {
                    ScheduleId = item.ScheduleId,
                    CommitmentId = savedcommitment?.Id ?? Guid.Empty,
                    OldCaseCode = item.OldCaseCode,
                    OpportunityId = item.OpportunityId,
                    PlanningCardId = item.PlanningCardId,
                    LastUpdatedBy = item.LastUpdatedBy
                };
                
                caseOppCommitments.Add(caseOppCommitment);
            }

            var caseOppCommitmentsDataTable = CreateCaseOppCommitmentsTable(caseOppCommitments);

            await _commitmentRepository.UpsertCaseOppCommitments(caseOppCommitmentsDataTable);

            var enrichedCommitments = savedCommitments.Select(commitment => new CommitmentWithCaseOppInfo
            {
                Id = commitment.Id,
                EmployeeCode = commitment.EmployeeCode,
                StartDate = commitment.StartDate,
                EndDate = commitment.EndDate,
                Allocation = commitment.Allocation,
                Notes = commitment.Notes,
                CommitmentType = commitment.CommitmentType,
                CommitmentTypeReasonCode = commitment.CommitmentTypeReasonCode,
                LastUpdatedBy = commitment.LastUpdatedBy,
                OldCaseCode = firstItem.OldCaseCode,
                PlanningCardId = firstItem.PlanningCardId,
                OpportunityId = firstItem.OpportunityId
            });

            return enrichedCommitments;
        }

        public async Task DeleteCaseOppCommitments(string commitmentIds, string lastUpdatedBy)
        {
            if (commitmentIds == null)
            {
                throw new ArgumentException("commitmentIds cannot be null or empty");
            }
            if (string.IsNullOrEmpty(lastUpdatedBy))
            {
                throw new ArgumentException("lastUpdatedBy cannot be null or empty");
            }

            await DeleteResourceCommitmentByIds(commitmentIds, lastUpdatedBy);

            await _commitmentRepository.DeleteCaseOppCommitments(commitmentIds, lastUpdatedBy);
        }

        #region Private methods

        private bool ValidateParamValuesByClient(string clientId, string commitmentTypeCodes, string employeeCodes, DateTime? startDate,
            DateTime? endDate, bool? ringfenceCommitmentsOnly, string currentMethodName)
        {
            if (string.IsNullOrEmpty(clientId))
                return true;

            var allClientDetails = JObject.Parse(File.ReadAllText(ConfigurationUtility.GetValue("clientAccessFilePath")));

            var clientDetails = new TSGClientNameAndMandatoryParams()
            {
                ClientName = allClientDetails[clientId]?[Constants.ClientProperties.ClientName].ToString(),
                MandatoryParams = allClientDetails[clientId]?[currentMethodName][Constants.ClientProperties.MandatoryParams].ToString()
            };

            if (!string.IsNullOrEmpty(clientDetails?.MandatoryParams))
            {
                var mandatoryParams = clientDetails.MandatoryParams.Trim(',').Split(',');
                foreach (var parameter in mandatoryParams)
                {
                    switch (parameter)
                    {
                        case "commitmentTypeCodes":
                            if (string.IsNullOrEmpty(commitmentTypeCodes)) return false;
                            break;
                        case "employeeCodes":
                            if (string.IsNullOrEmpty(employeeCodes)) return false;
                            break;
                        case "startDate":
                            if (startDate == null || startDate == DateTime.MinValue) return false;
                            break;
                        case "endDate":
                            if (endDate == null || endDate == DateTime.MinValue) return false;
                            break;
                        case "ringfenceCommitmentsOnly":
                            if (!ringfenceCommitmentsOnly.HasValue || ringfenceCommitmentsOnly == false) return false;
                            break;
                        default:
                            return false;
                    }
                }
            }


            return true;
        }

        private DataTable CreateCommitmentsTable(IList<Commitment> resourcesCommitments)
        {
            var commitmentsDataTable = new DataTable();
            commitmentsDataTable.Columns.Add("id", typeof(Guid));
            commitmentsDataTable.Columns.Add("employeeCode", typeof(string));
            commitmentsDataTable.Columns.Add("commitmentTypeCode", typeof(string));
            commitmentsDataTable.Columns.Add("commitmentTypeReasonCode", typeof(string));
            commitmentsDataTable.Columns.Add("startDate", typeof(DateTime));
            commitmentsDataTable.Columns.Add("endDate", typeof(DateTime));
            commitmentsDataTable.Columns.Add("allocation", typeof(Int16));
            commitmentsDataTable.Columns.Add("notes", typeof(string));
            commitmentsDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var resourceCommitment in resourcesCommitments)
            {
                var row = commitmentsDataTable.NewRow();
                row["id"] = resourceCommitment.Id == Guid.Empty ? Guid.NewGuid() : (object)resourceCommitment.Id ?? DBNull.Value;
                row["employeeCode"] = (object)resourceCommitment.EmployeeCode ?? DBNull.Value;
                row["commitmentTypeCode"] = (object)resourceCommitment.CommitmentType.CommitmentTypeCode ?? DBNull.Value;
                row["commitmentTypeReasonCode"] = resourceCommitment.CommitmentTypeReasonCode;
                row["startDate"] = resourceCommitment.StartDate;
                row["endDate"] = resourceCommitment.EndDate;
                row["allocation"] = (object)resourceCommitment.Allocation ?? DBNull.Value;
                row["notes"] = (object)resourceCommitment.Notes ?? DBNull.Value;
                row["lastUpdatedBy"] = resourceCommitment.LastUpdatedBy;
                commitmentsDataTable.Rows.Add(row);
            }

            return commitmentsDataTable;
        }

        private DataTable CreateCaseOppCommitmentsTable(IList<CaseOppCommitment> caseOppCommitments)
        {
            var caseOppCommitmentsDataTable = new DataTable();
            caseOppCommitmentsDataTable.Columns.Add("scheduleId", typeof(Guid));
            caseOppCommitmentsDataTable.Columns.Add("commitmentId", typeof(Guid));
            caseOppCommitmentsDataTable.Columns.Add("oldCaseCode", typeof(string));
            caseOppCommitmentsDataTable.Columns.Add("opportunityId", typeof(Guid));
            caseOppCommitmentsDataTable.Columns.Add("planningCardId", typeof(Guid));
            caseOppCommitmentsDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var caseOppCommitment in caseOppCommitments)
            {
                var row = caseOppCommitmentsDataTable.NewRow();
                row["scheduleId"] = caseOppCommitment.ScheduleId == Guid.Empty ? Guid.NewGuid() : (object)caseOppCommitment.ScheduleId ?? DBNull.Value;
                row["commitmentId"] = caseOppCommitment.CommitmentId == Guid.Empty ? Guid.NewGuid() : (object)caseOppCommitment.CommitmentId ?? DBNull.Value;
                row["oldCaseCode"] = (object)caseOppCommitment.OldCaseCode ?? DBNull.Value;
                row["opportunityId"] = caseOppCommitment.OpportunityId == Guid.Empty ? Guid.NewGuid() : (object)caseOppCommitment.OpportunityId ?? DBNull.Value;
                row["planningCardId"] = caseOppCommitment.PlanningCardId == Guid.Empty ? Guid.NewGuid() : (object)caseOppCommitment.PlanningCardId ?? DBNull.Value;
                row["lastUpdatedBy"] = caseOppCommitment.LastUpdatedBy;

                caseOppCommitmentsDataTable.Rows.Add(row);
            }

            return caseOppCommitmentsDataTable;
        }

        private static IEnumerable<CommitmentViewModel> ConvertToCommitmentViewModel(IEnumerable<Commitment> commitmentData) => commitmentData.Select(item => new CommitmentViewModel
        {
            Id = item.Id,
            EmployeeCode = item.EmployeeCode,
            CommitmentTypeCode = item.CommitmentType.CommitmentTypeCode,
            CommitmentTypeName = item.CommitmentType.CommitmentTypeName,
            CommitmentTypeReasonCode = item.CommitmentTypeReasonCode,
            CommitmentTypeReasonName = item.CommitmentTypeReasonName,
            StartDate = item.StartDate,
            EndDate = item.EndDate,
            Allocation = item.Allocation,
            Description = item.Notes,
            IsSourceStaffing = item.IsSourceStaffing,
            IsOverridenInSource = item.IsOverridenInSource,
            Precdence = item.CommitmentType.Precedence,
            ReportingPrecdence = item.CommitmentType.ReportingPrecedence,
            IsStaffingTag = item.CommitmentType.IsStaffingTag
        });

        private IEnumerable<ResourceAllocation> GetResourceAllocationsWithoutPartners(IList<ResourceAllocation> resourceAllocations)
        {
            if (resourceAllocations == null || resourceAllocations.Count() == 0)
            {
                return Enumerable.Empty<ResourceAllocation>();
            }

            return GetResourceAllocationsWithoutPartnersLevelGrades(resourceAllocations);
        }

        private IEnumerable<ResourceAllocation> GetResourceAllocationsWithoutPartnersLevelGrades(IList<ResourceAllocation> resourceAllocations)
        {
            return resourceAllocations.Where(x => !x.CurrentLevelGrade.StartsWith("V", true, CultureInfo.InvariantCulture) && !x.CurrentLevelGrade.StartsWith("EV", true, CultureInfo.InvariantCulture));
        }

        private IEnumerable<ResourceAllocation> GetResourceAllocationsForBillableCases(IEnumerable<ResourceAllocation> resourceAllocations)
        {
            if (resourceAllocations == null || resourceAllocations.Count() == 0)
            {
                return Enumerable.Empty<ResourceAllocation>();
            }

            return resourceAllocations.Where(x => (!string.IsNullOrEmpty(x.OldCaseCode) && x.CaseTypeCode == 1) || string.IsNullOrEmpty(x.OldCaseCode) && (x.PipelineId != Guid.Empty));

        }


        public async Task<IEnumerable<CommitmentViewModel>> checkPegRingfenceAllocationAndInsertDownDayCommitments(IList<ResourceAllocation> resourceAllocations)
        {
            if (resourceAllocations == null || resourceAllocations.Count() == 0)
            {
                return Enumerable.Empty<CommitmentViewModel>();
            }

            var filteredResources = GetResourceAllocationsWithoutPartners(resourceAllocations);
            filteredResources = GetResourceAllocationsForBillableCases(filteredResources);

            var commitmentlist = new List<Commitment>();
            foreach (var allocation in filteredResources)
            {
                var pegCommitmentsForEmployees =
                  await GetResourceCommitmentsWithinDateRangeByEmployees(allocation.EmployeeCode, allocation.StartDate, allocation.EndDate, "P");
                if (pegCommitmentsForEmployees?.Count() > 0)
                {
                    var distinctEmployeesWithPegCommitments = pegCommitmentsForEmployees.Select(x => x.EmployeeCode).Distinct();
                    var commitment = new Commitment
                    {
                        EmployeeCode = allocation.EmployeeCode,
                        StartDate = Utilities.AddBusinessDays(Convert.ToDateTime(allocation.EndDate), 1),
                        EndDate = Utilities.AddBusinessDays(Convert.ToDateTime(allocation.EndDate), 1),
                        CommitmentType = new CommitmentType
                        {
                            CommitmentTypeCode = "DD",
                            IsStaffingTag = false
                        },
                        CommitmentTypeReasonCode = null,
                        LastUpdatedBy = "Auto-DownDay"
                    };
                    commitmentlist.Add(commitment);
                }
            }

            var savedCommitments = await UpsertResourcesCommitments(commitmentlist);
            var savedCommitmentsViewModel = ConvertToCommitmentViewModel(savedCommitments).ToList();
            return savedCommitmentsViewModel;
        }


        public async Task<IEnumerable<CommitmentAlert>> UpsertRingfenceCommitmentAlerts(CommitmentEnrichment commitmentDetails)
        {

          var matchingResources = commitmentDetails.resources;

          DataTable employeeShedulingOfficeTable = CreateEmployeeSchedulingOfficeTable(matchingResources);

          var lastUpdatedBy = commitmentDetails.commitments.FirstOrDefault()?.LastUpdatedBy;

          var notificationMappings =  await _commitmentRepository.GetEmployeesForRingfenceAlert(employeeShedulingOfficeTable, lastUpdatedBy);

          DataTable ringfenceAlertTable = CreateRingfenceAlertDataTable(commitmentDetails.commitments, notificationMappings, matchingResources);
       
          var upsertedData = await _commitmentRepository.UpsertRingfenceCommitmentAlerts(ringfenceAlertTable);

          return upsertedData;
        }
        private DataTable CreateEmployeeSchedulingOfficeTable(IList<ResourceModel> resources)
        {
            var table = new DataTable();
            table.Columns.Add("EmployeeCode", typeof(string));
            table.Columns.Add("SchedulingOfficeCode", typeof(string));

            foreach (var resource in resources)
            {
                var row = table.NewRow();

                row["EmployeeCode"] = (object)resource?.EmployeeCode ?? DBNull.Value;
                row["SchedulingOfficeCode"] = (object)resource?.SchedulingOffice?.OfficeCode ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }


        private DataTable CreateRingfenceAlertDataTable( IEnumerable<Commitment> commitments, IEnumerable<NotificationRecipientMapping> notificationMappings, IEnumerable<ResourceModel> matchingResources)
        {
            var table = new DataTable();
            table.Columns.Add("commitmentId", typeof(Guid));
            table.Columns.Add("employeeCode", typeof(string));
            table.Columns.Add("levelGrade", typeof(string));
            table.Columns.Add("operatingOfficeName", typeof(string));
            table.Columns.Add("operatingOfficeCode", typeof(short));
            table.Columns.Add("alertStatus", typeof(string));
            table.Columns.Add("lastUpdated", typeof(DateTime));
            table.Columns.Add("lastUpdatedBy", typeof(string));
            table.Columns.Add("createdBy", typeof(string));

            foreach (var mapping in notificationMappings)
            {
                var relatedCommitments = commitments
                    .Where(c => c.EmployeeCode == mapping.SourceEmployeeCode && c.Id.HasValue)
                    .ToList();

                foreach (var commitment in relatedCommitments)
                {
                    if (string.IsNullOrEmpty(mapping.RecipientEmployeeCodes)) continue;

                    var recipientCodes = mapping.RecipientEmployeeCodes.Split(',');
                    var resource = matchingResources.FirstOrDefault(r => r.EmployeeCode == commitment.EmployeeCode);

                    foreach (var recipientCode in recipientCodes)
                    {
     
                        var row = table.NewRow();
                        row["commitmentId"] = (object)commitment.Id ?? DBNull.Value;
                        row["employeeCode"] = recipientCode;
                        row["levelGrade"] = (object)resource.LevelGrade ?? DBNull.Value;
                        row["operatingOfficeName"] = (object)resource.SchedulingOffice?.OfficeName ?? DBNull.Value;
                        row["operatingOfficeCode"] = (object)resource.SchedulingOffice?.OfficeCode ?? DBNull.Value;
                        row["alertStatus"] = "U";
                        row["lastUpdated"] = DateTime.Now;
                        row["lastUpdatedBy"] = (object)commitment.LastUpdatedBy ?? DBNull.Value;
                        row["createdBy"] = (object)commitment.LastUpdatedBy ?? DBNull.Value;

                        table.Rows.Add(row);
                    }
                }
            }

            return table;
        }

        #endregion
    }
}