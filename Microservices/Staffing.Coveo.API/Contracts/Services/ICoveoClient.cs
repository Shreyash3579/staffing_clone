using Staffing.Coveo.API.Models;
using Staffing.Coveo.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Coveo.API.Contracts.Services
{
    public interface ICoveoClient
    {        
        Task<(ResourcesViewModel, AnalyticsSearchViewModel)> SearchByResource(string searchTerm, string userDisplayName, string username, bool? test = false);
        Task<(IEnumerable<Case>, AnalyticsSearchViewModel)> SearchByCase(string searchTerm, string userDisplayName, string username);
        Task<dynamic> SearchByAllocation(string searchTerm);
        Task<(IEnumerable<Opportunity>, AnalyticsSearchViewModel)> SearchByOpportunity(string searchTerm, string userDisplayName, string username);
        Task<IEnumerable<Allocation>> UpsertOrDeleteAllocationIndexes(IEnumerable<ResourceAllocation> allocations);
    }
}
