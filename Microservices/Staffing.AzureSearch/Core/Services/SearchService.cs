using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure;
using Staffing.AzureSearch.Models;
using Azure.Search.Documents.Models;
using Staffing.AzureSearch.Contracts.Services;
using Staffing.AzureSearch.Core.Helpers;
using Newtonsoft.Json;

namespace Staffing.AzureSearch.Core.Services
{
    public class SearchService : ISearchService
    {
        private static SearchClient _searchClient;
        private static SearchIndexClient _indexClient;
        private static IQueryIntentOpenAIService _queryIntentOpenAIService;
        private static IEmbeddingGenerationService _embeddingGenerationService;
        private readonly IStaffingApiClient _staffingApiClient;

        public SearchService(IQueryIntentOpenAIService queryIntentOpenAIService, IStaffingApiClient staffingApiClient, IEmbeddingGenerationService embeddingGenerationService)
        {
            _queryIntentOpenAIService = queryIntentOpenAIService;
            _embeddingGenerationService = embeddingGenerationService;
            _staffingApiClient = staffingApiClient;
            InitSearch();
        }

        public async Task<BossSearchResult> GetResourcesBySearchString(string mustHavesSearchString, string niceToHaveSearchString, string searchTriggeredFrom, string loggedInUser)
        {
            

            if (string.IsNullOrEmpty(mustHavesSearchString) || string.IsNullOrEmpty(loggedInUser))
            {
                throw new ArgumentNullException("Must haves search string and LoggedInUser are required fields");
            }

            var searchResultsForMandatoryParams = await SearchAllResourcesBySearchString(mustHavesSearchString, searchTriggeredFrom, loggedInUser);
            
            //if (!string.IsNullOrEmpty(niceToHaveSearchString) && searchResultsForMandatoryParams.Searches.Any())
            //{
            //    var employeeCodesToSearchIn = string.Join(",", searchResultsForMandatoryParams.Searches.Select(x => x.Document.employeeCode).ToList());
            //    var searchResultsForNiceToHaveParams = await SearchResourcesWithinSupplyBySearchString(niceToHaveSearchString, searchTriggeredFrom, loggedInUser, employeeCodesToSearchIn);

            //    combinedResults.AddRange(searchResultsForMandatoryParams.Searches.IntersectBy(searchResultsForNiceToHaveParams.Searches.Select(x => x.Document.id), y => y.Document.id));
            //    combinedResults.AddRange(searchResultsForMandatoryParams.Searches.ExceptBy(searchResultsForNiceToHaveParams.Searches.Select(x => x.Document.id), y=> y.Document.id));

            //    return new BossSearchResult { GeneratedLuceneSearchQuery = searchResultsForMandatoryParams.GeneratedLuceneSearchQuery, GeneratedLuceneSearchQueryForNiceToHave = searchResultsForNiceToHaveParams.GeneratedLuceneSearchQuery,  Searches = combinedResults.Take(50).ToList() };
            //}
            if (!string.IsNullOrEmpty(niceToHaveSearchString))
            {
                var combinedResults = new List<SearchResult<Resource>>();
                var vectorSearchResults = await GetResourcesBySearchStringUsingVectorSearch(niceToHaveSearchString, loggedInUser);

                if (vectorSearchResults.Searches.Any())
                {
                    if (searchResultsForMandatoryParams.Searches.Any())
                    {
                        combinedResults.AddRange(searchResultsForMandatoryParams.Searches.IntersectBy(vectorSearchResults.Searches.Select(x => x.Document.id), y => y.Document.id));
                        combinedResults.AddRange(searchResultsForMandatoryParams.Searches.ExceptBy(vectorSearchResults.Searches.Select(x => x.Document.id), y => y.Document.id));
                    }
                    else
                    {
                        combinedResults = vectorSearchResults.Searches;
                    }

                    return new BossSearchResult { GeneratedLuceneSearchQuery = searchResultsForMandatoryParams.GeneratedLuceneSearchQuery, Searches = combinedResults.Take(50).ToList() };
                }
            }

            return new BossSearchResult { GeneratedLuceneSearchQuery = searchResultsForMandatoryParams.GeneratedLuceneSearchQuery, Searches = searchResultsForMandatoryParams.Searches.Take(50).ToList() }; ;
        }

