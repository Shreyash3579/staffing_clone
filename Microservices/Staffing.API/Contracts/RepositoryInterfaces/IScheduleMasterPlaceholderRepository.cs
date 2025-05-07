using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IScheduleMasterPlaceholderRepository
    {
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPlaceholderScheduleIds(string placeholderScheduleIds);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPipelineIds(string pipelineIds);
        Task<IEnumerable<ScheduleMasterPlaceholder>> UpsertPlaceholderAllocation(DataTable placeholderAllocationsDatatable);
        Task DeletePlaceholderAllocationsByIds(string placeholderIds, string lastUpdatedBy);
        Task<IList<ScheduleMasterPlaceholder>> GetPlaceholderPlanningCardAllocationsWithinDateRange(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByEmployeeCodes(string employeeCodes,DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPlanningCardIds(string planningCardIds);

        Task<IEnumerable<ScheduleMasterPlaceholder>> GetAllocationsByPlanningCardIds(string planningCardIds);
    }
}
