using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.ViewModels;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeStaffingPreferenceController : ControllerBase
    {
        private readonly IEmployeeStaffingPreferenceService _employeeStaffingPreferenceService;

        public EmployeeStaffingPreferenceController(IEmployeeStaffingPreferenceService employeeStaffingPreferenceService)
        {
            _employeeStaffingPreferenceService = employeeStaffingPreferenceService;
        }

        /// <summary>
        /// Get employee industry and capability preferences for staffing
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetEmployeeStaffingPreferences(string employeeCode)
        {
            var result = await _employeeStaffingPreferenceService.GetEmployeeStaffingPreferences(employeeCode);
            return Ok(result);
        }


        /// <summary>
        /// Save/Update employee staffing preferences
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        ///     {
        ///         "employeeCode": "37995",
        ///         "preferenceType": "C",
        ///         "lastUpdatedBy": "39209",
        ///         "staffingPreferences": 
        ///         [
        ///             {
        ///                 "code":"10",
        ///                 "name":"Strategy"
        ///             },
        ///             {
        ///                 "code":"32",
        ///                 "name":"Technology & Analytics"
        ///             },
        ///             {
        ///                 "code":"11",
        ///                 "name":"Performance Improvement"
        ///             }
        ///         ]       
        ///     }
        /// </remarks>
        [HttpPut]
        public async Task<IActionResult> UpsertEmployeeStaffingPreferences(EmployeeStaffingPreferencesViewModel employeeStaffingPreferencesToUpsert)
        {
            var result = await _employeeStaffingPreferenceService.UpsertEmployeeStaffingPreferences(employeeStaffingPreferencesToUpsert);
            return Ok(result);
        }
    }
}
