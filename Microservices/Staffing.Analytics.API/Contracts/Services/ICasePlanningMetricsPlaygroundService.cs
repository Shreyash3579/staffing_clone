using Staffing.Analytics.API.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface ICasePlanningMetricsPlaygroundService
    {
        Task<AvailabilityMetricsViewModel> CreateCasePlanningBoardMetricsPlayground(DemandFilterCriteria demandFilterCriteria, SupplyFilterCriteria supplyFilterCriteria,
            bool isCountOfIndividualResourcesToggle, bool enableMemberGrouping, bool enableNewlyAvailableHighlighting, string lastUpdatedBy);
    }
}
