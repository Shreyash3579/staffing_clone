using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRHub.Contracts.Services;
using SignalRHub.HubConfig;
using Staffing.SignalRHub.Models;
using System.Threading.Tasks;

namespace SignalRHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignalRController : ControllerBase
    {
        private readonly IHubContext<StaffingHub> _hub;
        private readonly ISignalRHubService _signalRHubService;

        public SignalRController(IHubContext<StaffingHub> hub, ISignalRHubService signalRHubService)
        {
            _hub = hub;
            _signalRHubService = signalRHubService;
        }

        /// <summary>
        /// Sned the case code on which Planning Cards are merged to PEG Queue
        /// </summary>
        /// <returns>true oronPegDataReceivedFromServiceBus false if addition to queue fails or succeeds</returns>
        [HttpPost("onPegDataReceivedFromServiceBus")]

        public async Task<IActionResult> OnPegDataReceivedFromServiceBus(PegOpportunity data)
        {
            var value = await _signalRHubService.OnPegDataReceivedFromServiceBus(data);
            return Ok(value);
        }

        [HttpPost("getUpdateOnSharedNotes")]

        public async Task<IActionResult> getUpdateOnSharedNotes([FromBody] string sharedWithEmployeeCodes)
        {
          var value  = await _signalRHubService.getUpdateOnSharedNotes(sharedWithEmployeeCodes);
          return Ok(value);
        }

        [HttpPost("getUpdateOnCaseIntakeChanges")]

        public async Task<IActionResult> getUpdateOnCaseIntakeChanges([FromBody] string sharedWithEmployeeCodes)
        {
            var value = await _signalRHubService.getUpdateOnCaseIntakeChanges(sharedWithEmployeeCodes);
            return Ok(value);
        }

        [HttpPost("getUpdateOnRingfenceCommitment")]

        public async Task<IActionResult> getUpdateOnRingfenceCommitment([FromBody] string employeeCodes)
        {
            var value = await _signalRHubService.getUpdateOnRingfenceCommitment(employeeCodes);
            return Ok(value);
        }

        public IActionResult SendUpdatesToAll()
        {
            _hub.Clients.All.SendAsync("onSendUpdates", "TestData");
            return Ok(new { Message = "Request Completed" });
        }
    }
}