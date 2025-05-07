using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Services;
using Staffing.HttpAggregator.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectAggregatorController : ControllerBase
    {
        private readonly IProjectAggregatorService _service;

        public ProjectAggregatorController(IProjectAggregatorService service)
        {
            _service = service;
        }

        /// <summary>
        ///     Get opportunity and cases data along with their allocation filtered by selected criteria
        /// </summary>
        /// <param name="payload">Pass loggedInUser param if casePlanningViewNotes are required</param>
        /// <returns></returns>
        [HttpPost("projectBySelectedValues")]
        public async Task<IActionResult> GetOpportunitiesAndCasesWithAllocationsBySelectedValues(dynamic payload)
        {
            var filterObj = payload != null && payload.ContainsKey("demandFilterCriteria")
                ? JsonConvert.DeserializeObject<DemandFilterCriteria>(payload["demandFilterCriteria"].ToString())
                : JsonConvert.DeserializeObject<DemandFilterCriteria>(payload.ToString());
            var loggedInUser = payload != null && payload.ContainsKey("loggedInUser") ? $"{payload["loggedInUser"]}" : null;

            var result =
                await _service.GetOpportunitiesAndCasesWithAllocationsBySelectedValues(filterObj, loggedInUser);
            return Ok(result);
        }

        /// <summary>
        ///     Get opportunity and cases data along with their allocation filtered by selected criteria
        /// </summary>
        /// <param name="payload">Pass loggedInUser param if casePlanningViewNotes are required</param>
        /// <returns></returns>
        [HttpPost("newProjectBySelectedValues")]
        public async Task<IActionResult> GetOpportunitiesAndNewDemandWithAllocationsBySelectedValues(dynamic payload)
        {
            var filterObj = payload != null && payload.ContainsKey("demandFilterCriteria")
                ? JsonConvert.DeserializeObject<DemandFilterCriteria>(payload["demandFilterCriteria"].ToString())
                : JsonConvert.DeserializeObject<DemandFilterCriteria>(payload.ToString());
            var loggedInUser = payload != null && payload.ContainsKey("loggedInUser") ? $"{payload["loggedInUser"]}" : null;

            var result =
                await _service.GetOpportunitiesAndNewDemandWithAllocationsBySelectedValues(filterObj, loggedInUser);
            return Ok(result);
        }

        /// <summary>
        /// Get Opportunity details along with its allocations by pipeline Id
        /// </summary>
        /// <param name="pipelineId"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        [HttpGet("opporunityDetailsByPipelineId")]
        public async Task<IActionResult> GetOpportunityDetailsWithAllocationByPipelineId(string pipelineId, string loggedInUser = null)
        {
            var result =
                await _service.GetOpportunityDetailsWithAllocationByPipelineId(pipelineId, loggedInUser);
            return Ok(result);
        }

        /// <summary>
        /// Get Case details along with its allocations by Case Code
        /// </summary>
        /// <param name="oldCaseCode"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        [HttpGet("caseDetailsByCaseCode")]
        public async Task<IActionResult> GetCaseDetailsWithAllocationByCaseCode(string oldCaseCode, string loggedInUser = null)
        {
            var result =
                await _service.GetCaseDetailsWithAllocationByCaseCode(oldCaseCode, loggedInUser);
            return Ok(result);
        }

        /// <summary>
        ///     Get ongoing cases data along with their allocation filtered by selected criteria
        /// </summary>
        /// <param name="payload">Pass loggedInUser param if casePlanningViewNotes are required</param>
        /// <returns></returns>
        [HttpPost("ongoingCasesBySelectedValues")]
        public async Task<IActionResult> GetOnGoingCasesWithAllocationsBySelectedValues(dynamic payload)
        {
            var filterObj = payload != null && payload.ContainsKey("demandFilterCriteria")
                ? JsonConvert.DeserializeObject<DemandFilterCriteria>(payload["demandFilterCriteria"].ToString())
                : JsonConvert.DeserializeObject<DemandFilterCriteria>(payload.ToString());
            var loggedInUser = payload != null && payload.ContainsKey("loggedInUser") ? $"{payload["loggedInUser"]}" : null;

            var result =
                await _service.GetOnGoingCasesWithAllocationsBySelectedValues(filterObj, loggedInUser);
            return Ok(result);
        }

        /// <summary>
        /// Get opportunities and cases for typeahead
        /// </summary>
        /// <param name="searchString">Search opportunities and cases by its name, client Code or Client Name</param>
        /// <returns></returns>
        [HttpGet("projectTypeahead")]
        public async Task<IActionResult> GetProjectsForTypeahead(string searchString)
        {
            var cases = await _service.GetProjectsForTypeahead(searchString);
            return Ok(cases);
        }

        /// <summary>
        /// Get cases for typeahead
        /// </summary>
        /// <param name="searchString">Search cases by its name, client Code or Client Name</param>
        /// <returns></returns>
        [HttpGet("caseTypeahead")]
        public async Task<IActionResult> GetCasesForTypeahead(string searchString)
        {
            var cases = await _service.GetCasesForTypeahead(searchString);
            return Ok(cases);
        }

        /// <summary>
        /// Get planning card and its allocations
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="officeCodes"></param>
        /// <param name="staffingTags"></param>
        /// <param name="isStaffedFromSupply"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        [HttpGet("planningCardsByEmployeeAndFilters")]
        public async Task<IActionResult> GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(string employeeCode, string officeCodes, string staffingTags,
            bool isStaffedFromSupply = false, string loggedInUser = null)
        {
            var cases = await _service.GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(employeeCode, officeCodes, staffingTags, isStaffedFromSupply, loggedInUser);
            return Ok(cases);
        }

        /// <summary>
        /// Upsert Case View Note
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertCaseViewNote")]
        public async Task<IActionResult> UpsertCaseViewNote(dynamic payload)
        {
            CaseViewNote caseViewNote = JsonConvert.DeserializeObject<CaseViewNote>(payload.ToString());
            var securityUsersDetails = await _service.UpsertCaseViewNote(caseViewNote);
            return Ok(securityUsersDetails);
        }

        /// <summary>
        /// Get Notes created by employee or shared with employee for a single case or opp or planning card
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     {
        ///        "loggedInUser": "39209",
        ///        "oldCaseCode": null,
        ///        "pipelineId": null,
        ///        "planningCardId": "59a10495-9703-ee11-a9cc-005056accf10"
        ///    }
        /// </remarks>
        [HttpPost]
        [Route("getCaseViewNote")]
        public async Task<IActionResult> GetCaseViewNote(dynamic payload)
        {
            var oldCaseCode = payload != null && payload.ContainsKey("oldCaseCode") ? $"{payload["oldCaseCode"]}" : null;
            var pipelineId = payload != null && payload.ContainsKey("pipelineId") ? $"{payload["pipelineId"]}" : null;
            var planningCardId = payload != null && payload.ContainsKey("planningCardId") ? $"{payload["planningCardId"]}" : null;
            var loggedInUser = payload != null && payload.ContainsKey("loggedInUser") ? $"{payload["loggedInUser"]}" : null;

            var caseViewNotes = await _service.GetCaseViewNote(oldCaseCode, pipelineId, planningCardId, loggedInUser);
            return Ok(caseViewNotes);
        }

    }
}