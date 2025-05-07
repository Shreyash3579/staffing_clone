using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using System;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpertEmailUtilityController : ControllerBase
    {
        private readonly IExpertEmailUtilityService _expertEmailUtilityService;

        public ExpertEmailUtilityController(IExpertEmailUtilityService expertEmailUtilityService)
        {
            _expertEmailUtilityService = expertEmailUtilityService;
        }

        /// <summary>
        /// Send monthly emails to Expert groups on their monthly staffing History 
        /// and Retry for failed employees
        /// </summary>
        /// <param employeeCodes="string">If employeeCodes are passed, email will only be sent to them</param>
        /// <returns></returns>
        [HttpPost("sendMonthlyStaffingAllocationsEmailToExperts")]
        public async Task<IActionResult> SendMonthlyStaffingAllocationsEmailToExperts([FromBody]string employeeCodes = null)
        {
            var employeeCodesWithSuccessEmails = await _expertEmailUtilityService.SendMonthlyStaffingAllocationsEmailToExperts(employeeCodes);
            return Ok(employeeCodesWithSuccessEmails);
        }

        /// <summary>
        /// Send monthly emails to Expert groups on their monthly staffing History 
        /// and Retry for failed employees
        /// </summary>
        /// <param employeeCodes="string">If employeeCodes are passed, email will only be sent to them</param>
        /// <returns></returns>
        [HttpPost("sendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign")]
        public async Task<IActionResult> SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign([FromBody] string employeeCodes = null)
        {
            var employeeCodesWithSuccessEmails = await _expertEmailUtilityService.SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(employeeCodes);
            return Ok(employeeCodesWithSuccessEmails);
        }
    }
}