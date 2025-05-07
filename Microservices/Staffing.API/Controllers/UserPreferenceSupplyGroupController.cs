using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class UserPreferenceSupplyGroupController : ControllerBase
    {
        private readonly IUserPreferenceSupplyGroupService _userPreferenceSupplyGroupService;
        public UserPreferenceSupplyGroupController(IUserPreferenceSupplyGroupService userPreferenceSupplyGroupService)
        {
            _userPreferenceSupplyGroupService = userPreferenceSupplyGroupService;
        }

        /// <summary>
        /// Get saved user supply groups set by user as their default settings.
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <returns>Saved user supply groups for employee</returns>
        [HttpGet]
        [Route("getUserPreferenceSupplyGroups")]
        public async Task<IActionResult> GetUserPreferenceSupplyGroups(string employeeCode)
        {
            var userPreferences = await _userPreferenceSupplyGroupService.GetUserPreferenceSupplyGroups(employeeCode);
            return Ok(userPreferences);
        }

        /// <summary>
        /// Insert/Update the supply groups from staffing settings
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// [
        ///    {
        ///       "id": null,
        ///       "name": "Test Group 1",
        ///       "description":"Test Group that manages some members",
        ///       "isDefault" : false,
        ///       "groupMemberCodes": "31021, 39209, 37995",
        ///       "createdBy":"39209",
        ///       "lastUpdatedBy": "39209"
        /// }]
        /// </remarks>
        /// <returns>Assigned resource(s) to case</returns>
        /// <param name="payload">Json representing one or more Resource Allocation</param>
        /// <response code="201">Returns Added and Updated resource(s) to case</response>
        /// <response code="400">If Payload is null or wrongly formatted</response>
        [HttpPost]
        [Route("upsertUserPreferenceSupplyGroups")]
        public async Task<IActionResult> UpsertUserPreferenceSupplyGroups(dynamic payload)
        {
            IEnumerable<UserPreferenceSupplyGroupViewModel> supplyGroupsToUpsert = JsonConvert.DeserializeObject<UserPreferenceSupplyGroupViewModel[]>(payload.ToString());
            var result = await _userPreferenceSupplyGroupService.UpsertUserPreferenceSupplyGroups(supplyGroupsToUpsert);
            
            return Ok(result);
        }

        /// <summary>
        /// Deletes the user created groups
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "listSupplyGroupIdsToDelete": "",
        ///     "lastUpdatedBy" : "39209"
        /// }
        /// </remarks>
        /// <param name="listSupplyGroupIdsToDelete"></param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("deleteUserPreferenceSupplyGroupByIds")]
        public async Task<IActionResult> DeletesUserPreferenceSupplyGroupByIds(string listSupplyGroupIdsToDelete, string lastUpdatedBy)
        {
            var deletedCaseRollIds = await _userPreferenceSupplyGroupService.DeleteUserPreferenceSupplyGroupByIds(listSupplyGroupIdsToDelete, lastUpdatedBy);
            return Ok(deletedCaseRollIds);
        }
    }
}
