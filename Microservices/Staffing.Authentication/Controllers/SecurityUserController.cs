using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.Authentication.Contracts.Services;
using Staffing.Authentication.Core.Enums;
using System.Threading.Tasks;

namespace Staffing.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityUserController : ControllerBase
    {
        private readonly ISecurityUserService _userService;

        public SecurityUserController(ISecurityUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("authenticate")]
        public IActionResult AuthenticateApp(string appName, string appSecret)
        {
            var token = _userService.AuthenticateApp(appName, appSecret);
            return Ok(token);
        }

        /// <summary>
        /// Get Employee details along with feature access that it has in the token
        /// </summary>
        /// <param name="userCode">employee code to get data for</param>
        /// <returns></returns>
        [HttpGet("loggedInUser")]
        [Authorize]
        public async Task<IActionResult> GetLoggedInUser(string userCode)
        {
            // TODO: Remove parameter [userCode] before deploying it to production. This is kept for testing purpose so that test by impersonating user
            var employeeCode = userCode ?? HttpContext.User.Identity.Name.Replace("BAIN\\", "");

            if (employeeCode.ToLower().Contains("c-"))
            {
                employeeCode = employeeCode.ToLower().Replace("c-", ""); //THis hack is provided since contractors are now set-up as C-<Ecode> at Bain due to which they are not getting access
            }

            var employee = await _userService.GetEmployeeByEmployeeCode(employeeCode);
            return Ok(employee);
        }

        // This API endpoint is written to perform Integration test for getting employee and authorization token for the employee.
        // Since windows authentication can not be provides by Xunit, this is the only way to test this.
        //[ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("impersonatedUser")]
        public async Task<IActionResult> GetImpersonatedUser(string userCode)
        {
            var employee = await _userService.GetEmployeeByEmployeeCode(userCode);
            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Refresh(string token, string refreshToken)
        {
            var refreshedToken = await _userService.RefreshToken(token, refreshToken);
            return Ok(refreshedToken);
        }

        /// <summary>
        /// Register application to allow access staffing resources
        /// </summary>
        /// <param name="appName">app that is going to consume staffing resources</param>
        /// <param name="claims">app claims</param>
        /// <returns></returns>
        [HttpGet("registerApp")]
        [Authorize]
        public async Task<IActionResult> RegisterApp(string appName, AppClaim claim)
        {
            var employeeCode = HttpContext.User.Identity.Name.Replace("BAIN\\", "");
            var result = await _userService.RegisterApp(employeeCode, appName, claim);
            return result == null ? Ok($"App {appName} already registered with {claim.ToString()}") : Ok(result);
        }
    }
}