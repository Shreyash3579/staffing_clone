using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StaffingPreferencesController : ControllerBase
    {
        private readonly IStaffingPreferencesService _staffingPreferencesService;
        public StaffingPreferencesController(IStaffingPreferencesService staffingPreferencesService)
        {
            _staffingPreferencesService = staffingPreferencesService;
        }
        /// <summary>
        /// Get all staffing preferences for employee
        /// </summary>
        /// <param name="employeeCode">employee for which preferences are to be fetched</param>
        /// <returns>staffing preferences object containing data about employee's staffing needs </returns>
        [HttpGet("getEmployeePreferences")]
        [Authorize]
        public async Task<IActionResult> GetEmployeePreferences(string employeeCode)
        {
            var securityUsers = await _staffingPreferencesService.GetEmployeePreferences(employeeCode);
            return Ok(securityUsers);
        }

        /// <summary>
        /// Get all staffing preferences for employee
        /// </summary>
        /// <returns>staffing preferences object containing data about employee's staffing needs </returns>
        [HttpGet("getAllEmployeePreferences")]
        [Authorize]
        public async Task<IActionResult> GetAllEmployeePreferences()
        {
            var securityUsers = await _staffingPreferencesService.GetAllEmployeePreferences();
            return Ok(securityUsers);
        }

        /// <summary>
        /// Update staffig preferences for employee
        /// </summary>
        /// <param name="securityUser"></param>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "employeeCode":"55555",
        ///       "lastUpdatedBy":"51030",
        ///       "isAdmin": false
        ///    }
        /// </remarks>
        /// <returns></returns>
        [HttpPost("upsertEmployeePreferences")]
        [Authorize]
        public async Task<IActionResult> UpsertEmployeePreferences(EmployeeStaffingPreferencesForInsightsTool securityUser)
        {
            var updatedSecurityUser = await _staffingPreferencesService.UpsertEmployeePreferences(securityUser);
            return Ok(updatedSecurityUser);
        }
    }
}
