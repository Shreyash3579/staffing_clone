using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vacation.API.Contracts.Services;
using Vacation.API.Models;

namespace Vacation.API.Controllers
{
    /// <summary>
    /// Vacation Requests Controller
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _service;
        public SearchController(ISearchService service)
        {
            _service = service;
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
        [Route("resourcesBySearchString")]
        [Authorize]
        public async Task<IActionResult> GetResourcesBySearchString(string searchString, string loggedInUser)
        {
            var searchedResources = await _service.GetResourcesBySearchString(searchString, loggedInUser);
            return Ok(searchedResources);
        }

        /// <summary>
        /// Get results TOP 30 from cognitive search based on selected params
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
        public async Task<IActionResult> GetResourcesByLuceneSearchQuery(BossSearchQuery searchData)
        {
            var searchedResources = await _service.GetResourcesByLuceneSearchQuery(searchData);
            return Ok(searchedResources);
        }

    }
}
