using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.AzureSearch.Contracts.Services;

namespace Staffing.AzureSearch.Controllers
{
    /// <summary>
    /// Azure Search Query Intent Controller
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class QueryIntentOpenAIController : ControllerBase
    {
        private readonly IQueryIntentOpenAIService _service;
        public QueryIntentOpenAIController(IQueryIntentOpenAIService service)
        {
            _service = service;
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
