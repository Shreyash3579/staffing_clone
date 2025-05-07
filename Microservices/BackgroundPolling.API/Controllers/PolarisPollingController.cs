using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolarisPollingController : ControllerBase
    {
        public IPolarisPollingService _polarisPollingService;
        public PolarisPollingController(IPolarisPollingService polarisPollingService)
        {
            _polarisPollingService = polarisPollingService;
        }

        /// <summary>
        /// Upsert security users saved in Polaris system and AD groups into Boss system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpsertPolarisSecurityUsers()
        {
            await _polarisPollingService.UpsertSecurityUsers();
            return Ok();
        }
    }
}
