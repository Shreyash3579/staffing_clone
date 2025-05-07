using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailUtilityDataLogController : ControllerBase
    {
        private readonly IEmailUtilityDataLogService _emailUtilityDataLogServiceservice;

        public EmailUtilityDataLogController(IEmailUtilityDataLogService emailUtilityDataLogServiceservice)
        {
            _emailUtilityDataLogServiceservice = emailUtilityDataLogServiceservice;
        }

        /// <summary>
        /// Gets log of email utility for the date. It will return the data containing users for which email is in-progress, success or failed
        /// </summary>
        /// <returns>Comma Separated List of employees</returns>
        /// <param name="dateOfEmail">Scheduled Date of email utility JOb</param>
        /// <param name="emailType">Type of Email Utility : expert email or I&D emails</param>
        [HttpGet]
        [Route("getEmailUtilityDataLogsByDateAndEmailType")]
        public async Task<IActionResult> GetEmailUtilityDataLogsByDateAndEmailType(DateTime dateOfEmail, string emailType)
        {
            var listFailedEmployeeCodes = await _emailUtilityDataLogServiceservice.GetEmailUtilityDataLogsByDateAndEmailType(dateOfEmail, emailType);
            return Ok(listFailedEmployeeCodes);
        }

        /// <summary>
        /// Save employees who successfully recieved email
        /// </summary>
        /// <returns>Saved employee(s)</returns>
        /// <param name="emailUtilityData">array of one or more Email Utility Data</param>
        /// <response code="201">Returns Added employee(s)</response>
        /// <response code="400">If Payload is null or wrongly formatted</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Route("upsertEmailUtilityDataLog")]
        public async Task<IActionResult> UpsertEmailUtilityDataLog(IEnumerable<EmailUtilityData> emailUtilityData)
        {
            var result = await _emailUtilityDataLogServiceservice.UpsertEmailUtilityDataLog(emailUtilityData);
            return Ok(result);
        }
    }
}