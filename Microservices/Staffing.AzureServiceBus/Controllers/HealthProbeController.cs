using Microsoft.AspNetCore.Mvc;

namespace Staffing.AzureServiceBus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthProbeController : ControllerBase
    {
        [HttpGet("healthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok();
        }
    }
}
