using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ADPollingController : ControllerBase
    {
        private readonly IADSecurityUserService _ADSecurityUserService;
        public ADPollingController(IADSecurityUserService ADSecurityUserService)
        {
            _ADSecurityUserService = ADSecurityUserService;
        }

        /// <summary>
        /// Gets Security Users from AD Group to Staffing DB to provide access to BOSS
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> GetADSecurityUsers(string accountName)
        //{
        //    var securityUsers = await _ADSecurityUserService.GetBOSSSecurityUsersFromAD(accountName);
        //    return Ok(securityUsers);
        //}
    }
}
