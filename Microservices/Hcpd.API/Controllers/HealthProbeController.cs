using Microsoft.AspNetCore.Mvc;

namespace Hcpd.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    //new comment added new 1
    public class HealthProbeController : ControllerBase
    {
        [HttpGet("healthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok();
        }
    }
}
