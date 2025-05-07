using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class UserCustomFilterController : ControllerBase
    {
        private readonly IUserCustomFilterService _userFilterService;

        public UserCustomFilterController(IUserCustomFilterService userFilterService)
        {
            _userFilterService = userFilterService;
        }

        /// <summary>
        /// Get custom filters created by user for resources
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomResourceFiltersByEmployeeCode(string employeeCode)
        {
            var savedFilterForUser = await _userFilterService.GetCustomResourceFiltersByEmployeeCode(employeeCode);
            return Ok(savedFilterForUser);
        }

        /// <summary>
        /// Upsert custom filters created by user for resources
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// [{
        ///     "id": null,
        ///     "title": "Boston 2 weeks out",
        ///     "employeeCode": "39209",
        ///     "officeCodes": "110",
        ///     "levelGrades": "M1,M2",
        ///     "staffingTags":"SL0001",
        ///     "employeeStatuses":"active",
        ///     "lastUpdatedBy": "39209",
        ///     "staffableAsTypeCodes": "1,2"
        /// }]
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpsertCustomResourceFilters(dynamic payload)
        {
            var upsertedResourceFilters = JsonConvert.DeserializeObject<IEnumerable<ResourceFilterViewModel>>(payload.ToString());
            var updatedCaseRollsResponse = await _userFilterService.UpsertCustomResourceFilters(upsertedResourceFilters);
            return Ok(updatedCaseRollsResponse);
        }

        /// <summary>
        /// Delete custom filters created by user for resources
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "filterIdToDelete": "",
        ///     "lastUpdatedBy" : "39209"
        /// }
        /// </remarks>
        /// <param name="filterIdToDelete"></param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomResourceFilterById(string filterIdToDelete, string lastUpdatedBy)
        {
            var affiliations = await _userFilterService.DeleteCustomResourceFilterById(filterIdToDelete, lastUpdatedBy);
            return Ok(affiliations);
        }
    }
}