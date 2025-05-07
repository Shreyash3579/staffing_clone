using Staffing.AzureSearch.Models;
using Azure.Search.Documents.Models;

namespace Staffing.AzureSearch.Contracts.Services
{
    public interface ISearchService
    {
        Task<BossSearchResult> GetResourcesBySearchString(string mustHavesSearchString, string niceToHaveSearchString, string searchTriggeredFrom, string loggedInUser);
        Task<BossSearchResult> GesourcesBySearchStringWithinSupply(string mustHavesSearchString, string niceToHaveSearchString, string searchTriggeredFrom, string loggedInUser, string employeeCodesToSearchIn);
        Task<List<SearchResult<Resource>>> GetResourcesByLuceneSearchQuery(OpenAIGeneratedSearchQuery searchModel);
        Task<BossSearchResult> GetResourcesBySearchStringUsingVectorSearch(string searchString, string loggedInUser, int k = 3);
    }
}
