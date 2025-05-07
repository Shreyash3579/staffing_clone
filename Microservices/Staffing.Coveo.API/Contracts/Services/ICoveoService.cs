using Staffing.Coveo.API.Models;
using Staffing.Coveo.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Coveo.API.Contracts.Services
{
    public interface ICoveoService
    {
        Task<dynamic> SearchByQuery(string searchTerm, string source, string userDisplayName, string username, string userIPAddress, bool? test = false);
        Task<IEnumerable<Allocation>> UpsertOrDeleteAllocationIndexes(IEnumerable<ResourceAllocation> allocations);
        Task<dynamic> LogClickEventInCoveoAnalytics(AnalyticsClickViewModel analyticsClickParams, string userIPAddress);
    }
}
