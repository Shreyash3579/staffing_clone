using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
    public class EmbeddingGenerationController : ControllerBase
    {
        private readonly IEmbeddingGenerationService _service;
        public EmbeddingGenerationController(IEmbeddingGenerationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Creates Embedding
        /// </summary>
        /// <param name="textToEmbed"></param>
        /// <returns>Vector representation of text</returns>
        [HttpGet("getEmbeddingsVectorFromOpenAI")]
        public async Task<IActionResult> GetEmbeddingsVectorFromOpenAI(string textToEmbed)
        {
            var embeddedText = await _service.GetEmbeddingsVectorFromOpenAI(textToEmbed);
            return Ok(embeddedText);

        }

        /// <summary>
        /// Creates Embedding
        /// </summary>
        /// <param name="keyValuePairs">key is the key of dictionary and vlaue stores the text to be embedded</param>
        /// <returns>Dictonary of Vector representation of text itmes with key holding key of dictionary and value the embedded value</returns>
        [HttpPost("getMultipleEmbeddingsVectorFromOpenAI")]
        public async Task<IActionResult> GetMultipleEmbeddingsVectorFromOpenAI([FromBody] Dictionary<string, string> keyValuePairs)
        {
            var embeddedDictionary = await _service.GetMultipleEmbeddingsVectorFromOpenAI(keyValuePairs);
            return Ok(embeddedDictionary);

        }
    }
}
