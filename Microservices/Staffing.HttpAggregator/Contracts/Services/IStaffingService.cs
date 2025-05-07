using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IStaffingService
    {
        Task<IEnumerable<ResourceAssignmentViewModel>> GetHistoricalStaffingAllocationsForEmployee(string employeeCode);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetStaffingAllocationsForEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<ResourceView>> GetResourcesStaffingAndCommitments(SupplyFilterCriteria supplyFilterCriteria, string loggedInUser);
        Task<IEnumerable<ResourceView>> GetResourcesIncludingTerminatedWithAllocationsBySearchString(string searchString, string loggedInUser = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsByCaseCode(string oldCaseCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsByPipelineId(string pipelineId, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetHistoricalStaffingAllocationsForProject(string oldCaseCode, string pipelineId);
        Task<List<SecurityUserDetailsViewModel>> GetAllSecurityUsersDetails();
        Task<IEnumerable<ResourceView>> GetFilteredResourcesGroupWithAllocations(SupplyGroupFilterCriteria supplyGroupFilterCriteria, string loggedInUser);

        Task<IEnumerable<NotesAlertViewModel>> GetNotesAlert(string employeeCode);

        Task<IEnumerable<NotesSharedWithGroupViewModel>> GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(string employeeCode);
        Task<ResourceViewNoteViewModel> UpsertResourceViewNote(ResourceViewNote resourceViewNote);

        Task<ResourceViewCDViewModel> UpsertResourceRecentCD(ResourceCD resourceViewCD);

        Task<ResourceViewCommercialModelViewModel> UpsertResourceCommercialModel(ResourceCommercialModel resourceViewCommercialModel);

        Task<StaffingResponsible> GetResourceStaffingResponsibeDataByEmployeeCode(string employeeCode);
        Task<PlanningCardViewModel> UpsertPlanningCard(PlanningCard planningCard, string employeeCode);

        Task<IEnumerable<Commitment>> UpsertResourcesCommitments(IList<Commitment> commitments);
    }
}