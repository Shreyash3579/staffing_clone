using Staffing.Analytics.API.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.RepositoryInterfaces
{
    public interface ICasePlanningMetricsPlaygroundRepository
    {
        Task<AvailabilityMetricsViewModel> CreateCasePlanningBoardMetricsPlayground(CasePlanningBoardPlaygroundFilters playgroundParameters,string createdBy);
    }
}