        public async Task<BossSearchResult> GesourcesBySearchStringWithinSupply(string mustHavesSearchString, string niceToHaveSearchString, string searchTriggeredFrom, string loggedInUser, string employeeCodesToSearchIn)
        {
            if(string.IsNullOrEmpty(employeeCodesToSearchIn) || string.IsNullOrEmpty(mustHavesSearchString) || string.IsNullOrEmpty(loggedInUser))
            {
                throw new ArgumentNullException("EmployeeCodes, MustHavesSearchString and LoggedInUser are required fields");
            }

            var searchResultsForMandatoryParams = await SearchResourcesWithinSupplyBySearchString(mustHavesSearchString, searchTriggeredFrom, loggedInUser, employeeCodesToSearchIn);

            if (!string.IsNullOrEmpty(niceToHaveSearchString) && searchResultsForMandatoryParams.Searches.Any())
            {
                var employeeCodesToSearchInForNiceToHave = string.Join(",", searchResultsForMandatoryParams.Searches.Select(x => x.Document.employeeCode).ToList());
                var searchResultsForNiceToHaveParams = await SearchResourcesWithinSupplyBySearchString(niceToHaveSearchString, searchTriggeredFrom, loggedInUser, employeeCodesToSearchInForNiceToHave);

                var combinedResults = new List<SearchResult<Resource>>();
                combinedResults.AddRange(searchResultsForMandatoryParams.Searches.IntersectBy(searchResultsForNiceToHaveParams.Searches.Select(x => x.Document.id), y => y.Document.id));
                combinedResults.AddRange(searchResultsForMandatoryParams.Searches.ExceptBy(searchResultsForNiceToHaveParams.Searches.Select(x => x.Document.id), y => y.Document.id));

                return new BossSearchResult { GeneratedLuceneSearchQuery = searchResultsForMandatoryParams.GeneratedLuceneSearchQuery, GeneratedLuceneSearchQueryForNiceToHave = searchResultsForNiceToHaveParams.GeneratedLuceneSearchQuery, Searches = combinedResults.Take(50).ToList() };
            }

            return new BossSearchResult { GeneratedLuceneSearchQuery = searchResultsForMandatoryParams.GeneratedLuceneSearchQuery, Searches = searchResultsForMandatoryParams.Searches.Take(50).ToList() }; ;
        }

        public async Task<List<SearchResult<Resource>>> GetResourcesByLuceneSearchQuery(OpenAIGeneratedSearchQuery searchModel)
        {
            return await GetResourcesByLuceneSearchQuery(searchModel, Convert.ToInt16(ConfigService.GetSearchResultsCount()));
        }

        public async Task<List<SearchResult<Resource>>> GetResourcesByHybridSearchQuery(OpenAIGeneratedSearchQuery searchModel, string searchString)
        {
            var options = new SearchOptions()
            {
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Full,
                SearchMode = SearchMode.All,
                Size = Convert.ToInt16(ConfigService.GetSearchResultsCount())
            };

            if (!string.IsNullOrEmpty(searchModel.filter))
            {
                options.Filter = searchModel.filter.Replace("AND","and").Replace("OR","or") + " and staffingPreferences ne null";
            }

            var queryEmbeddings = await _embeddingGenerationService.GetEmbeddingsVectorFromOpenAI(searchString);

            if (queryEmbeddings != null)
            {
                // Perform the vector similarity search  
                options.VectorSearch = new()
                {
                    FilterMode = VectorFilterMode.PreFilter,
                    Queries =
                    {
                        new VectorizedQuery(queryEmbeddings.ToArray()) { KNearestNeighborsCount = 50, Fields = { "staffingPreferencesVector" } }
                    }
                };
            }

            // For efficiency, the search call should be asynchronous, so use SearchAsync rather than Search.
            var data = await _searchClient.SearchAsync<Resource>(searchModel.search, options).ConfigureAwait(false);
            var list = data.Value.GetResults().ToList();
            return list;
        }

