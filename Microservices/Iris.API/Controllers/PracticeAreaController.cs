using Iris.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Iris.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeAreaController : ControllerBase
    {
        private readonly IPracticeAreaService _service;

        public PracticeAreaController(IPracticeAreaService service)
        {
            _service = service;
        }

        
        /// <summary>
        /// Get lookup data for industry practice area
        /// </summary>
        /// <returns></returns>
        [HttpGet("industryPracticeAreaLookup")]
        public async Task<IActionResult> GetAllIndustryPracticeArea()
        {
            var result = await _service.GetAllIndustryPracticeArea();
            return Ok(result);
        }


        /// <summary>
        /// Get lookup data for capability practice area
        /// </summary>
        /// <returns></returns>
        [HttpGet("capabilityPracticeAreaLookup")]
        public async Task<IActionResult> GetAllCapabilityPracticeArea()
        {
            var result = await _service.GetAllCapabilityPracticeArea();
            return Ok(result);
        }
    }
}
