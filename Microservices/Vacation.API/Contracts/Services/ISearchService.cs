using System.Threading.Tasks;
using Vacation.API.Models;
using Azure.Search.Documents.Models;
using System.Collections.Generic;

namespace Vacation.API.Contracts.Services
{
    public interface ISearchService
    {
        Task<BossSearchResult> GetResourcesBySearchString(string searchString, string loggedInUser);
        Task<List<SearchResult<Resource>>> GetResourcesByLuceneSearchQuery(BossSearchQuery searchModel);
    }
}
