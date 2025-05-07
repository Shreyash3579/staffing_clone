using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.ViewModels;
using Staffing.HttpAggregator.Models;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResourcePlaceholderAllocationAggregatorController : ControllerBase
    {
        private readonly IResourcePlaceholderAllocationService _placeholderAllocationService;

        public ResourcePlaceholderAllocationAggregatorController(IResourcePlaceholderAllocationService placeholderAllocationService)
        {
            _placeholderAllocationService = placeholderAllocationService;
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpsertPlaceholderAllocations(dynamic payload)
        {
            IEnumerable<ResourceAssignmentViewModel> placeholderAllocations = JsonConvert.DeserializeObject<ResourceAssignmentViewModel[]>(payload.ToString());
            var result = await _placeholderAllocationService.UpsertPlaceholderAllocations(placeholderAllocations);
            return Ok(result);
        }


        [HttpPost]
        [Route("upsertCaseRollsAndPlaceholderAllocations")]
        public async Task<IActionResult> UpsertCaseRollsAndPlaceholderAllocations(dynamic payload)
        {
            var caseRoll = JsonConvert.DeserializeObject<CaseRoll[]>(payload["caseRolls"].ToString());
            IEnumerable<ResourceAssignmentViewModel> resourceAllocations = JsonConvert.DeserializeObject<ResourceAssignmentViewModel[]>(payload["resourceAllocations"].ToString());

            var result = await _placeholderAllocationService.UpsertCaseRollsAndPlaceholderAllocations(caseRoll, resourceAllocations);

            return Ok(result);
        }


        /// <summary>
        /// Gets Placeholders Allocations on cases
        /// </summary>
        /// <remarks>
        /// Smaple Request:
        /// {
        ///     "oldCaseCodes" : ""
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("allocationsByCaseCodes")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPlaceholderAllocationsByCaseCodes(dynamic payload)
        {
            var oldCaseCodes = payload["oldCaseCodes"].ToString();
            var result = await _placeholderAllocationService.GetPlaceholderAllocationsByCaseCodes(oldCaseCodes);
            return Ok(result);
        }

        /// <summary>
        /// Gets Placeholders Allocations on opportunites
        /// </summary>
        /// <remarks>
        /// Smaple Request:
        /// {
        ///     "pipelineIds" : ""
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("allocationsByPipelineIds")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPlaceholderAllocationsByPipelineIds(dynamic payload)
        {
            var pipelineIds = payload["pipelineIds"].ToString();
            var result = await _placeholderAllocationService.GetPlaceholderAllocationsByPipelineIds(pipelineIds);
            return Ok(result);
        }

        /// <summary>
        /// Gets Placeholders Allocations on Planning Cards
        /// </summary>
        /// <remarks>
        /// Smaple Request:
        /// {
        ///     "planningCardIds" : ""
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("placeholderAllocationsByPlanningCardIds")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPlaceholdersAllocationsByPlanningCardIds(dynamic payload)
        {

            var planningCardIds = payload["planningCardIds"].ToString();      
            var result = await _placeholderAllocationService.GetPlaceholderAllocationsByPlanningCardIds(planningCardIds);
            return Ok(result);
        }

        [HttpPost]
        [Route("allocationsByPlanningCardIds")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetAllocationsByPlanningCardIds(dynamic payload)
        {

            var planningCardIds = payload["planningCardIds"].ToString();
            var effectiveDate = payload["effectiveDate"].ToString();
            var result = await _placeholderAllocationService.GetAllocationsByPlanningCardIds(planningCardIds, effectiveDate);
            return Ok(result);
        }

        /// <summary>
        /// Gets placeholder allocations for a resource based on date range
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="effectiveFromDate"></param>
        /// <param name="effectiveToDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("allocationsByEmployeeCode")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPlaceholderAllocationsByByEmployeeCode(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var result = await _placeholderAllocationService.GetPlaceholderAllocationsByEmployeeCode(employeeCode, effectiveFromDate, effectiveToDate);
            return Ok(result);
        }
    }
}