        public  async Task<BossSearchResult> GetResourcesBySearchStringUsingVectorSearch(string searchString, string loggedInUser, int k = 5)
        {
            // Generate the embedding for the query  
            var queryEmbeddings = await _embeddingGenerationService.GetEmbeddingsVectorFromOpenAI(searchString);

            if (queryEmbeddings != null)
            {
                // Perform the vector similarity search  
                var searchOptions = new SearchOptions
                {
                    VectorSearch = new()
                    {
                        FilterMode = VectorFilterMode.PreFilter ,
                        Queries = {
                            new VectorizedQuery(queryEmbeddings.ToArray()) { KNearestNeighborsCount = 50, Fields = { "notesVector" } }
                        }  //staffingPreferencesVector, employeeVector
                    },
                    
                    Size = 50
                };

                var data = await _searchClient.SearchAsync<Resource>(null, searchOptions);
                var notesData = new List<ResourcePartial>();
                //int count = 0;

                var list = data.Value.GetResults().ToList();

                foreach (var result in list)
                {
                    notesData.Add(new ResourcePartial { id = result.Document.employeeCode, notes = result.Document.notes });
                    //count++;
                    //Console.WriteLine($"EmployeeCode: {result.Document.employeeCode}");
                    //Console.WriteLine($"Score: {result.Score}\n");
                    //Console.WriteLine($"notes: {result.Document.notes}");
                    //Console.WriteLine($"levelGrade: {result.Document.levelGrade}\n");
                }
                //Console.WriteLine($"Total Results: {count}");

                var notesJson = JsonConvert.SerializeObject(notesData);

                var data2 = await _queryIntentOpenAIService.GetCompletionsDataFromSearchResultsForNotes(searchString, notesJson);
                var data3 = !string.IsNullOrEmpty(data2) ? string.Join(",", JsonConvert.DeserializeObject<List<ResourceNotesPartial>>(data2).Select(x => x.id)) : "";
                var filtereddata = !string.IsNullOrEmpty(data2) ? list.Where(x => data3.Contains(x.Document.employeeCode)).ToList() : new List<SearchResult<Resource>>();
                return new BossSearchResult { GeneratedLuceneSearchQuery = new OpenAIGeneratedSearchQuery(), Searches = filtereddata }; ;


                //return filtereddata;
            }
            else
            {
                return new BossSearchResult();
            }
        }

        public async Task<List<SearchResult<Resource>>> GetResourcesByLuceneSearchQuery(OpenAIGeneratedSearchQuery searchModel, short pageSize)
        {
            searchModel.search = string.IsNullOrEmpty(searchModel.search) ? "*" : searchModel.search;
            //searchModel.filter = !string.IsNullOrEmpty(searchModel.filter) ? searchModel.filter.Replace("AND", "and").Replace("OR", "or") : searchModel.filter;

            var options = new SearchOptions()
            {
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Full,
                SearchMode = SearchMode.All,
                Size = pageSize
            };
            
            if (!string.IsNullOrEmpty(searchModel.filter))
            {
                options.Filter = searchModel.filter;
            }

            var data = await _searchClient.SearchAsync<Resource>(searchModel.search, options).ConfigureAwait(false);
            var list = data.Value.GetResults().ToList();
            return list;
        }

