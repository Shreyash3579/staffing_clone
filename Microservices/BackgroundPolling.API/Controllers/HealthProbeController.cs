using Microsoft.AspNetCore.Mvc;

namespace BackgroundPolling.API.Controllers
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
