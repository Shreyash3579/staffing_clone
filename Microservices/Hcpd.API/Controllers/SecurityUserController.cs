using Hcpd.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Hcpd.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityUserController : ControllerBase
    {
        private readonly ISecurityUserService _securityUserService;
        public SecurityUserController(ISecurityUserService securityUserService)
        {
            _securityUserService = securityUserService;
        }


        /// <summary>
        /// Get Review and rating for an employee
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <returns></returns>
        [HttpGet("securityUserDetails")]
        public async Task<IActionResult> GetSecurityUserDetails(string employeeCode)
        {
            var reviews = await _securityUserService.GetSecurityUserDetails(employeeCode);
            return Ok(reviews);
        }
    }
}