        #region Private Helpers
        private void InitSearch()
        {
            // Read the values from appsettings.json
            string searchServiceUri = ConfigService.GetSearchBaseUrl();
            string queryApiKey = ConfigService.GetSearchQueryApiKey();
            string searchIndex = ConfigService.GetSearchIndexName();

            // Create a service and index client.
            _indexClient = new SearchIndexClient(new Uri(searchServiceUri), new AzureKeyCredential(queryApiKey));
            _searchClient = _indexClient.GetSearchClient(searchIndex);
        }

        private async Task<BossSearchResult> SearchAllResourcesBySearchString(string searchString, string searchTriggeredFrom, string loggedInUser)
        {
            var luceneSearchQuery = await _queryIntentOpenAIService.GetAzureSearchQueryFromSearchText(searchString);
            var searchResponse = await GetFormattedSearchResultsByLuceneSearchQuery(luceneSearchQuery);

            LogSearchQueryToDB(searchString, searchTriggeredFrom, loggedInUser, searchResponse);
            return searchResponse;
        }

        private async Task<BossSearchResult> SearchResourcesWithinSupplyBySearchString(string searchString, string searchTriggeredFrom, string loggedInUser, string employeeCodesToSearchIn)
        {
            short pageSize = (short)employeeCodesToSearchIn.Split(',').Length;

            var luceneSearchQuery = await _queryIntentOpenAIService.GetAzureSearchQueryFromSearchText(searchString);
            luceneSearchQuery.filter = $"{luceneSearchQuery.filter} and search.in(employeeCode, '{employeeCodesToSearchIn.ToUpper()}')";
            var searchResponse = await GetFormattedSearchResultsByLuceneSearchQuery(luceneSearchQuery, pageSize);

            LogSearchQueryToDB(searchString, searchTriggeredFrom, loggedInUser, searchResponse);
            return searchResponse;
        }


        private async Task<BossSearchResult> GetFormattedSearchResultsByLuceneSearchQuery(OpenAIGeneratedSearchQuery luceneSearchQuery, short? pageSize = null)
        {
            BossSearchResult searchResponse;
            List<SearchResult<Resource>> searchResults;

            if (luceneSearchQuery.isErrorInGeneratingSearchQuery)
            {
                searchResponse = new BossSearchResult { GeneratedLuceneSearchQuery = luceneSearchQuery, Searches = null };
            }
            else
            {
                try
                {
                    searchResults = pageSize != null ? 
                        await GetResourcesByLuceneSearchQuery(luceneSearchQuery, pageSize.Value) : 
                        await GetResourcesByLuceneSearchQuery(luceneSearchQuery);
                    searchResponse = new BossSearchResult { GeneratedLuceneSearchQuery = luceneSearchQuery, Searches = searchResults };
                }
                catch (Exception ex)
                {
                    luceneSearchQuery.isErrorInGeneratingSearchQuery = true;
                    luceneSearchQuery.errorResponse = ex.Message;
                    searchResponse = new BossSearchResult { GeneratedLuceneSearchQuery = luceneSearchQuery, Searches = null };
                }

            }

            return searchResponse;
        }

        private void LogSearchQueryToDB(string searchString, string searchTriggeredFrom, string loggedInUser, BossSearchResult searchResponse)
        {
            var queryResponse = new AzureSearchQueryLog
            {
                EmployeeCode = loggedInUser,
                SearchString = searchString,
                SearchTriggeredFrom = searchTriggeredFrom,
                OpenAIGeneratedSearchQuery = JsonConvert.SerializeObject(searchResponse.GeneratedLuceneSearchQuery),
                IsErrorInOpenAiGeneratedSearchQuery = searchResponse.GeneratedLuceneSearchQuery.isErrorInGeneratingSearchQuery,
                SearchResultsCount = searchResponse.Searches?.Count ?? -1,
                LastUpdatedBy = "Auto-SearchLogging"
            };

            _staffingApiClient.InsertAzureSearchQueryLog(queryResponse);

        }
        #endregion
    }
}
