using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class ScheduleMasterPlaceholderController : ControllerBase
    {
        private readonly IScheduleMasterPlaceholderService _placeholderService;

        public ScheduleMasterPlaceholderController(IScheduleMasterPlaceholderService placeholderService)
        {
            _placeholderService = placeholderService;
        }

        /// <summary>
        /// Get Placeholder Allocations by Ids
        /// </summary>
        /// <remarks>
        /// Sample Request: "xyz,abc"
        /// </remarks>
        /// <param name="placeholderScheduleIds">Comma separated Placeholder Schedule Ids</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("allocationsByPlaceholderScheduleIds")]
        public async Task<IActionResult> GetAllocationsByPlaceholderScheduleIds([FromBody] string placeholderScheduleIds)
        {
            var allocations = await _placeholderService.GetPlaceholderAllocationsByPlaceholderScheduleIds(placeholderScheduleIds);
            return Ok(allocations);
        }

        /// <summary>
        /// Get Resource Allocations to selected opportunities
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "pipelineIds": ""
        /// }
        /// </remarks>
        /// <param name="payload">comma separated list of pipeline Id</param>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("allocationsByPipelineIds")]
        public async Task<IActionResult> GetPlaceholderAllocationsByPipelineIds(dynamic payload)
        {
            var pipelineIds = $"{payload["pipelineIds"]}";
            var allocations = await _placeholderService.GetPlaceholderAllocationsByPipelineIds(pipelineIds);
            return Ok(allocations);
        }

        /// <summary>
        /// Get Resource Allocations to selected cases
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "oldCaseCodes": ""
        /// }
        /// </remarks>
        /// <param name="payload">comma separated list of old case code</param>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("allocationsByCaseCodes")]
        public async Task<IActionResult> GetPlaceholderAllocationsByCaseCodes(dynamic payload)
        {
            var oldCaseCodes = $"{payload["oldCaseCodes"]}";
            var allocations = await _placeholderService.GetPlaceholderAllocationsByCaseCodes(oldCaseCodes);
            return Ok(allocations);
        }

        /// <summary>
        /// Get placeholder allocations to selected planning cards
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "planningCardIds": ""
        /// }
        /// </remarks>
        /// <param name="payload">comma separated list of planning card ids</param>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("placeholderAllocationsByPlanningCardIds")]
        public async Task<IActionResult> GetPlaceholderAllocationsByPlanningCardIds(dynamic payload)
        {
            var planningCardIds = $"{payload["planningCardIds"]}";
            var allocations = await _placeholderService.GetPlaceholderAllocationsByPlanningCardIds(planningCardIds);
            return Ok(allocations);
        }
        /// <summary>
        /// Get both placeholder allocations and regular allocations to selected Planning cards
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "planningCardIds": ""
        /// }
        /// </remarks>
        /// <param name="payload">comma separated list of planning card ids</param>

        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("allocationsByPlanningCardIds")]
        //placeholder
        public async Task<IActionResult> GetAllocationsByPlanningCardIds(dynamic payload)
        {
            var planningCardIds = $"{payload["planningCardIds"]}";
            var allocations = await _placeholderService.GetAllocationsByPlanningCardIds(planningCardIds);
            return Ok(allocations);
        }
        /// <summary>
        /// Add/Update placeholder allocation on case/opp
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>Placeholder Allocation data</returns>
        /// <param name="payload">Json representingPlaceholde Resource Allocation</param>
        /// <response code="201">Returns Added and Updated resource </response>
        /// <response code="400">If Payload is null or wrongly formatted</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Route("upsertPlaceholderResourceAllocation")]
        public async Task<IActionResult> UpsertPlaceholderAllocation(dynamic payload)
        {
            var placeholderAllocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(payload.ToString());
            var result = await _placeholderService.UpsertPlaceholderAllocation(placeholderAllocations);
            return Ok(result);
        }

        /// <summary>
        /// Delete Placeholder Resource Allocation by placeholder id
        /// </summary>
        /// <param name="placeholderIds"></param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeletePlaceholderAllocationsByIds(string placeholderIds, string lastUpdatedBy)
        {
            await _placeholderService.DeletePlaceholderAllocationsByIds(placeholderIds, lastUpdatedBy);
            return Ok();
        }

        /// <summary>
        /// Get Placeholder Resource Allocations within date range
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "employeeCodes" : "37995,42AKS,42624", 
        ///     "startDate": "2019-10-01,
        ///     "endDate": "2020-01-31"
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns>list of placeholder allocations</returns>
        [HttpPost]
        [Route("placeholderPlanningCardAllocationsWithinDateRange")]
        public async Task<IActionResult> GetPlacedholdersAllocationsWithinDateRange(dynamic payload)
        {
            var employeeCodes = !string.IsNullOrEmpty(payload["employeeCodes"]?.ToString()) ? payload["employeeCodes"].ToString() : null;
            var startDate = !string.IsNullOrEmpty(payload["startDate"]?.ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"]?.ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
            var result = await _placeholderService.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate);
            return Ok(result);
        }

        /// <summary>
        /// Get Placeholder allocations of selected employees and within Date Range
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "employeeCodes" : "37995,42AKS,42624", 
        ///     "startDate": "2019-10-01,
        ///     "endDate": "2020-01-31"
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("placeholderAllocationsByEmployees")]
        public async Task<IActionResult> GetPlaceholderAllocationsByEmployeeCodes(dynamic payload)
        {
            var employeeCodes = payload["employeeCodes"].ToString();
            var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
            var placeholderAllocations = await _placeholderService.GetPlaceholderAllocationsByEmployeeCodes(employeeCodes, startDate, endDate);
            return Ok(placeholderAllocations);
        }
    }
}