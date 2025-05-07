using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailUtilityController : ControllerBase
    {
        private readonly IEmailUtilityService _emailUtilityService;
        public EmailUtilityController(IEmailUtilityService emailUtilityService)
        {
            _emailUtilityService = emailUtilityService;
        }

        /// <summary>
        /// Send an email for the audit logs of allocation done by staffing officers
        /// </summary>
        /// <param name="staffingUserECode">comma separated list of staffing office ECode</param>
        /// <param name="auditLogsFromDate">Date since logs required</param>
        /// <returns></returns>
        [HttpGet]
        [Route("testEmailWithO365")]
        public async Task<IActionResult> TestEmailWithO365()
        {
            await _emailUtilityService.TestEmailWithO365();
            return Ok();
        }

        /// <summary>
        /// Send an email for the audit logs of allocation done by staffing officers
        /// </summary>
        /// <param name="staffingUserECode">comma separated list of staffing office ECode</param>
        /// <param name="auditLogsFromDate">Date since logs required</param>
        /// <returns></returns>
        [HttpGet]
        [Route("sendEmailForAuditsOfStaffingUser")]
        public async Task<IActionResult> SendEmailForAuditsOfAllocationByStaffingUser(string staffingUserECode, DateTime auditLogsFromDate )
        {
            await _emailUtilityService.SendEmailForAuditsOfAllocationByStaffingUser(staffingUserECode, auditLogsFromDate);
            return Ok();
        }

        /// <summary>
        /// Send Email to EMEA staffing officer for the case served by Ringfence
        /// </summary>
        /// <param name="officeCodes">Comma separated list of offices</param>
        /// <param name="caseTypeCodes">Comma separated list of case types</param>
        /// <returns></returns>
        [HttpGet]
        [Route("SendEmailsForCasesServedByRingfence")]
        public async Task<IActionResult> SendEmailsForCasesServedByRingfenceByOfficeAndCaseType(string officeCodes, string caseTypeCodes)
        {
            await _emailUtilityService.SendEmailsForCasesServedByRingfenceByOfficeAndCaseType(officeCodes, caseTypeCodes);
            return Ok();
        }

        /// <summary>
        /// triggers email to Exoert group on staffing data. 
        /// Send to all experts is null is passed or to select group 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("sendMonthlyStaffingAllocationsEmailToExperts")]
        public async Task<IActionResult> SendMonthlyStaffingAllocationsEmailToExperts(string employeeCodes)
        {
            var responseMessage = await _emailUtilityService.SendMonthlyStaffingAllocationsEmailToExperts(employeeCodes);
            return Ok(responseMessage);
        }

        /// <summary>
        /// triggers email to APAC I&D group on staffing data. 
        /// Send to all if null is passed or to select group 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("sendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign")]
        public async Task<IActionResult> SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(string employeeCodes)
        {
            var responseMessage = await _emailUtilityService.SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(employeeCodes);
            return Ok(responseMessage);
        }
    }
}
