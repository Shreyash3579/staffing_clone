using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class UserPreferenceGroupSharedInfoController : ControllerBase
    {
        private readonly IUserPreferenceGroupSharedInfoService _userPreferenceGroupSharedInfoService;
        public UserPreferenceGroupSharedInfoController(IUserPreferenceGroupSharedInfoService userPreferenceSupplyGroupSharedInfoService)
        {
            _userPreferenceGroupSharedInfoService = userPreferenceSupplyGroupSharedInfoService;
        }

        /// <summary>
        /// Get saved user supply groups set by user as their default settings.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>Saved user supply groups for employee</returns>
        [HttpGet]
        [Route("getUserPreferenceGroupSharedInfo")]
        public async Task<IActionResult> GetUserPreferenceGroupSharedInfo(string groupId)
        {
            var userPreferenceGroupSharedInfo = await _userPreferenceGroupSharedInfoService.GetUserPreferenceGroupSharedInfo(groupId);
            return Ok(userPreferenceGroupSharedInfo);
        }

        /// <summary>
        /// Insert/Update the shared group info from staffing settings
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// [
        ///    {
        ///       "id": "",
        ///       "sharedWith": "58749",
        ///       "isDefault" : false,
        ///       "userPreferenceSupplyGroupId": "",
        ///       "lastUpdatedBy": "58749"
        /// }]
        /// </remarks>
        /// <returns>Assigned resource(s) to case</returns>
        /// <param name="payload">Json representing one or more Resource Allocation</param>
        /// <response code="201">Returns Added and Updated resource(s) to case</response>
        /// <response code="400">If Payload is null or wrongly formatted</response>
        [HttpPost]
        [Route("upsertUserPreferenceGroupSharedInfo")]
        public async Task<IActionResult> UpsertUserPreferenceGroupSharedInfo(dynamic payload)
        {
            IEnumerable<UserPreferenceGroupSharedInfo> groupsSharedInfoToUpsert = JsonConvert.DeserializeObject<UserPreferenceGroupSharedInfo[]>(payload.ToString());
            var result = await _userPreferenceGroupSharedInfoService.UpsertUserPreferenceGroupSharedInfo(groupsSharedInfoToUpsert);

            return Ok(result);
        }

        /// <summary>
        /// Insert/Update the shared group info from staffing settings
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// [
        ///    {
        ///       "id": "",
        ///       "sharedWith": "58749",
        ///       "isDefault" : false,
        ///       "userPreferenceSupplyGroupId": "",
        ///       "lastUpdatedBy": "58749"
        /// }]
        /// </remarks>
        /// <returns>Assigned resource(s) to case</returns>
        /// <param name="payload">Json representing one or more Resource Allocation</param>
        /// <response code="201">Returns Added and Updated resource(s) to case</response>
        /// <response code="400">If Payload is null or wrongly formatted</response>
        [HttpPost]
        [Route("updateUserPreferenceSupplyGroupSharedInfo")]
        public async Task<IActionResult> UpdateUserPreferenceSupplyGroupSharedInfo(dynamic payload)
        {
            IEnumerable<UserPreferenceGroupSharedInfo> supplyGroupsSharedInfoToUpsert = JsonConvert.DeserializeObject<UserPreferenceGroupSharedInfo[]>(payload.ToString());
            var result = await _userPreferenceGroupSharedInfoService.UpdateUserPreferenceSupplyGroupSharedInfo(supplyGroupsSharedInfoToUpsert);

            return Ok(result);
        }
    }
}
