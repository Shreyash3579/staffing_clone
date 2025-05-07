using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vacation.API.Contracts.Services;

namespace Vacation.API.Controllers
{
    /// <summary>
    /// Vacation Requests Controller
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class QueryIntentOpenAIController : ControllerBase
    {
        private readonly IQueryIntentOpenAIService _service;
        private readonly ISearchService _searchService;
        public QueryIntentOpenAIController(IQueryIntentOpenAIService service, ISearchService searchService)
        {
            _service = service;
            _searchService = searchService;

        }

        /// <summary>
        /// Creates query intent for text using OpenAI chat completions API
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns>summarized text</returns>
        [HttpGet("getAzureSearchQueryFromSearchText")]
        public async Task<IActionResult> GetAzureSearchQueryFromSearchText(string searchQuery)
        {
            var queryIntent = await _service.GetAzureSearchQueryFromSearchText(searchQuery);
            return Ok(queryIntent);

        }

    }
}
