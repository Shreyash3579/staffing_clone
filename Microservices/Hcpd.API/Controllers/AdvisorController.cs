using Hcpd.API.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Hcpd.API.Controllers
{
    /// <summary>
    /// Advisor Controller
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class AdvisorController : ControllerBase
    {
        private readonly IAdvisorService _advisorService;
     
        public AdvisorController(IAdvisorService advisorService)
        {
            _advisorService = advisorService;
        }

        /// <summary>
        /// Get reviewr/advisor employee code
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <returns>Approved/Pending vacations</returns>
        [HttpGet("advisorByEmployeeCode")]
        public async Task<IActionResult> GetAdvisorByEmployeeCode(string employeeCode)
        {
            var advisor = await _advisorService.GetAdvisorByEmployeeCode(employeeCode);
            return Ok(advisor);
        }

        /// <summary>
        /// Get mentee by employee code
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <returns>IEnumerable of mentees</returns>
        [HttpGet("menteesByEmployeeCode")]
        public async Task<IActionResult> GetMenteesByEmployeeCode(string employeeCode)
        {
            var mentees = await _advisorService.GetMenteesByEmployeeCode(employeeCode);
            return Ok(mentees);
        }
    }
}
