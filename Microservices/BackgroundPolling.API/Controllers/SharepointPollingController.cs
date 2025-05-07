using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharepointPollingController : ControllerBase
    {
        private readonly ISharepointPollingService _sharepointPollingService;

        public SharepointPollingController(ISharepointPollingService sharepointPollingService)
        {
            _sharepointPollingService = sharepointPollingService;
        }

        /// <summary>
        /// Upsert SMAP Mission notes from Sharepoint Site to Notes table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertSignedOffSMAPMissions")]
        public async Task<IActionResult> UpsertSignedOffSMAPMissions()
        {
            await _sharepointPollingService.UpsertSignedOffSMAPMissions();
            return Ok();
        }

        /// <summary>
        /// Upsert Staffing prefrences for Sharepoint site to employee preferences table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertStaffingPreferences")]
        public async Task<IActionResult> UpsertStaffingPreferences()
        {
            await _sharepointPollingService.UpsertStaffingPreferences();
            return Ok();
        }

    }
}
