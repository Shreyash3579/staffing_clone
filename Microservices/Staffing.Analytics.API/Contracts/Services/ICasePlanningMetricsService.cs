using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface ICasePlanningMetricsService
    {
        Task<AvailabilityMetricsViewModel> GetAvailabilityMetricsByFilterValues(SupplyFilterCriteria supplyFilterCriteria);
        Task<AvailabilityMetricsViewModel> UpsertAndGetCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(Guid playgroundId, IEnumerable<CasePlanningBoardPlaygroundAllocation> playgroundAllocations, string lastUpdatedBy);
        Task<AvailabilityMetricsViewModel> GetAvailabilityMetricsForPlaygroundById(string playgroundId);
        Task<Guid> DeleteCasePlanningBoardMetricsPlaygroundById(Guid playgroundId, string lastUpdatedBy);
        Task<CasePlanningBoardPlaygroundFilters> GetCasePlanningBoardPlaygroundFiltersByPlaygroundId(string playgroundId);
        
    }
}
