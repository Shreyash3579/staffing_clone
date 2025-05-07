using Microsoft.AspNetCore.Mvc;

namespace CaseIntake.API.Controllers
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
