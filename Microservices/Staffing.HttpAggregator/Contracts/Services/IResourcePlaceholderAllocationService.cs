using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IResourcePlaceholderAllocationService
    {
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPipelineIds(string pipelineIds);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetAllocationsByPlanningCardIds(string planningCardIds, string effectiveDate);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPlanningCardIds(string planningCardIds);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByEmployeeCode(string employeeCode, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<ResourceAssignmentViewModel>> UpsertPlaceholderAllocations(
           IEnumerable<ResourceAssignmentViewModel> PlaceholderAllocations);

        Task<IEnumerable<ResourceAssignmentViewModel>> UpsertCaseRollsAndPlaceholderAllocations(IEnumerable<CaseRoll> caseRolls, IEnumerable<ResourceAssignmentViewModel> resourceAllocations);
    }
}
