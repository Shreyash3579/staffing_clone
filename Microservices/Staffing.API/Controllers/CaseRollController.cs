using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class CaseRollController : ControllerBase
    {
        private readonly ICaseRollService _caseRollService;

        public CaseRollController(ICaseRollService caseRollService)
        {
            _caseRollService = caseRollService;
        }

        /// <summary>
        ///     Get Cases on Roll by Old Case codes
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getCasesOnRollByCaseCodes")]
        public async Task<IActionResult> GetCasesOnRollByCaseCodes(dynamic payload)
        {
            var oldCaseCodes = $"{payload["oldCaseCodes"]}";
            var casesOnRoll = await _caseRollService.GetCasesOnRollByCaseCodes(oldCaseCodes);
            return Ok(casesOnRoll);
        }

        /// <summary>
        /// Upserts data for when case roll(s) are updated
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// [{
        ///     "rolledFromOldCaseCode": "K7FC",
        ///     "rolledFromOldCaseCode": null
        ///     "currentCaseEndDate": "10-Jul-2020",
        ///     "expectedCaseEndDate": "30-Jul-2020",
        ///     "lastUpdatedBy": "39209"
        /// }]
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertCaseRolls")]
        public async Task<IActionResult> UpsertCaseRolls(dynamic payload)
        {
            var updatedCaseRolls = JsonConvert.DeserializeObject<IEnumerable<CaseRoll>>(payload.ToString());
            var updatedCaseRollsResponse = await _caseRollService.UpsertCaseRolls(updatedCaseRolls);
            return Ok(updatedCaseRollsResponse);
        }

        /// <summary>
        /// Deletes the case roll(s) and their corresponding data from caseRollScheduleMaster mapping table
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "caseRollIds": "",
        ///     "lastUpdatedBy" : "39209"
        /// }
        /// </remarks>
        /// <param name="caseRollIds"></param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("deleteCaseRollsByIds")]
        public async Task<IActionResult> DeleteCaseRollsByIds(string caseRollIds, string lastUpdatedBy)
        {
            var deletedCaseRollIds = await _caseRollService.DeleteCaseRollsByIds(caseRollIds, lastUpdatedBy);
            return Ok(deletedCaseRollIds);
        }

        /// <summary>
        /// Deletes rolled allocations from caseRollScheduleMaster
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "rolledScheduleIds": "",
        ///     "lastUpdatedBy" : "39209"
        /// }
        /// </remarks>
        /// <param name="rolledScheduleIds"></param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("deleteRolledAllocationsByScheduleIds")]
        public async Task<IActionResult> DeleteRolledAllocationsByScheduleIds (string rolledScheduleIds, string lastUpdatedBy)
        {
            var deletedCaseRollIds = await _caseRollService.DeleteRolledAllocationsByScheduleIds(rolledScheduleIds, lastUpdatedBy);
            return Ok(deletedCaseRollIds);
        }

        /// <summary>
        ///     Get All Cases that were rolled and have not been processed from CCM.
        ///     Needed for Polling purpose
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("unprocessedCasesOnCaseRoll")]
        public async Task<IActionResult> GetAllUnprocessedCasesOnCaseRoll()
        {
            var casesOnRoll = await _caseRollService.GetAllUnprocessedCasesOnCaseRoll();
            return Ok(casesOnRoll);
        }

        /// <summary>
        /// Deletes rolled allocations mapping (in caseRollScheduleMaster) for rolled Case Codes so that these allocations are not affected by changes in CCM case dates
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "rolledCaseCodes": "",
        ///     "lastUpdatedBy" : "39209"
        /// }
        /// </remarks>
        /// <param name="lastUpdatedBy"></param>
        /// <param name="rolledCaseCodes">pass case codes for deletions on these case codes else pass NULL for all</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("deleteRolledAllocationsMappingFromCaseRollTracking")]
        public async Task<IActionResult> DeleteRolledAllocationsMappingFromCaseRollTracking(string lastUpdatedBy, string rolledCaseCodes = null)
        {
            await _caseRollService.DeleteRolledAllocationsMappingFromCaseRollTracking(lastUpdatedBy, rolledCaseCodes);
            return Ok("Task Completed Successfully");
        }

        /// <summary>
        ///     Get all rolled cases that have been recently processed in Staffing. This will be used in correction job to convert billable to pre-post
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("caseRollsRecentlyProcessedInStaffing")]
        public async Task<IActionResult> GetCaseRollsRecentlyProcessedInStaffing(DateTime lastPollDateTime)
        {
            var casesOnRoll = await _caseRollService.GetCaseRollsRecentlyProcessedInStaffing(lastPollDateTime);
            return Ok(casesOnRoll);
        }

    }
}