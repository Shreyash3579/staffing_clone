using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class StaffingApiClient : IStaffingApiClient
    {
        private readonly HttpClient _apiClient;

        public StaffingApiClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:StaffingApiBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }
        public async Task<IEnumerable<ScheduleMaster>> UpdatePrePostAllocations(IEnumerable<ScheduleMaster> allocationsToUpdate)
        {
            var payload = allocationsToUpdate;
            var responseMessage = await _apiClient.PostAsJsonAsync("api/resourceAllocation/upsertResourceAllocations", payload);
            var updatedPrePostAllocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMaster>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? Enumerable.Empty<ScheduleMaster>();
            return updatedPrePostAllocations;
        }

        public async Task<IEnumerable<CaseRoll>> GetAllUnprocessedCasesOnCaseRoll()
        {

            var responseMessage = await _apiClient.GetAsync("api/caseRoll/unprocessedCasesOnCaseRoll");

            var unporcessedCasesOnRoll = JsonConvert.DeserializeObject<IEnumerable<CaseRoll>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<CaseRoll>();
            return unporcessedCasesOnRoll;
        }

        public async Task<IEnumerable<ScheduleMaster>> GetResourceAllocationsOnCaseRollByCaseCodes(string listOldCaseCodes)
        {
            var payload = new { listOldCaseCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/resourceAllocation/allocationsOnCaseRollByCaseCodes", payload);

            var unporcessedCasesOnRoll = JsonConvert.DeserializeObject<IEnumerable<ScheduleMaster>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<ScheduleMaster>();
            return unporcessedCasesOnRoll;
        }

        public async Task DeleteRolledAllocationsMappingFromCaseRollTracking(string lastUpdatedBy, string rolledCaseCodes = null)
        {
            var responseMessage =
                await _apiClient.DeleteAsync($"api/caseRoll/deleteRolledAllocationsMappingFromCaseRollTracking?lastUpdatedBy={lastUpdatedBy}&rolledCaseCodes={rolledCaseCodes}");

            return;
        }


        public async Task<IEnumerable<ScheduleMaster>> GetAllocationsForEmployeesOnPrePost()
        {
            var responseMessage = await _apiClient.GetAsync("api/resourceAllocation/allocationsForEmployeesOnPrePost");

            var allocationForEmployeesOnPrePost = JsonConvert.DeserializeObject<IEnumerable<ScheduleMaster>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<ScheduleMaster>();
            return allocationForEmployeesOnPrePost;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsBySelectedValues(string oldCaseCodes,
           string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate)
        {
            var payload = new { oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/resourceAllocation/allocationsBySelectedValues", payload);
            var allocations = JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<ResourceAssignmentViewModel>();

            return allocations;

        }

        public async Task<IEnumerable<ScheduleMaster>> GetResourceAllocationsByPipelineIds(string pipelineIds)
        {
            var payload = new { pipelineIds };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/resourceAllocation/allocationsByPipelineIds", payload);
            var allocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMaster>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<ScheduleMaster>();

            return allocations;
        }

        public async Task DeleteResourceAllocationByIds(string allocationIds, string lastUpdatedBy)
        {
            var payload = new { allocationIds, lastUpdatedBy };
            await _apiClient.PostAsJsonAsync($"api/resourceAllocation/deleteAllocationsByIds", payload);
            
            return;
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
                    ?.ReadAsStringAsync().Result).ToList() ?? new List<ResourceAssignmentViewModel>();

            return allocatedResources;
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

        public async Task<IEnumerable<StaffableAs>> UpsertResourcesStaffableAs(IEnumerable<StaffableAs> staffableAsToUpsert)
        {
            var payload = staffableAsToUpsert;
            var responseMessage = await _apiClient.PostAsJsonAsync("api/staffableAs/upsertResourceStaffableAs", payload);
            var upsertedStaffableAs = JsonConvert.DeserializeObject<IEnumerable<StaffableAs>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? Enumerable.Empty<StaffableAs>();
            return upsertedStaffableAs;
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPipelineIds(string pipelineIds)
        {
            var payload = new { pipelineIds };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/scheduleMasterPlaceholder/allocationsByPipelineIds", payload);
            var allocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<ScheduleMasterPlaceholder>();

            return allocations;
        }
        public async Task<IEnumerable<SecurityUserViewModel>> GetBossSecurityUsers()
        {
            var responseMessage = await _apiClient.GetAsync("api/securityuser/getAllSecurityUsers");
            var securityUsers = JsonConvert.DeserializeObject<IEnumerable<SecurityUserViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<SecurityUserViewModel>();
            return securityUsers.ToList();
        }

        public async Task DeleteBossSecurityUsers(string employeeCodes)
        {
            var lastUpdatedBy = "Auto-Termination";
            var deletedUsers =
               await _apiClient.DeleteAsync($"api/securityuser/deleteSecurityUser?employeecode={employeeCodes}&lastUpdatedBy={lastUpdatedBy}");
            return;
        }

        public async Task<IEnumerable<SecurityGroup>> GetAllBOSSSecurityGroups()
        {
            var responseMessage = await _apiClient.GetAsync("api/securityuser/getAllSecurityGroups");
            var securityUsers = JsonConvert.DeserializeObject<IEnumerable<SecurityGroup>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<SecurityGroup>();
            return securityUsers.ToList();
        }

        public async Task<IEnumerable<Commitment>> checkPegRingfenceAllocationAndInsertDownDayCommitments(IEnumerable<ResourceAssignmentViewModel> resourceAllocations)
        {
            var payload = new { resourceAllocations };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/commitment/checkPegRingfenceAllocationAndInsertDownDayCommitments", payload);
            var commitments = JsonConvert.DeserializeObject<IEnumerable<Commitment>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<Commitment>();

            return commitments;
        }

        public async Task<IEnumerable<CasePlanningBoard>> GetOpportunityDataInCasePlanningBoard()
        {
            var responseMessage = await _apiClient.GetAsync("api/casePlanning/getOpportunityDataInCasePlanningBoard");
            var casePlanningBoardOpportunities = JsonConvert.DeserializeObject<IEnumerable<CasePlanningBoard>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<CasePlanningBoard>();

            return casePlanningBoardOpportunities;
        }

        public async Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectIds(string oldCaseCodes, string pipelineIds, string planningCardIds)
        {
            var payload = new { oldCaseCodes, pipelineIds, planningCardIds };

            var responseMessage = await _apiClient.PostAsJsonAsync("api/casePlanning/getCasePlanningBoardDataByProjectIds", payload);
            var casePlanningBoardProjects = JsonConvert.DeserializeObject<IEnumerable<CasePlanningBoard>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<CasePlanningBoard>();

            return casePlanningBoardProjects;
        }

        public async Task<IEnumerable<CasePlanningBoard>> UpsertCasePlanningBoardData(IEnumerable<CasePlanningBoard> casePlanningBoardData)
        {
            var responseMessage = await _apiClient.PostAsJsonAsync("api/casePlanning", casePlanningBoardData);

            var upsertedProjects = JsonConvert.DeserializeObject<IEnumerable<CasePlanningBoard>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<CasePlanningBoard>();

            return upsertedProjects;
        }

        public async Task<string> GetCaseRollsRecentlyProcessedInStaffing(DateTime lastPollDateTime)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/caseRoll/caseRollsRecentlyProcessedInStaffing?lastPollDateTime={lastPollDateTime}");
            var listCaseRollsRecentlyProcessedInStaffing =
                responseMessage.Content?.ReadAsStringAsync().Result ?? string.Empty;

            return listCaseRollsRecentlyProcessedInStaffing;
        }

        public async Task<IEnumerable<PreponedCasesAllocationsAudit>> UpsertPreponedCaseAllocationsAudit(IEnumerable<PreponedCasesAllocationsAudit> preponedCasesAllocationsAudit)
        {
            var payload = preponedCasesAllocationsAudit;
            var responseMessage = await _apiClient.PostAsJsonAsync("api/preponedCasesAllocationsAudit/upsertPreponedCaseAllocationsAudit", payload);
            var upsertedPrePostAllocations = JsonConvert.DeserializeObject<IEnumerable<PreponedCasesAllocationsAudit>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? Enumerable.Empty<PreponedCasesAllocationsAudit>();
            return upsertedPrePostAllocations;
        }

        public async Task<IEnumerable<CADMismatchLog>> GetStaffingTablesRecordsForSync()
        {
            var responseMessage = await _apiClient.GetAsync("api/dataSyncMismatch/getCountforSyncTablesInStaffing");
            var staffingSyncTables = JsonConvert.DeserializeObject<IEnumerable<CADMismatchLog>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<CADMismatchLog>();

            return staffingSyncTables;
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



        public async Task<IEnumerable<RevOffice>> GetRevOfficeList()
        {
            var responseMessage = await _apiClient.GetAsync("api/SecurityUser/getRevOfficeList");

            var officeList = JsonConvert.DeserializeObject<IEnumerable<RevOffice>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<RevOffice>();
            return officeList;
        }
        public async Task<IEnumerable<ServiceLine>> GetServiceLineList()
        {
            var responseMessage = await _apiClient.GetAsync("api/SecurityUser/getServiceLineList");

            var serviceLineList = JsonConvert.DeserializeObject<IEnumerable<ServiceLine>>(responseMessage.Content?.ReadAsStringAsync().Result)
                               ?? Enumerable.Empty<ServiceLine>();
            return serviceLineList;
        }
        public async Task SaveRevOfficeList(IEnumerable<RevOffice> officeList)
        {
            var payload = officeList;
            var responseMessage = await _apiClient.PostAsJsonAsync("api/SecurityUser/saveRevOfficeList", payload);

            var upsertedOfficeList = JsonConvert.DeserializeObject<IEnumerable<RevOffice>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                    ?? Enumerable.Empty<RevOffice>();
            return;
        }

        public async Task SaveServiceLineList(IEnumerable<ServiceLine> serviceLineList)
        {
            var payload = serviceLineList;
            var responseMessage = await _apiClient.PostAsJsonAsync("api/SecurityUser/saveServiceLineList", payload);

            var upsertedOfficeList = JsonConvert.DeserializeObject<IEnumerable<ServiceLine>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                    ?? Enumerable.Empty<ServiceLine>();
            return;
        }

        public async Task UpdateSecurityUserForWFPRole(IEnumerable<OfficeHierarchyDetails> officeList, IEnumerable<ServiceLine> serviceLineList)
        {
            // Prepare the payload as an anonymous object containing both office and service line lists
            var payload = new
            {
                newOffices = officeList,
                newServiceLines = serviceLineList
            };

            // Send the POST request with the updated payload
            var responseMessage = await _apiClient.PostAsJsonAsync("api/SecurityUser/updateSecurityUserForWFPRole", payload);

            return;
        }



    }
}
