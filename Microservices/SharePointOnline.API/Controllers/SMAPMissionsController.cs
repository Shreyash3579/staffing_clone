using Microsoft.AspNetCore.Mvc;
using SharePointOnline.API.Contracts.Services;

namespace SharePointOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMAPMissionsController : Controller
    {
        private readonly ISMAPMissionsService _service;

        public SMAPMissionsController(ISMAPMissionsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get signed-off SMAP missions  from SharePoint List
        /// </summary>
        /// <returns></returns>
        [HttpGet("getSMAPMissions")]
        public async Task<IActionResult> GetSMAPMissions()
        {
            var result = await _service.GetSMAPMissions();
            return Ok(result);
        }

    }
}
