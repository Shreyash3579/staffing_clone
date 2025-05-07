using Microsoft.AspNetCore.Mvc;
using Polaris.API.Contracts.Services;
using System.Threading.Tasks;

namespace Polaris.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityUserController : ControllerBase
    {
        private readonly ISecurityUserService _service;

        public SecurityUserController(ISecurityUserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get Rev users who should have Finance access to BOSS and their office mapping
        /// </summary>
        /// <returns></returns>
        [HttpGet("getrevuserpersona")]
        public async Task<IActionResult> GetRevSecurityUsersWithGeography()
        {
            var result = await _service.GetRevSecurityUsersWithGeography();
            return Ok(result);
        }
    }
}
