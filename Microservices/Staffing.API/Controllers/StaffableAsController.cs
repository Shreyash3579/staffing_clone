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
    public class StaffableAsController : ControllerBase
    {
        private readonly IStaffableAsService _staffableAsService;

        public StaffableAsController(IStaffableAsService staffableAsService)
        {
            _staffableAsService = staffableAsService;
        }

        /// <summary>
        /// Gets the active staffable as role for employee
        /// </summary>
        /// <sample>
        /// {
        ///     "employeeCodes": "abc,xyz"
        /// }
        /// </sample>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("activeStaffableAsByEmployeeCodes")]
        public async Task<IActionResult> GetResourceActiveStaffableAsByEmployeeCodes(dynamic payload)
        {
            var employeeCodes = $"{payload["employeeCodes"]}";
            var staafbleAsRoles = await _staffableAsService.GetResourceActiveStaffableAsByEmployeeCodes(employeeCodes);
            return Ok(staafbleAsRoles);
        }

        /// <summary>
        /// Inserts/Update changs to the staffable as role for an employee
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// [{
        /// "id": null,
        /// "employeeCode": "39209",
        /// "effectiveDaye" : "22-May-2021",
        /// "staffableAsTypeCode" : 1,
        /// "isActive" : 1
        /// }]
        /// </remarks>
        /// <param name="employeesStaffableAs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertResourceStaffableAs")]
        public async Task<IActionResult> UpsertResourceStaffableAs (IEnumerable<StaffableAs> employeesStaffableAs)
        {
            var upsertedData = await _staffableAsService.UpsertResourceStaffableAs(employeesStaffableAs);
            return Ok(upsertedData);
        }

        /// <summary>
        /// Deletes the staffable as role for the resource
        /// </summary>
        /// <param name="idToDelete"></param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteResourceStaffableAsById(string idToDelete, string lastUpdatedBy)
        {
            var deletedId = await _staffableAsService.DeleteResourceStaffableAsById(idToDelete, lastUpdatedBy);
            return Ok();
        }

    }
}