using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface ISearchService
    {
        Task<SearchResponseViewModel> GetResourcesBySearchString(BossSearchCriteria azureSearchQuery);
    }
}
