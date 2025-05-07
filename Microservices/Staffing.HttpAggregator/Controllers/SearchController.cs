using Microsoft.AspNetCore.Mvc;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Get Resources from Azure Search by the search string
        /// mustHavesSearchString: search croteria that should mandatorily be in results
        /// niceToHaveSearchString: search criteria that is not mandatory but nice to have
        /// </summary>
        /// <param name="azureSearchQuery">Search Query object</param>
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
        [HttpPost("resourcesBySearchString")]
        public async Task<IActionResult> GetResourcesBySearchString(BossSearchCriteria azureSearchQuery)
        {
            var data = await _searchService.GetResourcesBySearchString(azureSearchQuery);
            return Ok(data);
        }
    }
}