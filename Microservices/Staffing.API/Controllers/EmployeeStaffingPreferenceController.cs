using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class EmployeeStaffingPreferenceController : ControllerBase
    {
        private readonly IEmployeeStaffingPreferenceService _employeeStaffingPreferenceService;
        public EmployeeStaffingPreferenceController(IEmployeeStaffingPreferenceService employeeStaffingPreferenceService)
        {
            _employeeStaffingPreferenceService = employeeStaffingPreferenceService;
        }

        /// <summary>
        ///      Get Employee staffing preferences saved in staffing system
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetEmployeeStaffingPreferences(dynamic payload)
        {
            var employeeCode = $"{payload["employeeCode"]}";
            var result = await _employeeStaffingPreferenceService.GetEmployeeStaffingPreferences(employeeCode);
            return Ok(result);
        }

        /// <summary>
        /// Upsert employee staffing preferences
        /// </summary>
        /// <param name="employeeStaffingPreferences"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        /// [
        ///     {
        ///         "employeeCode":"37995",
        ///         "preferenceTypeCode": "I",
        ///         "staffingPreference":"4",
        ///         "priority":1,
        ///         "lastUpdatedBy":"39209"
        ///     },
        ///     {
        ///         "employeeCode":"37995",
        ///         "preferenceTypeCode": "I",
        ///         "staffingPreference":"16",
        ///         "priority":2,
        ///         "lastUpdatedBy":"39209"
        ///     },
        ///     {
        ///         "employeeCode":"37995",
        ///         "preferenceTypeCode": "I",
        ///         "staffingPreference":"3",
        ///         "priority":3,
        ///         "lastUpdatedBy":"39209"
        ///     },
        ///     {
        ///         "employeeCode":"37995",
        ///         "preferenceTypeCode": "C",
        ///         "staffingPreference":"11",
        ///         "priority":1,
        ///         "lastUpdatedBy":"39209"
        ///     },
        ///      {
        ///         "employeeCode":"37995",
        ///         "preferenceTypeCode": "C",
        ///         "staffingPreference":"10",
        ///         "priority":2,
        ///         "lastUpdatedBy":"39209"         
        ///     }
        /// ]
        /// </remarks>
        [HttpPut]
        public async Task<IActionResult> UpsertEmployeeStaffingPreferences(IEnumerable<EmployeeStaffingPreferences> employeeStaffingPreferences)
        {
            var result = await _employeeStaffingPreferenceService.UpsertEmployeeStaffingPreferences(employeeStaffingPreferences);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEmployeeStaffingPreferenceByType(string employeeCode, string preferenceTypeCode)
        {
            await _employeeStaffingPreferenceService.DeleteEmployeeStaffingPreferenceByType(employeeCode, preferenceTypeCode);
            return Ok();
        }
    }
}
