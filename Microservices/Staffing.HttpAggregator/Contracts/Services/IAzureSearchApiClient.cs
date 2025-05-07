using Staffing.HttpAggregator.Models;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IAzureSearchApiClient
    {
        Task<BossSearchResult> GetResourcesBySearchString(BossSearchCriteria bossSearchCriteria);
    }
}
