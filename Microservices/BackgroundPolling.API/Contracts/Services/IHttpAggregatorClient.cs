using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IHttpAggregatorClient
    {
        Task<string> UpsertCaseRollsAndAllocations(IEnumerable<CaseRoll> caseRolls, IEnumerable<ScheduleMaster> resourceAllocations);
        Task<IEnumerable<ResourceAssignmentViewModel>> UpsertResourceAllocations(IEnumerable<ScheduleMaster> allocationsToUpdate);
        Task<IEnumerable<ResourceAssignmentViewModel>> UpsertResourceAllocations(IEnumerable<ResourceAssignmentViewModel> allocations);
        Task<IEnumerable<ResourceAssignmentViewModel>> UpsertResourcePlaceholderAllocations(IEnumerable<ScheduleMasterPlaceholder> allocations);
        Task<OfficeHierarchy> GetOfficeHierarchy();
        Task<string> SendMonthlyStaffingAllocationsEmailToExperts(string employeeCodes);
        Task<string> SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(string employeeCodes);
    }
}
