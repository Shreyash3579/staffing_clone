using Microsoft.AspNetCore.Mvc;
using SharePointOnline.API.Contracts.Services;

namespace SharePointOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffingPreferencesController : Controller
    {
        private readonly ISMAPMissionsService _service;

        public StaffingPreferencesController(ISMAPMissionsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get staffing preferences for employees
        /// </summary>
        /// <returns></returns>
        [HttpGet("getStaffingPreferences")]
        public async Task<IActionResult> GetStaffingPreferences()
        {
            var result = await _service.GetStaffingPreferences();
            return Ok(result);
        }
    }
}
