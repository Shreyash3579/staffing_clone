using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class StaffingApiClient : IStaffingApiClient
    {
        private readonly HttpClient _apiClient;

        public StaffingApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("StaffingApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsBySelectedValues(string oldCaseCodes, string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate)
        {
            var payload = new { oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/resourceAllocation/allocationsBySelectedValues", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.ToList();
        }

        public async Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsBySelectedSupplyValues(string officeCodes, DateTime startDate, DateTime endDate,
            string staffingTags, string currentLevelGrades)
        {
            var payload = new { officeCodes, startDate, endDate, staffingTags, currentLevelGrades };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/resourceAllocation/allocationsBySelectedSupplyValues", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.ToList();
        }

        public async Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return new List<ResourceAssignmentViewModel>();
            var payload = new { pipelineIds };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/resourceAllocation/allocationsByPipelineIds", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.OrderByDescending(x => x.CurrentLevelGrade).ThenBy(y => y.EmployeeName).ToList();
        }

        public async Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByPipelineId(string pipelineId)
        {
            if (string.IsNullOrEmpty(pipelineId))
                return new List<ResourceAssignmentViewModel>();

            var responseMessage =
                await _apiClient.GetAsync($"api/resourceAllocation/allocationsByPipelineId?pipelineId={pipelineId}");
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.OrderByDescending(x => x.CurrentLevelGrade).ThenBy(y => y.EmployeeName).ToList();
        }

        public async Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return new List<ResourceAssignmentViewModel>();

            var payload = new { oldCaseCodes };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/resourceAllocation/allocationsByCaseCodes", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.OrderByDescending(x => x.CurrentLevelGrade).ThenBy(y => y.EmployeeName).ToList();
        }

        public async Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByCaseCode(string oldCaseCode)
        {
            if (string.IsNullOrEmpty(oldCaseCode))
                return new List<ResourceAssignmentViewModel>();

            var responseMessage =
                await _apiClient.GetAsync($"api/resourceAllocation/allocationsByCaseCode?oldCaseCode={oldCaseCode}");
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.OrderByDescending(x => x.CurrentLevelGrade).ThenBy(y => y.EmployeeName).ToList();
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsByEmployeeCodes(string employeeCodes,
            DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return new List<ResourceAssignmentViewModel>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/resourceAllocation/allocationsByEmployees", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.ToList();
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByEmployeeCodes(string employeeCodes,
    DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return new List<ScheduleMasterPlaceholder>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/scheduleMasterPlaceholder/placeholderAllocationsByEmployees", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ScheduleMasterPlaceholder>();

            return allocatedResources.ToList();
        }



        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetPlaceholderAllocationsByEmployeeCodesV2(string employeeCodes,
    DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return new List<ResourceAssignmentViewModel>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/scheduleMasterPlaceholder/placeholderAllocationsByEmployees", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.ToList();
        }

        //commented on 06-jun-23 as it is not being used anymore
        //public async Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByOfficeCodes(string officeCodes,
        //    DateTime? startDate, DateTime? endDate)
        //{
        //    if (string.IsNullOrEmpty(officeCodes))
        //        return new List<ResourceAssignmentViewModel>();

        //    var responseMessage =
        //        await _apiClient.GetAsync($"api/resourceAllocation/allocationsByOffices?" +
        //        $"officeCodes={officeCodes}&startDate={startDate}&endDate={endDate}");
        //    var allocatedResources =
        //        JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
        //            ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

        //    return allocatedResources.ToList();
        //}

        public async Task<IList<ResourceAllocation>> UpsertResourceAllocations(IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var payload = JsonConvert.SerializeObject(resourceAllocations);
            var responseMessage = await _apiClient.PostAsJsonAsync("api/resourceAllocation/upsertResourceAllocations", payload);
            var allocatedResources = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(
                    responseMessage.Content?.ReadAsStringAsync().Result);

            return allocatedResources.ToList();
        }

        public async Task<IEnumerable<EmailUtilityData>> GetEmailUtilityDataLogsByDateAndEmailType(DateTime dateOfEmail, string emailType)
        {
            var responseMessage = await _apiClient.GetAsync($"api/emailUtilityDataLog/getEmailUtilityDataLogsByDateAndEmailType?dateOfEmail={dateOfEmail}&emailType={emailType}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"Error while getting Data logs: {responseMessage.StatusCode}.");
            }

            var listFailedEmployees = JsonConvert.DeserializeObject<IEnumerable<EmailUtilityData>>(
                    responseMessage.Content?.ReadAsStringAsync().Result);

            return listFailedEmployees ?? Enumerable.Empty<EmailUtilityData>();
        }

        public async Task<IList<EmailUtilityData>> UpsertEmailUtilityDataLog(IList<EmailUtilityData> employees)
        {
            //var payload = JsonConvert.SerializeObject(employees);
            var responseMessage = await _apiClient.PostAsJsonAsync("api/emailUtilityDataLog/upsertEmailUtilityDataLog", employees);
            var emailUtilityDataLog = JsonConvert.DeserializeObject<IEnumerable<EmailUtilityData>>(
                    responseMessage.Content?.ReadAsStringAsync().Result);

            return emailUtilityDataLog.ToList();
        }

        public async Task<IEnumerable<ResourceAllocation>> UpdateResourceAssignmentForTableau(IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var payload = JsonConvert.SerializeObject(resourceAllocations);
            var responseMessage = await _apiClient.PutAsJsonAsync("api/resourceAllocation/updateResourceAssignmentForTableau", payload);
            var updatedResourceAllocations = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(
                responseMessage.Content?.ReadAsStringAsync().Result);

            return updatedResourceAllocations;
        }


        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetHistoricalStaffingAllocationsForEmployee(
            string employeeCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/resourceHistory?employeeCode={employeeCode}");
            var result =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return result;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetStaffingAllocationsForEmployee(string employeeCode)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/resourceHistory?employeeCode={employeeCode}");
            var result =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return result;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/resourceAllocation/lastTeamByEmployeeCode?employeeCode={employeeCode}&date={date}");
            var result =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return result;
        }

        public async Task<IEnumerable<EmployeeLastBillableDateViewModel>> GetLastBillableDateByEmployeeCodes(string employeeCodes, DateTime? date = null)
        {
            var payload = new { employeeCodes, date };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/resourceAllocation/lastBillableDateByEmployeeCodes", payload);
            var lastBillablDates =
                JsonConvert.DeserializeObject<IEnumerable<EmployeeLastBillableDateViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<EmployeeLastBillableDateViewModel>();

            return lastBillablDates;
        }

        public async Task DeleteResourceAllocationByIds(string allocationIds, string lastUpdatedBy)
        {
            var payload = new { allocationIds, lastUpdatedBy };
            await _apiClient.PostAsJsonAsync($"api/resourceAllocation/deleteAllocationsByIds", payload);

            return;
        }


        public async Task DeletePlanningCardAndItsAllocations(Guid id, string lastUpdatedBy)
        {
            await _apiClient.DeleteAsync($"api/PlanningCard?id={id}&lastUpdatedBy={lastUpdatedBy}");
            return;

        }
        public async Task<IList<AuditCaseHistory>> GetAuditTrailForCaseOrOpportunity(string oldCaseCode,
            string pipelineId, int? limit, int? offset)
        {
            string auditLogUrl;
            if (limit != null && offset != null)
            {
                auditLogUrl = $"api/auditTrail/auditCase?oldCaseCode={oldCaseCode}&pipelineId={pipelineId}&limit={limit}&offset={offset}";
            }
            else
            {
                auditLogUrl = $"api/auditTrail/auditCase?oldCaseCode={oldCaseCode}&pipelineId={pipelineId}";
            }

            var responseMessage =
                await _apiClient.GetAsync(auditLogUrl);
            var auditCaseHistories =
                JsonConvert.DeserializeObject<IEnumerable<AuditCaseHistory>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result)
                ?? new List<AuditCaseHistory>();

            return auditCaseHistories.ToList();
        }

        public async Task<IEnumerable<AuditEmployeeHistory>> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset)
        {
            string auditLogUrl;

            if (limit != null && offset != null)
            {
                auditLogUrl = $"api/auditTrail/auditEmployee?employeeCode={employeeCode}&limit={limit}&offset={offset}";
            }
            else
            {
                auditLogUrl = $"api/auditTrail/auditEmployee?employeeCode={employeeCode}";
            }

            var responseMessage =
                await _apiClient.GetAsync(auditLogUrl);
            var auditEmployeeHistories =
                JsonConvert.DeserializeObject<IEnumerable<AuditEmployeeHistory>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result)
                ?? new List<AuditEmployeeHistory>();

            return auditEmployeeHistories.ToList();
        }

        public async Task<IEnumerable<CaseRoll>> GetCasesOnRollByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return new List<CaseRoll>();
            var payload = new { oldCaseCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/caseRoll/getCasesOnRollByCaseCodes", payload);
            var auditEmployeeHistories =
                JsonConvert.DeserializeObject<IEnumerable<CaseRoll>>(
                    responseMessage.Content?.ReadAsStringAsync().Result);

            return auditEmployeeHistories.ToList();
        }

        public async Task<string> DeleteCaseRollsByIds(string caseRollIdsToDelete, string lastUpdatedBy)
        {
            var responseMessage =
                await _apiClient.DeleteAsync($"api/caseRoll/deleteCaseRollsByIds?caseRollIds={caseRollIdsToDelete}&lastUpdatedBy={lastUpdatedBy}");

            var deletedCaseRollIds = responseMessage.Content?.ReadAsStringAsync().Result;

            return deletedCaseRollIds;
        }

        public async Task<string> DeleteRolledAllocationsByScheduleIds(string rolledScheduleIds, string lastUpdatedBy)
        {
            var responseMessage =
                await _apiClient.DeleteAsync($"api/caseRoll/deleteRolledAllocationsByScheduleIds?rolledScheduleIds={rolledScheduleIds}&lastUpdatedBy={lastUpdatedBy}");

            var deletedRolledAllocations = responseMessage.Content?.ReadAsStringAsync().Result;

            return deletedRolledAllocations;
        }

        public async Task<IEnumerable<CaseRoll>> UpsertCaseRolls(IEnumerable<CaseRoll> caseRoll)
        {
            var payload = JsonConvert.SerializeObject(caseRoll);
            var responseMessage = await _apiClient.PostAsJsonAsync("api/caseRoll/upsertCaseRolls", payload);
            var upsertedCaseRolls = JsonConvert.DeserializeObject<IEnumerable<CaseRoll>>(
                responseMessage.Content?.ReadAsStringAsync().Result);

            return upsertedCaseRolls;
        }

        public async Task<IEnumerable<CaseOppCommitmentViewModel>> GetProjectSTACommitmentDetails(string oldCaseCodes, string opportunityIds, string planningCardIds)
        {

            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(opportunityIds) && string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<CaseOppCommitmentViewModel>();

            var payload = new
            {
                oldCaseCode = oldCaseCodes,
                opportunityIds = opportunityIds,
                planningCardIds = planningCardIds
            };

            var responseMessage = await _apiClient.PostAsJsonAsync("api/commitment/getProjectSTACommitmentDetails", payload);

            var resultContent = await responseMessage.Content.ReadAsStringAsync();

            var commitmentDetails = JsonConvert.DeserializeObject<IEnumerable<CaseOppCommitmentViewModel>>(resultContent);

            return commitmentDetails;
        }

        public async Task<IEnumerable<SKUCaseTerms>> GetSKUTermsForCaseOrOpportunityForDuration(string oldCaseCodes,
            string pipelineIds, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds))
                return Enumerable.Empty<SKUCaseTerms>();

            var payload = new
            {
                oldCaseCodes,
                pipelineIds,
                startDate,
                endDate
            };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/skucaseterms/getskutermsforcasesoropportunitiesforduration",
                    payload);

            var skuCaseTermsCommitments =
                JsonConvert.DeserializeObject<IEnumerable<SKUCaseTerms>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);

            return skuCaseTermsCommitments;
        }

        public async Task<IEnumerable<SKUDemand>> GetSKUTermForProjects(string oldCaseCodes, string pipelineIds, string planningCardIds)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds) && string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<SKUDemand>();

            var payload = new
            {
                oldCaseCodes,
                pipelineIds,
                planningCardIds
            };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/sku/getSkuForProjects",
                    payload);

            var skuCaseTermsCommitments =
                JsonConvert.DeserializeObject<IEnumerable<SKUDemand>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);

            return skuCaseTermsCommitments;
        }


        public async Task<IEnumerable<SKUTerm>> GetSKUTermList()
        {
            var responseMessage = await _apiClient.GetAsync("api/skucaseterms/getskutermlist");

            var skuTermList =
                JsonConvert.DeserializeObject<IEnumerable<SKUTerm>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);

            return skuTermList;
        }

        //public async Task<CasePlanningBoardDataModel> GetCasePlanningBoardDataByDateRange(DateTime startDate, DateTime endDate, string employeeCode)
        //{
        //    return await GetCasePlanningBoardDataByDateRange(startDate, endDate, employeeCode);
        //}


        public async Task<IEnumerable<CasePlanningProjectPreferences>> GetCasePlanningProjectDetails(string oldCaseCodes, string pipelineIds, string planningCardIds)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds) && string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<CasePlanningProjectPreferences>();

            var payload = new
            {
                oldCaseCodes,
                pipelineIds,
                planningCardIds
            };
            var responseMessage = 
                await _apiClient.PostAsJsonAsync(
                    "api/casePlanning/getCasePlanningProjectDetails",
                    payload);

            var casePlanningIncludeInDemandProjects = 
                JsonConvert.DeserializeObject<IEnumerable<CasePlanningProjectPreferences>>(responseMessage.Content?.ReadAsStringAsync() .Result);

            return casePlanningIncludeInDemandProjects;
        }

        public async Task<CasePlanningBoardDataModel> GetCasePlanningBoardDataByDateRange(DateTime startDate, DateTime? endDate, string employeeCode)
        {
            var responseMessage =
                await _apiClient.GetAsync(
                    $"api/casePlanning/getCasePlanningBoardDataByDateRange?startDate={startDate}&endDate={endDate}&loggedInUser={employeeCode}");

            var casePlanningBoardData =
                JsonConvert.DeserializeObject<CasePlanningBoardDataModel>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return casePlanningBoardData;
        }

        public async Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectEndDateAndBucketIds(DateTime startDate, string bucketIds)
        {
            var responseMessage =
                await _apiClient.GetAsync(
                    $"api/casePlanning/getCasePlanningBoardDataByProjectEndDateAndBucketIds?startDate={startDate}&bucketIds={bucketIds}");

            var casePlanningBoardDataByProjectEndDateAndBucketIds =
                JsonConvert.DeserializeObject<IEnumerable<CasePlanningBoard>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return casePlanningBoardDataByProjectEndDateAndBucketIds;
        }

        public async Task<IEnumerable<CasePlanningBoardStaffableTeams>> GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(string officeCodes, DateTime startWeek, DateTime endWeek)
        {
            var payload = new { officeCodes, startWeek, endWeek };
            var responseMessage =
                await _apiClient.PostAsJsonAsync(
                    $"api/casePlanningStaffableTeams/getCasePlanningBoardStaffableTeamsByOfficesAndDateRange", payload);

            var casePlanningBoardDataByProjectEndDateAndBucketIds =
                JsonConvert.DeserializeObject<IEnumerable<CasePlanningBoardStaffableTeams>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return casePlanningBoardDataByProjectEndDateAndBucketIds;
        }

        public async Task<IEnumerable<CommitmentViewModel>> GetCommitmentsWithinDateRange(DateTime startDate,
            DateTime endDate)
        {
            var responseMessage =
                await _apiClient.GetAsync(
                    $"api/commitment/commitmentsWithinDateRange?startDate={startDate}&endDate={endDate}");

            var commitments =
                JsonConvert.DeserializeObject<IEnumerable<CommitmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return commitments;
        }

        public async Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsWithinDateRangeByEmployees(string employeeCodes,
            DateTime? startDate, DateTime? endDate, string commitmentTypeCode = null)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<CommitmentViewModel>();

            var payload = new { employeeCodes, startDate, endDate, commitmentTypeCode };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/commitment/commitmentsWithinDateRangeByEmployees", payload);

            var commitments =
                JsonConvert.DeserializeObject<IEnumerable<CommitmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CommitmentViewModel>();

            return commitments.ToList();
        }
        public async Task<IEnumerable<CommitmentViewModel>> GetCommitmentBySelectedValues(string commitmentTypeCodes, string employeeCodes, DateTime? startDate,
            DateTime? endDate, bool? ringfenceCommitmentsOnly)
        {
            if (string.IsNullOrEmpty(commitmentTypeCodes) && string.IsNullOrEmpty(employeeCodes) && !startDate.HasValue && !endDate.HasValue && !ringfenceCommitmentsOnly.HasValue)
                return Enumerable.Empty<CommitmentViewModel>();

            var payload = new { commitmentTypeCodes, employeeCodes, startDate, endDate, ringfenceCommitmentsOnly };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/commitment/commitmentBySelectedValues", payload);

            var commitments =
                JsonConvert.DeserializeObject<IEnumerable<CommitmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CommitmentViewModel>();

            return commitments.ToList();
        }

        public async Task<IEnumerable<Commitment>> GetResourceCommitments(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return Enumerable.Empty<Commitment>();

            var responseMessage =
               await _apiClient.GetAsync($"api/commitment/commitmentsByEmployee?employeeCode={employeeCode}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");

            var commitments =
                JsonConvert.DeserializeObject<IEnumerable<Commitment>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<Commitment>();

            return commitments.ToList();
        }

        public async Task<IEnumerable<InvestmentCategory>> GetInvestmentCategoryList()
        {
            var responseMessage = await _apiClient.GetAsync("api/lookup/investmentTypes");

            var investmentCategories =
                JsonConvert.DeserializeObject<IEnumerable<InvestmentCategory>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return investmentCategories;
        }

        public async Task<IEnumerable<CaseRoleType>> GetCaseRoleTypeList()
        {
            var responseMessage = await _apiClient.GetAsync("api/lookup/caseRoleTypes");

            var caseRoleTypes =
                JsonConvert.DeserializeObject<IEnumerable<CaseRoleType>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);

            return caseRoleTypes;
        }

        public async Task<IEnumerable<CommitmentType>> GetCommitmentTypeList()
        {
            var responseMessage = await _apiClient.GetAsync("api/commitment/lookup");

            var commitmentTypes =
                JsonConvert.DeserializeObject<IEnumerable<CommitmentType>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);

            return commitmentTypes;
        }

        public async Task<IEnumerable<string>> GetCasesByResourceServiceLines(string oldCaseCodes,
            string serviceLineNames)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) || string.IsNullOrEmpty(serviceLineNames))
                return Enumerable.Empty<string>();

            var payload = new
            {
                oldCaseCodes,
                serviceLineNames
            };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/taggedCase/casesByResourceServiceLines",
                    payload);

            var oldCaseCodesFilteredByResourceServiceLines = responseMessage.Content?.ReadAsStringAsync().Result;

            return oldCaseCodesFilteredByResourceServiceLines.Split(",");
        }

        public async Task<IEnumerable<string>> GetOpportunitiesByResourceServiceLines(string pipelineIds,
            string serviceLineNames)
        {
            if (string.IsNullOrEmpty(pipelineIds) || string.IsNullOrEmpty(serviceLineNames))
                return Enumerable.Empty<string>();

            var payload = new
            {
                pipelineIds,
                serviceLineNames
            };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/taggedCase/opportunitiesByResourceServiceLines",
                    payload);

            var pipelineIdsFilteredByResourceServiceLines = responseMessage.Content?.ReadAsStringAsync().Result;

            return pipelineIdsFilteredByResourceServiceLines.Split(",");
        }

        public async Task<CaseOppChanges> GetPipelineChangesByPipelineId(Guid pipelineId)
        {
            var responseMessage = await _apiClient.GetAsync($"api/caseOppChanges/pipelineChangesByPipelineId?pipelineId={pipelineId}");

            var pipelineChanges =
                JsonConvert.DeserializeObject<CaseOppChanges>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);

            return pipelineChanges;
        }

        public async Task<IEnumerable<CaseOppChanges>> GetPipelineChangesByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return Enumerable.Empty<CaseOppChanges>();

            var payload = new { pipelineIds };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/caseOppChanges/pipelineChangesByPipelineIds", payload);
            var pipelineChanges =
                JsonConvert.DeserializeObject<IEnumerable<CaseOppChanges>>(
                    responseMessage.Content?.ReadAsStringAsync().Result);

            return pipelineChanges;
        }

        public async Task<IList<CaseOppChanges>> GetPipelineChangesByDateRange(DateTime startDate, DateTime? endDate)
        {
            var responseMessage = await _apiClient.GetAsync($"api/caseOppChanges/pipelineChangesByDateRange?startDate={startDate}&endDate={endDate}");

            var pipelineChanges =
                JsonConvert.DeserializeObject<IEnumerable<CaseOppChanges>>(responseMessage.Content?.ReadAsStringAsync().Result);

            return pipelineChanges.ToList();
        }

        public async Task<IEnumerable<CaseOppChanges>> GetCaseChangesByOldCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return Enumerable.Empty<CaseOppChanges>();

            var payload = new { oldCaseCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/caseOppChanges/caseChangesByOldCaseCodes", payload);
            var caseChanges =
                JsonConvert.DeserializeObject<IEnumerable<CaseOppChanges>>(
                    responseMessage.Content?.ReadAsStringAsync().Result);

            return caseChanges;
        }

        public async Task<CaseOppChanges> UpsertCaseChanges(CaseOppChanges upsertedData)
        {
            if (upsertedData == null)
                return null;

            var responseMessage = await _apiClient.PutAsJsonAsync("api/caseOppChanges/upsertCaseChanges", upsertedData);
            var caseChanges =
                JsonConvert.DeserializeObject<CaseOppChanges>(
                    responseMessage.Content?.ReadAsStringAsync().Result);
            return caseChanges;
        }

    public async Task<IEnumerable<CaseOppCortexTeamSize>> GetCaseTeamSizeByOldCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return Enumerable.Empty<CaseOppCortexTeamSize>();

            var responseMessage = await _apiClient.GetAsync($"api/caseOppChanges/caseTeamSizeByOldCaseCodes?oldCaseCode={oldCaseCodes}");
            var casesDetails = JsonConvert.DeserializeObject<IEnumerable<CaseOppCortexTeamSize>>(responseMessage.Content?.ReadAsStringAsync().Result);
            return casesDetails;
        }

        public async Task<IEnumerable<CaseOppCortexTeamSize>> GetOppCortexPlaceholderInfoByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return Enumerable.Empty<CaseOppCortexTeamSize>();

            var payload = new { pipelineIds };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/cortexSku/getOppCortexPlaceholderInfoByPipelineIds", payload);
            var oppsDetails =
                JsonConvert.DeserializeObject<IEnumerable<CaseOppCortexTeamSize>>(
                    responseMessage.Content?.ReadAsStringAsync().Result);

            return oppsDetails;
        }

        public async Task<IEnumerable<CaseViewNote>> GetLatestCaseViewNotes(string oldCaseCodes, string pipelineIds, string planningCardIds, string loggedInUser)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds) && string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<CaseViewNote>();

            var payload = new { oldCaseCodes, pipelineIds, planningCardIds, loggedInUser };

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/Note/getLatestCaseViewNotes", payload);
            var casesNotes =
                JsonConvert.DeserializeObject<IEnumerable<CaseViewNote>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<CaseViewNote>();
            return casesNotes;
        }

        public async Task<IEnumerable<CaseViewNote>> GetCaseViewNotesByPlanningCardIds(string planningCardIds, string loggedInUser)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<CaseViewNote>();

            var payload = new { planningCardIds, loggedInUser };

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/Note/getCaseViewNotes", payload);
            var casesNotes =
                JsonConvert.DeserializeObject<IEnumerable<CaseViewNote>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<CaseViewNote>();
            return casesNotes;
        }

        public async Task<IEnumerable<CaseViewNote>> GetCaseViewNotesByOldCaseCodes(string oldCaseCodes, string loggedInUser)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return Enumerable.Empty<CaseViewNote>();

            var payload = new { oldCaseCodes, loggedInUser };

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/Note/getCaseViewNotes", payload);
            var casesNotes =
                JsonConvert.DeserializeObject<IEnumerable<CaseViewNote>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<CaseViewNote>();
            return casesNotes;
        }

        public async Task<IEnumerable<CaseViewNote>> GetCaseViewNotesByPipelineIds(string pipelineIds, string loggedInUser)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return Enumerable.Empty<CaseViewNote>();

            var payload = new { pipelineIds, loggedInUser };

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/Note/getCaseViewNotes", payload);
            var casesNotes =
                JsonConvert.DeserializeObject<IEnumerable<CaseViewNote>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<CaseViewNote>();
            return casesNotes;
        }

        public async Task<IEnumerable<CaseViewNote>> GetCaseViewNotes(string oldCaseCodes, string pipelineIds, string planningCardIds, string loggedInUser)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds) && string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<CaseViewNote>();

            IEnumerable<CaseViewNote> casesNotes = new List<CaseViewNote>();

            if (!string.IsNullOrEmpty(oldCaseCodes))
                casesNotes = await GetCaseViewNotesByOldCaseCodes(oldCaseCodes, loggedInUser);
            else if (!string.IsNullOrEmpty(pipelineIds))
                casesNotes = await GetCaseViewNotesByPipelineIds(pipelineIds, loggedInUser);
            else if (!string.IsNullOrEmpty(planningCardIds))
                casesNotes = await GetCaseViewNotesByPlanningCardIds(planningCardIds, loggedInUser);

            return casesNotes;
        }

        public async Task<IList<CaseOppChanges>> GetCaseChangesByDateRange(DateTime startDate, DateTime? endDate)
        {
            var responseMessage = await _apiClient.GetAsync($"api/caseOppChanges/caseChangesByDateRange?startDate={startDate}&endDate={endDate}");

            var caseChanges =
                JsonConvert.DeserializeObject<IEnumerable<CaseOppChanges>>(responseMessage.Content?.ReadAsStringAsync().Result);

            return caseChanges.ToList();
        }

        public async Task<IList<CaseOppChanges>> GetCaseOppChangesByOfficesAndDateRange(string officeCodes, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(officeCodes))
                throw new ArgumentException("Office Codes cannot be null or empty for getting case/opp changes by officeCodes and date range");

            var responseMessage = await _apiClient.GetAsync($"api/caseOppChanges/caseOppChangesByOfficesAndDateRange?officeCodes={officeCodes}&startDate={startDate}&endDate={endDate}");

            var caseOppChanges =
                JsonConvert.DeserializeObject<IEnumerable<CaseOppChanges>>(responseMessage.Content?.ReadAsStringAsync().Result);

            return caseOppChanges.ToList();
        }

        public async Task<IEnumerable<CurrencyRate>> GetCurrencyRatesByCurrencyCodesAndDate(string currencyCodes, string currencyRateTypeCode, DateTime effectiveFromDate, DateTime effectiveTillDate)
        {
            if (string.IsNullOrEmpty(currencyCodes) || string.IsNullOrEmpty(currencyRateTypeCode) || effectiveFromDate == DateTime.MinValue || effectiveTillDate == DateTime.MinValue)
            {
                return Enumerable.Empty<CurrencyRate>();
            }

            var responseMessage = await _apiClient.GetAsync($"api/currency/currencyRatesByCurrencyCodesAndDate?" +
                                            $"currencyCodes={currencyCodes}&currencyRateTypeCode={currencyRateTypeCode}&effectiveFromDate={effectiveFromDate}&effectiveTillDate={effectiveTillDate}");

            var currencyRates =
                JsonConvert.DeserializeObject<IEnumerable<CurrencyRate>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);

            return currencyRates;
        }

        public async Task<IList<ScheduleMasterPlaceholder>> UpsertPlaceholderAllocations(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations)
        {
            var payload = JsonConvert.SerializeObject(placeholderAllocations);
            var responseMessage = await _apiClient.PostAsJsonAsync("api/scheduleMasterPlaceholder/upsertPlaceholderResourceAllocation", payload);
            var allocatedResources = JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>
                (responseMessage.Content?.ReadAsStringAsync().Result);

            return allocatedResources.ToList();
        }
        public async Task<IList<ResourceAssignmentViewModel>> GetPlaceholderAllocationsByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return new List<ResourceAssignmentViewModel>();
            var payload = new { pipelineIds };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/scheduleMasterPlaceholder/allocationsByPipelineIds", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.OrderByDescending(x => x.CurrentLevelGrade).ThenBy(y => y.EmployeeName).ToList();
        }

        public async Task<IList<ResourceAssignmentViewModel>> GetPlaceholderAllocationsByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return new List<ResourceAssignmentViewModel>();

            var payload = new { oldCaseCodes };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/scheduleMasterPlaceholder/allocationsByCaseCodes", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.OrderByDescending(x => x.CurrentLevelGrade).ThenBy(y => y.EmployeeName).ToList();
        }

        public async Task<IList<ResourceAssignmentViewModel>> GetPlaceholderAllocationsByPlanningCardIds(string planningCardIds)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return new List<ResourceAssignmentViewModel>();

            var payload = new { planningCardIds };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/scheduleMasterPlaceholder/placeholderAllocationsByPlanningCardIds", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.OrderByDescending(x => x.CurrentLevelGrade).ThenBy(y => y.EmployeeName).ToList();
        }

        public async Task<IList<ResourceAssignmentViewModel>> GetAllocationsByPlanningCardIds(string planningCardIds)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return new List<ResourceAssignmentViewModel>();

            var payload = new { planningCardIds };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/scheduleMasterPlaceholder/allocationsByPlanningCardIds", payload);
            var allocatedResources =
                JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources.OrderByDescending(x => x.CurrentLevelGrade).ThenBy(y => y.EmployeeName).ToList();
        }

        public async Task<IEnumerable<ResourceNote>> GetResourceNotes(string employeeCode, string loggedInEmployeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return new List<ResourceNote>();

            var responseMessage =
                await _apiClient.GetAsync($"api/note/resourceNotes?employeeCode={employeeCode}&loggedInEmployeeCode={loggedInEmployeeCode}");

            var resourceNotes =
                JsonConvert.DeserializeObject<IEnumerable<ResourceNote>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceNote>();

            return resourceNotes;
        }

        public async Task<IEnumerable<ResourceViewNote>> GetResourceViewNotes(string employeeCodes, string loggedInUser, string noteTypeCode)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return new List<ResourceViewNote>();

            var payload = new { employeeCodes, loggedInUser, noteTypeCode };

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/note/getResourceViewNotes", payload);

            var resourceViewNotes =
                JsonConvert.DeserializeObject<IEnumerable<ResourceViewNote>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceViewNote>();

            return resourceViewNotes;

        }

        public async Task<IEnumerable<ResourceCD>> GetResourceRecentCD(string employeeCodes)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return new List<ResourceCD>();

            var payload = new { employeeCodes };

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/note/getResourceRecentCD", payload);

            var resourceViewCDs =
                JsonConvert.DeserializeObject<IEnumerable<ResourceCD>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceCD>();

            return resourceViewCDs;

        }

        public async Task<IEnumerable<ResourceCommercialModel>> GetResourceCommercialModel(string employeeCodes)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return new List<ResourceCommercialModel>();

            var payload = new { employeeCodes };

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/note/getResourceCommercialModel", payload);

            var resourceViewCommercialModel =
                JsonConvert.DeserializeObject<IEnumerable<ResourceCommercialModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceCommercialModel>();

            return resourceViewCommercialModel;

        }

        public async Task<ResourceViewNote> UpsertResourceViewNote(ResourceViewNote resourceViewNote)
        {
            if (string.IsNullOrEmpty(resourceViewNote.EmployeeCode) || string.IsNullOrEmpty(resourceViewNote.CreatedBy))
                return new ResourceViewNote();

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/note/upsertResourceViewNote", resourceViewNote);

            var upsertedNotes =
                JsonConvert.DeserializeObject<ResourceViewNote>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new ResourceViewNote();

            return upsertedNotes;

        }

        public async Task<ResourceCD> UpsertResourceCD(ResourceCD resourceViewCD)
        {
            if (string.IsNullOrEmpty(resourceViewCD.EmployeeCode) || string.IsNullOrEmpty(resourceViewCD.LastUpdatedBy))
            {
                return new ResourceCD();
            }

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/note/upsertResourceRecentCD", resourceViewCD);

            var upsertedCDS =
                JsonConvert.DeserializeObject<ResourceCD>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new ResourceCD();

            return upsertedCDS;

        }
        public async Task<ResourceCommercialModel> UpsertResourceCommercialModel(ResourceCommercialModel resourceViewCommercialModel)
        {
            if (string.IsNullOrEmpty(resourceViewCommercialModel.EmployeeCode) || string.IsNullOrEmpty(resourceViewCommercialModel.LastUpdatedBy))
            {
                return new ResourceCommercialModel();
            }

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/note/upsertResourceCommercialModel", resourceViewCommercialModel);

            var upsertedCDS =
                JsonConvert.DeserializeObject<ResourceCommercialModel>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new ResourceCommercialModel();

            return upsertedCDS;

        }

        public async Task<CaseViewNote> UpsertCaseViewNote(CaseViewNote caseViewNote)
        {
            if (string.IsNullOrEmpty(caseViewNote.CreatedBy))
                return new CaseViewNote();

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/note/upsertCaseViewNote", caseViewNote);

            var upsertedNotes =
                JsonConvert.DeserializeObject<CaseViewNote>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new CaseViewNote();

            return upsertedNotes;

        }

        public async Task<IEnumerable<Commitment>> UpsertResourceCommitment(IEnumerable<Commitment> commitments)
        {
            if (commitments == null || !commitments.Any())
                return Enumerable.Empty<Commitment>();

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/commitment/resourcesCommitments", commitments);


            var upsertedData = JsonConvert.DeserializeObject<IEnumerable<Commitment>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<Commitment>();

            return upsertedData;
        }

        public async Task<IEnumerable<CommitmentAlert>> UpsertRingfenceCommitmentAlerts(CommitmentEnrichment commitmentDetails)
        {
            if (commitmentDetails == null)
                return Enumerable.Empty<CommitmentAlert>();

            var response = await _apiClient.PostAsJsonAsync("api/commitment/upsertRingfenceCommitmentAlerts", commitmentDetails);

            var result = JsonConvert.DeserializeObject<IEnumerable<CommitmentAlert>>(response.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CommitmentAlert>();

            return result;
        }




        public async Task<IEnumerable<PlanningCard>> GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(string employeeCode, string officeCodes, string staffingTags, string bucketIds = null)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return new List<PlanningCard>();

            var responseMessage =
                await _apiClient.GetAsync($"api/planningCard/?employeeCode={employeeCode}&officeCodes={officeCodes}&staffingTags={staffingTags}&bucketIds={bucketIds}");

            var planningCards =
                JsonConvert.DeserializeObject<IEnumerable<PlanningCard>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<PlanningCard>();

            return planningCards;
        }

        public async Task<IEnumerable<PlanningCard>> GetPlanningCardByPlanningCardIds(string planningCardIds)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return new List<PlanningCard>();

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/planningCard/planningCardsByPlanningCardIds", planningCardIds);

            var planningCards =
                JsonConvert.DeserializeObject<IEnumerable<PlanningCard>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<PlanningCard>();

            return planningCards;
        }

        public async Task<PlanningCard> UpdatePlanningCard(PlanningCard planningCardToUpdate)
        {
            var responseMessage =
                await _apiClient.PutAsJsonAsync("api/planningCard", planningCardToUpdate);
            var updatedPlanningCard =
                JsonConvert.DeserializeObject<PlanningCard>(responseMessage.Content?.ReadAsStringAsync().Result);
            return updatedPlanningCard;
        }

        public async Task<IEnumerable<SecurityUser>> GetAllSecurityUsers()
        {
            var responseMessage =
                    await _apiClient.GetAsync("api/SecurityUser/getAllSecurityUsers");
            var securityUsers =
                JsonConvert.DeserializeObject<IEnumerable<SecurityUser>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<SecurityUser>();
            return securityUsers;
        }
        public async Task<IEnumerable<NoteAlert>> GetNotesAlert(string employeeCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/Note/notesAlert?employeeCode={employeeCode}");
            var notesAlert =
               JsonConvert.DeserializeObject<IEnumerable<NoteAlert>>(responseMessage.Content
                   ?.ReadAsStringAsync().Result) ?? new List<NoteAlert>();
            return notesAlert;
        }

        public async Task<IEnumerable<NotesSharedWithGroup>> GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(string employeeCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/Note/mostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode?employeeCode={employeeCode}");
            var recentSharedWithEmployeeGroups =
               JsonConvert.DeserializeObject<IEnumerable<NotesSharedWithGroup>>(responseMessage.Content
                   ?.ReadAsStringAsync().Result) ?? new List<NotesSharedWithGroup>();
            return recentSharedWithEmployeeGroups;
        }
        public async Task<IEnumerable<EmployeeStaffingPreferences>> GetEmployeeStaffingPreferences(string employeeCode)
        {

            var payload = new { employeeCode };

            var responseMessage =
                    await _apiClient.PostAsJsonAsync($"api/EmployeeStaffingPreference", payload);
            var employeeStaffingPreferences =
                JsonConvert.DeserializeObject<IEnumerable<EmployeeStaffingPreferences>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<EmployeeStaffingPreferences>();
            return employeeStaffingPreferences;
        }
        public async Task<IEnumerable<EmployeeStaffingPreferences>> UpsertEmployeeStaffingPreferences(IEnumerable<EmployeeStaffingPreferences> employeeStaffingPreferences)
        {
            var responseMessage = await _apiClient.PutAsJsonAsync("api/EmployeeStaffingPreference", employeeStaffingPreferences);
            var upsertedEmployeeStaffingPreferences = JsonConvert.DeserializeObject<IEnumerable<EmployeeStaffingPreferences>>(
                responseMessage.Content?.ReadAsStringAsync().Result);

            return upsertedEmployeeStaffingPreferences;
        }

        public async Task DeleteEmployeeStaffingPreferenceByType(string employeeCode, string preferenceTypeCode)
        {

           var response = await _apiClient.DeleteAsync($"api/EmployeeStaffingPreference?employeeCode={employeeCode}&preferenceTypeCode={preferenceTypeCode}");
     
        }

            public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlacholderAndPlanningCardAllocationsWithinDateRange(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return new List<ScheduleMasterPlaceholder>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/scheduleMasterPlaceholder/placeholderPlanningCardAllocationsWithinDateRange", payload);

            var placeholderPlanningCardAllocations =
                JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ScheduleMasterPlaceholder>();

            return placeholderPlanningCardAllocations;
        }

        //public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlanningCardAllocationsByEmployeeCodesAndDuration(string employeeCodes, DateTime? startDate, DateTime? endDate)
        //{
        //    if (string.IsNullOrEmpty(employeeCodes) || startDate == DateTime.MinValue || endDate == DateTime.MinValue)
        //        return new List<ScheduleMasterPlaceholder>();

        //    var payload = new { employeeCodes, startDate, endDate };

        //    var responseMessage =
        //        await _apiClient.PostAsJsonAsync($"api/planningCard/planningCardAllocations", payload);

        //    var placeholderPlanningCardAllocations =
        //        JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseMessage.Content
        //            ?.ReadAsStringAsync().Result) ?? new List<ScheduleMasterPlaceholder>();

        //    return placeholderPlanningCardAllocations;
        //}

        public async Task<IEnumerable<StaffableAs>> GetResourceActiveStaffableAsByEmployeeCodes(string employeeCodes)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return new List<StaffableAs>();

            var payload = new { employeeCodes };

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/staffableAs/activeStaffableAsByEmployeeCodes", payload);

            var employeesWithStaffableAsRoles =
                JsonConvert.DeserializeObject<IEnumerable<StaffableAs>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<StaffableAs>();

            return employeesWithStaffableAsRoles;

        }

        public async Task<IEnumerable<StaffingResponsible>> GetResourceStaffingResponsibleData(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return new List<StaffingResponsible>();

            var apiUrl = $"api/employeeStaffingInfo/getResourceStaffingResponsibleByEmployeeCodes?employeeCodes={employeeCode}";
            var responseMessage = await _apiClient.GetAsync(apiUrl);

            var staffingResponsibleData =
                JsonConvert.DeserializeObject<IEnumerable<StaffingResponsible>>(await responseMessage.Content.ReadAsStringAsync()) ?? new List<StaffingResponsible>();

            return staffingResponsibleData;
        }

        public async Task<IEnumerable<RingfenceManagement>> GetRingfencesDetailsByOfficesAndCommitmentCodes(string officeCodes, string commitmentTypeCodes)
        {
            if (string.IsNullOrEmpty(officeCodes) && string.IsNullOrEmpty(commitmentTypeCodes))
                return Enumerable.Empty<RingfenceManagement>();

            var payload = new { officeCodes, commitmentTypeCodes };

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/ringfenceManagement/ringfencesDetailsByOfficesAndCommitmentCodes", payload);

            var ringfenceDetails =
                JsonConvert.DeserializeObject<IEnumerable<RingfenceManagement>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<RingfenceManagement>();

            return ringfenceDetails;

        }

        public async Task<IEnumerable<RingfenceManagement>> GetRingfenceAuditLogByOfficeAndCommitmentCode(string officeCode, string commitmentTypeCode)
        {
            if (string.IsNullOrEmpty(officeCode) && string.IsNullOrEmpty(commitmentTypeCode))
                return Enumerable.Empty<RingfenceManagement>();

            var responseMessage =
                await _apiClient.GetAsync($"api/ringfenceManagement/ringfenceAuditLogByOfficeAndCommitmentCode?officeCode={officeCode}&commitmentTypeCode={commitmentTypeCode}");

            var ringfenceDetails =
                JsonConvert.DeserializeObject<IEnumerable<RingfenceManagement>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<RingfenceManagement>();

            return ringfenceDetails;

        }

        //commented on 06-jun-23 as it is not being used anymore
        //public async Task<OfficeClosureCases> GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate)
        //{
        //    if (string.IsNullOrEmpty(officeCodes) || string.IsNullOrEmpty(caseTypeCodes))
        //        return new OfficeClosureCases();

        //    var payload = new { officeCodes, caseTypeCodes, startDate, endDate };

        //    var responseMessage =
        //         await _apiClient.PostAsJsonAsync($"api/officeClosureCases/getOfficeClosureChangesWithinDateRangeByOfficeAndCaseType", payload);

        //    var officeClosureChanges =
        //        JsonConvert.DeserializeObject<OfficeClosureCases>(responseMessage.Content
        //            ?.ReadAsStringAsync().Result) ?? new OfficeClosureCases();

        //    return officeClosureChanges;
        //}

        public async Task<IList<UserPreferenceSupplyGroup>> GetUserPreferenceSupplyGroups(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return new List<UserPreferenceSupplyGroup>();

            var responseMessage =
                await _apiClient.GetAsync($"api/userPreferenceSupplyGroup/getUserPreferenceSupplyGroups?employeeCode={employeeCode}");

            var userPreferenceSupplyGroups =
                JsonConvert.DeserializeObject<IList<UserPreferenceSupplyGroup>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<UserPreferenceSupplyGroup>();

            return userPreferenceSupplyGroups;
        }

        public async Task<IList<UserPreferenceSavedGroup>> GetUserPreferenceSavedGroups(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return new List<UserPreferenceSavedGroup>();

            var responseMessage =
                await _apiClient.GetAsync($"api/userCustomFilter?employeeCode={employeeCode}");

            var userPreferenceSupplyGroups =
                JsonConvert.DeserializeObject<IList<UserPreferenceSavedGroup>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<UserPreferenceSavedGroup>();

            return userPreferenceSupplyGroups;
        }

        public async Task<IList<UserPreferenceGroupSharedInfo>> GetUserPreferenceGroupSharedInfo(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return new List<UserPreferenceGroupSharedInfo>();

            var responseMessage =
                await _apiClient.GetAsync($"api/userPreferenceGroupSharedInfo/getUserPreferenceGroupSharedInfo?groupId={groupId}");

            var userPreferenceGroups =
                JsonConvert.DeserializeObject<IList<UserPreferenceGroupSharedInfo>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<UserPreferenceGroupSharedInfo>();

            return userPreferenceGroups;
        }

        public async Task<IEnumerable<UserPreferenceSupplyGroup>> UpsertUserPreferenceSupplyGroups(IEnumerable<UserPreferenceSupplyGroup> supplyGroupsToUpsert)
        {
            if (supplyGroupsToUpsert == null)
                return new List<UserPreferenceSupplyGroup>();

            var payload = JsonConvert.SerializeObject(supplyGroupsToUpsert);

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/userPreferenceSupplyGroup/upsertUserPreferenceSupplyGroups", payload);

            var userPreferenceGroups =
                JsonConvert.DeserializeObject<IEnumerable<UserPreferenceSupplyGroup>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<UserPreferenceSupplyGroup>();

            return userPreferenceGroups;
        }

        public async Task<IEnumerable<UserPreferenceSavedGroup>> UpsertUserPreferenceSavedGroups(IEnumerable<UserPreferenceSavedGroup> savedGroupsToUpsert)
        {
            if (savedGroupsToUpsert == null)
                return new List<UserPreferenceSavedGroup>();

            var payload = JsonConvert.SerializeObject(savedGroupsToUpsert);

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/userCustomFilter", payload);

            var userPreferenceGroups =
                JsonConvert.DeserializeObject<IEnumerable<UserPreferenceSavedGroup>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<UserPreferenceSavedGroup>();

            return userPreferenceGroups;
        }

        public async Task<IList<UserPreferenceGroupSharedInfo>> UpsertUserPreferenceGroupSharedInfo(IEnumerable<UserPreferenceGroupSharedInfo> sharedWithInfo)
        {
            if (sharedWithInfo == null)
                return new List<UserPreferenceGroupSharedInfo>();

            var payload = JsonConvert.SerializeObject(sharedWithInfo);

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/userPreferenceGroupSharedInfo/upsertUserPreferenceGroupSharedInfo", payload);

            var userPreferenceGroups =
                JsonConvert.DeserializeObject<IList<UserPreferenceGroupSharedInfo>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<UserPreferenceGroupSharedInfo>();

            return userPreferenceGroups;
        }


        public async Task<IEnumerable<PreponedCasesAllocationsAudit>> GetPreponedCaseAllocationsAudit(string serviceLineCodes, string officeCodes,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(serviceLineCodes) || string.IsNullOrEmpty(officeCodes))
                return new List<PreponedCasesAllocationsAudit>();

            var payload = new { serviceLineCodes, officeCodes, startDate, endDate };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/preponedCasesAllocationsAudit", payload);

            var auditData =
                JsonConvert.DeserializeObject<IEnumerable<PreponedCasesAllocationsAudit>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<PreponedCasesAllocationsAudit>();

            return auditData;
        }
        public async Task<PlanningCard> UpsertPlanningCardData(PlanningCard planningCard)
        {
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/planningCard/upsertPlanningCard", planningCard);

            var upsertedPlanningCard =
                JsonConvert.DeserializeObject<PlanningCard>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);

            return upsertedPlanningCard;
        }

    }
}