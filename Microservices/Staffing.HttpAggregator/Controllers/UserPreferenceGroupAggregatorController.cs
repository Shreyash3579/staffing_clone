using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPreferenceGroupAggregatorController : ControllerBase
    {
        private readonly IUserPreferenceGroupService _userPreferenceGroupsService;
        public UserPreferenceGroupAggregatorController(IUserPreferenceGroupService userPreferenceSupplyGroupsService)
        {
            _userPreferenceGroupsService = userPreferenceSupplyGroupsService;
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
            var userPreferences = await _userPreferenceGroupsService.GetUserPreferenceSupplyGroupsDetails(employeeCode);
            return Ok(userPreferences);
        }

        /// <summary>
        /// Get saved user saved groups set by user as their default settings.
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <returns>Saved user saved groups for employee</returns>
        [HttpGet]
        [Route("getUserPreferenceSavedGroups")]
        public async Task<IActionResult> GetUserPreferenceSavedGroups(string employeeCode)
        {
            var userPreferences = await _userPreferenceGroupsService.GetUserPreferenceSavedGroupsDetails(employeeCode);
            return Ok(userPreferences);
        }

        /// <summary>
        /// Get saved user supply groups shared with users by passing the group ID.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>Saved user supply groups for employee</returns>
        [HttpGet]
        [Route("getUserPreferenceGroupSharedInfo")]
        public async Task<IActionResult> GetUserPreferenceGroupSharedInfo(string groupId)
        {
            var userPreferences = await _userPreferenceGroupsService.GetUserPreferenceGroupSharedInfo(groupId);
            return Ok(userPreferences);
        }

        [HttpPost]
        [Route("upsertUserPreferencesSupplyGroupWithSharedInfo")]
        public async Task<IActionResult> UpsertUserPreferencesSupplyGroupWithSharedInfo(dynamic payload)
        {
            var supplyGroupsToUpsert = JsonConvert.DeserializeObject<UserPreferenceSupplyGroupWithSharedInfo[]>(payload.ToString());
            var result = await _userPreferenceGroupsService.UpsertUserPreferencesSupplyGroupWithSharedInfo(supplyGroupsToUpsert);

            return Ok(result);
        }

        [HttpPost]
        [Route("upsertUserPreferencesSavedGroupWithSharedInfo")]
        public async Task<IActionResult> UpsertUserPreferencesSavedGroupWithSharedInfo(dynamic payload)
        {
            var savedGroupsToUpsert = JsonConvert.DeserializeObject<UserPreferenceSavedGroupWithSharedInfo[]>(payload.ToString());
            var result = await _userPreferenceGroupsService.UpsertUserPreferencesSavedGroupWithSharedInfo(savedGroupsToUpsert);

            return Ok(result);
        }

        [HttpPost]
        [Route("upsertUserPreferenceGroupSharedInfo")]
        public async Task<IActionResult> UpsertUserPreferenceGroupSharedInfo(dynamic payload)
        {
            IEnumerable<UserPreferenceGroupSharedInfo> groupsSharedInfoToUpsert = JsonConvert.DeserializeObject<UserPreferenceGroupSharedInfo[]>(payload.ToString());
            var result = await _userPreferenceGroupsService.UpsertUserPreferenceGroupSharedInfo(groupsSharedInfoToUpsert);

            return Ok(result);
        }
    }
}
