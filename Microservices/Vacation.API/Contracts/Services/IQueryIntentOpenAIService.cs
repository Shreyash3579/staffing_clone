using System.Threading.Tasks;
using Vacation.API.Models;

namespace Vacation.API.Contracts.Services
{
    public interface IQueryIntentOpenAIService
    {
        Task<BossSearchQuery> GetAzureSearchQueryFromSearchText(string searchQuery);
    }
}
