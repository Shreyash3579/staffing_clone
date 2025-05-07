using System.Threading.Tasks;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffingPollingController : Controller
    {
        private readonly IStaffingPollingService _staffingPollingService;

        public StaffingPollingController(IStaffingPollingService staffingPollingService)
        {
            _staffingPollingService = staffingPollingService;
        }

        /// <summary>
        /// delete security users from boss system whose endDate is expired
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("deleteSecurityUsersWithExpiredEndDate")]
        public async Task<IActionResult> DeleteSecurityUsersWithExpiredEndDate()
        {
            var result = await _staffingPollingService.DeleteSecurityUsersWithExpiredEndDate();
            return Ok(result);
        }

        [HttpDelete]
        [Route("deleteAnalyticsLog")]
        public async Task<IActionResult> DeleteAnalyticsLog()
        {
            await _staffingPollingService.DeleteAnalyticsLog();
            return Ok();
        }


        [HttpPost]
        [Route("updateSecurityUserForWFPRole")]
        public async Task<IActionResult> UpdateSecurityUserForWFPRole()
        {
          await _staffingPollingService.UpdateSecurityUserForWFPRole();
            return Ok();

        }
    }
}
