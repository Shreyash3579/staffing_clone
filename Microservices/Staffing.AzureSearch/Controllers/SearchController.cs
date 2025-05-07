using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.AzureSearch.Contracts.Services;
using Staffing.AzureSearch.Models;

namespace Staffing.AzureSearch.Controllers
{
    /// <summary>
    /// Vacation Requests Controller
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _service;
        public SearchController(ISearchService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get results TOP 50 from cognitive search based on search string
        /// </summary>
        /// <remarks>
        /// Sample Request: 
        /// {
        ///   "mustHavesSearchString":"Managers in Boston who are available today",
        ///   "niceToHaveSearchString":null,
        ///   "searchTriggeredFrom":"home_searchAll",
        ///   "loggedInUser":"39209"
        /// }
        /// </remarks>
        /// <returns>List of results satisfying the mandatory criteria. Results that also match nice to have criteria are boosted to the top</returns>
        [HttpPost]
        [Route("resourcesBySearchString")]
        public async Task<IActionResult> GetResourcesBySearchString(BossSearchCriteria bossSearchCriteria)
        {
            var searchedResources = await _service.GetResourcesBySearchString(bossSearchCriteria.MustHavesSearchString, bossSearchCriteria.NiceToHaveSearchString, bossSearchCriteria.SearchTriggeredFrom, bossSearchCriteria.LoggedInUser);
            return Ok(searchedResources);
        }

        /// <summary>
        /// Get results TOP 50 results from cognitive search based on search string within the employee codes list passed in the payload
        /// </summary>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "loggedInUser":"39209",
        ///       "mustHavesSearchString":"Managers with experience in retail who are available next week",
        ///       "niceToHaveSearchString":"bilingual in french",
        ///       "searchTriggeredFrom": "home_SearchSupply",
        ///       "employeeCodesToSearchIn": "39209,37995,60074",
        ///       "pageSize": 50
        ///    }
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns>Searched Results</returns>
        [HttpPost]
        [Route("resourcesBySearchStringWithinSupply")]
        public async Task<IActionResult> GesourcesBySearchStringWithinSupply(dynamic payload)
        {
            var mustHavesSearchString = payload["mustHavesSearchString"].ToString();
            //get value from payload if exists
            var niceToHaveSearchString = !string.IsNullOrEmpty(payload["niceToHaveSearchString"].ToString()) ? payload["niceToHaveSearchString"].ToString() : null;
            var searchTriggeredFrom = payload["searchTriggeredFrom"].ToString();
            var loggedInUser = payload["loggedInUser"].ToString();
            var employeeCodesToSearchIn = payload["employeeCodesToSearchIn"].ToString();
            var searchedResources = await _service.GesourcesBySearchStringWithinSupply(mustHavesSearchString, niceToHaveSearchString, searchTriggeredFrom, loggedInUser, employeeCodesToSearchIn);
            return Ok(searchedResources);
        }

        /// <summary>
        /// Get results TOP 30 from cognitive search based on selected params
        /// To be used for Debugging purposes only
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "searchText": "levelname:'Managers' AND operatingofficeName: 'Boston'",
        ///     "filterQuery": "availabilityDates/any(a: a/date gt 2023-12-05T00:00:00Z) and languages/any(a: a/name eq 'English')"
        /// }
        /// </remarks>
        /// <param name="searchData"></param>
        /// <returns>Searched Results</returns>
        [HttpPost]
        [Route("resourcesByLuceneSearchQuery")]
        [Authorize]
        public async Task<IActionResult> GetResourcesByLuceneSearchQuery(OpenAIGeneratedSearchQuery searchData)
        {
            var searchedResources = await _service.GetResourcesByLuceneSearchQuery(searchData);
            return Ok(searchedResources);
        }

        /// <summary>
        /// Get results TOP 30 from cognitive search based on search string
        /// </summary>
        /// <remarks>
        /// Sample Request: Managers in Boston who are available today
        /// </remarks>
        /// <param name="searchString">Managers in Boston who are available today</param>
        /// <param name="loggedInUser">User who performed search</param>
        /// <returns>Searched Results</returns>
        [HttpGet]
        [Route("resourcesBySearchStringUsingVectorSearch")]
        [Authorize]
        public async Task<IActionResult> GetResourcesBySearchStringUsingVectorSearch(string searchString, string loggedInUser)
        {
            var data = await _service.GetResourcesBySearchStringUsingVectorSearch(searchString, loggedInUser);
            return Ok(data);
        }

    }
}
