using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IScheduleMasterPlaceholderService
    {
        Task<IEnumerable<ResourceAllocationViewModel>> GetPlaceholderAllocationsByPlaceholderScheduleIds(string placeholderScheduleIds);
        Task<IEnumerable<ResourceAllocationViewModel>> GetPlaceholderAllocationsByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<ResourceAllocationViewModel>> GetPlaceholderAllocationsByPipelineIds(string pipelineIds);
        Task<IEnumerable<ScheduleMasterPlaceholder>> UpsertPlaceholderAllocation(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations);
        Task DeletePlaceholderAllocationsByIds(string placeholderIds, string lastUpdatedBy);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderPlanningCardAllocationsWithinDateRange(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByEmployeeCodes(string employeeCodes,DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ResourceAllocationViewModel>> GetPlaceholderAllocationsByPlanningCardIds(string planningCardIds);

        Task<IEnumerable<ScheduleMasterPlaceholder>> GetAllocationsByPlanningCardIds(string planningCardIds);
    }
}
