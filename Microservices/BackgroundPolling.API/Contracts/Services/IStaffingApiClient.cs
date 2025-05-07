using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IStaffingApiClient
    {
        Task<IEnumerable<ScheduleMaster>> UpdatePrePostAllocations(IEnumerable<ScheduleMaster> allocationsToUpdate);
        Task<IEnumerable<CaseRoll>> GetAllUnprocessedCasesOnCaseRoll();
        Task<IEnumerable<ScheduleMaster>> GetResourceAllocationsOnCaseRollByCaseCodes(string listOldCaseCodes);
        Task DeleteRolledAllocationsMappingFromCaseRollTracking(string lastUpdatedBy, string rolledCaseCodes);
        Task<IEnumerable<ScheduleMaster>> GetAllocationsForEmployeesOnPrePost();
        Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsBySelectedValues(string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ScheduleMaster>> GetResourceAllocationsByPipelineIds(string pipelineIds);
        Task DeleteResourceAllocationByIds(string allocationIds, string lastUpdatedBy);
        Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<CaseOppChanges>> GetPipelineChangesByPipelineIds(string pipelineIds);
        Task<IEnumerable<StaffableAs>> UpsertResourcesStaffableAs(IEnumerable<StaffableAs> staffableAsToUpsert);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPipelineIds(string pipelineIds);
        Task<IEnumerable<SecurityUserViewModel>> GetBossSecurityUsers();
        Task DeleteBossSecurityUsers(string employeeCodes);
        Task<IEnumerable<SecurityGroup>> GetAllBOSSSecurityGroups();
        Task<IEnumerable<Commitment>> checkPegRingfenceAllocationAndInsertDownDayCommitments(IEnumerable<ResourceAssignmentViewModel> resourceAllocations);
        Task<IEnumerable<CasePlanningBoard>> GetOpportunityDataInCasePlanningBoard();
        Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectIds(string oldCaseCodes, string pipelineIds, string planningCardIds);
        Task<IEnumerable<CasePlanningBoard>> UpsertCasePlanningBoardData(IEnumerable<CasePlanningBoard> casePlanningBoardData);
        Task<string> GetCaseRollsRecentlyProcessedInStaffing(DateTime lastPollDateTime);
        Task<IEnumerable<PreponedCasesAllocationsAudit>> UpsertPreponedCaseAllocationsAudit(IEnumerable<PreponedCasesAllocationsAudit> preponedCasesAllocationsAudit);
        Task<IEnumerable<CADMismatchLog>> GetStaffingTablesRecordsForSync();
        Task<IEnumerable<CaseOppCortexTeamSize>> GetOppCortexPlaceholderInfoByPipelineIds(string pipelineIds);

        Task<IEnumerable<RevOffice>> GetRevOfficeList();

        Task<IEnumerable<ServiceLine>> GetServiceLineList();

        Task SaveRevOfficeList(IEnumerable<RevOffice> officeList);

        Task SaveServiceLineList(IEnumerable<ServiceLine> serviceLineList);

        Task UpdateSecurityUserForWFPRole(IEnumerable<OfficeHierarchyDetails> officeList, IEnumerable<ServiceLine> serviceLineList);

        }
}
