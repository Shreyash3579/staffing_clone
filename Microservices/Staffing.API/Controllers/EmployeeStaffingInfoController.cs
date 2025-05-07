using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Services;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class EmployeeStaffingInfoController : ControllerBase
    {
        private readonly IEmployeeStaffingInfoService _employeeStaffingInfoService;

        public EmployeeStaffingInfoController(IEmployeeStaffingInfoService employeeStaffingInfoService)
        {
            _employeeStaffingInfoService = employeeStaffingInfoService;
        }
        /// <summary>
        /// Gets the staffing responsible data for employee
        /// <returns></returns>
        [HttpGet]
        [Route("getResourceStaffingResponsibleByEmployeeCodes")]
        public async Task<IActionResult> GetResourceStaffingResponsibleByEmployeeCodes(string employeeCodes)
        {
            var staffingResponsibleData = await _employeeStaffingInfoService.GetResourceStaffingResponsibleByEmployeeCodes(employeeCodes);
            return Ok(staffingResponsibleData);
        }
        /// <summary>
        /// Inserts/Update changs to the staffing responsible data for an employee
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// [{
        /// "id": null,
        /// "employeeCode": "39209",
        /// "responsibleForStaffing" : "39212",
        /// "lastUpdatedBy" : "39202",
        /// }]
        /// </remarks>
        /// <param name="staffingResponsibleData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertResourceStaffingResponsible")]
        public async Task<IActionResult> UpsertResourceStaffingResponsible(IEnumerable<StaffingResponsible> staffingResponsibleData)
        {
            var upsertedData = await _employeeStaffingInfoService.UpsertResourceStaffingResponsible(staffingResponsibleData);
            return Ok(upsertedData);
        }

    }
}