using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure;
using System;
using System.Threading.Tasks;
using Vacation.API.Models;
using Azure.Search.Documents.Models;
using Vacation.API.Contracts.Services;
using Vacation.API.Core.Helpers;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Vacation.API.Core.Services
{
    public class SearchService : ISearchService
    {
        private static SearchClient _searchClient;
        private static SearchIndexClient _indexClient;
        private static IQueryIntentOpenAIService _queryIntentOpenAIService;
        private readonly IStaffingApiClient _staffingApiClient;

        public SearchService(IQueryIntentOpenAIService queryIntentOpenAIService, IStaffingApiClient staffingApiClient)
        {
            _queryIntentOpenAIService = queryIntentOpenAIService;
            _staffingApiClient = staffingApiClient;
        }

        public async Task<BossSearchResult> GetResourcesBySearchString(string searchString, string loggedInUser)
        {
            var luceneSearchQuery = await _queryIntentOpenAIService.GetAzureSearchQueryFromSearchText(searchString);
            BossSearchResult searchResponse;
            if(luceneSearchQuery.isErrorInGeneratingSearchQuery)
            {
                searchResponse = new BossSearchResult { GeneratedLuceneSearchQuery = luceneSearchQuery, Searches = null };
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(luceneSearchQuery.search))
                    {
                        var searchResults = await GetResourcesByLuceneSearchQuery(luceneSearchQuery);

                        searchResponse = new BossSearchResult { GeneratedLuceneSearchQuery = luceneSearchQuery, Searches = searchResults };
                    }
                    else
                    {
                        luceneSearchQuery.isErrorInGeneratingSearchQuery = true;
                        searchResponse = new BossSearchResult { GeneratedLuceneSearchQuery = luceneSearchQuery, Searches = null };
                    }
                    
                }
                catch (Exception ex)
                {
                    luceneSearchQuery.isErrorInGeneratingSearchQuery = true;
                    luceneSearchQuery.errorResponse = ex.Message;
                    searchResponse =  new BossSearchResult { GeneratedLuceneSearchQuery = luceneSearchQuery, Searches = null };
                }

            }

            LogSearchQueryToDB(searchString, loggedInUser, searchResponse);

            return searchResponse;

        }

        public async Task<List<SearchResult<Resource>>> GetResourcesByLuceneSearchQuery(BossSearchQuery searchModel)
        {
            InitSearch();

            var options = new SearchOptions()
            {
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Full,
                SearchMode = SearchMode.Any,
                Size = Convert.ToInt16(ConfigurationUtility.GetValue("AzureSearch:ResultsCount"))
            };

            if (!string.IsNullOrEmpty(searchModel.filter))
            {
                options.Filter = searchModel.filter.Replace("AND", "and").Replace("OR", "or");
            }

            // For efficiency, the search call should be asynchronous, so use SearchAsync rather than Search.
            var data = await _searchClient.SearchAsync<Resource>(searchModel.search, options).ConfigureAwait(false);
            var list = data.Value.GetResults().ToList();
            return list;
        }

        private void InitSearch()
        {
            // Read the values from appsettings.json
            string searchServiceUri = ConfigurationUtility.GetValue("AzureSearch:BaseUrl");
            string queryApiKey = ConfigurationUtility.GetValue("AzureSearch:QueryApiKey");
            string searchIndex = ConfigurationUtility.GetValue("AzureSearch:IndexName");

            // Create a service and index client.
            _indexClient = new SearchIndexClient(new Uri(searchServiceUri), new AzureKeyCredential(queryApiKey));
            _searchClient = _indexClient.GetSearchClient(searchIndex);
        }

        private void LogSearchQueryToDB(string searchString, string loggedInUser, BossSearchResult searchResponse)
        {
            var queryResponse = new AzureSearchQueryLog
            {
                EmployeeCode = loggedInUser,
                SearchString = searchString,
                OpenAIGeneratedSearchQuery = JsonConvert.SerializeObject(searchResponse.GeneratedLuceneSearchQuery),
                IsErrorInOpenAiGeneratedSearchQuery = searchResponse.GeneratedLuceneSearchQuery.isErrorInGeneratingSearchQuery,
                SearchResultsCount = searchResponse.Searches?.Count ?? -1,
                LastUpdatedBy = "Auto-SearchLogging"
            };

            _staffingApiClient.InsertAzureSearchQueryLog(queryResponse);

        }
    }
}
