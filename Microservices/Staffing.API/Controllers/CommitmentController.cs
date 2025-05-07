using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.CommitmentRead)]
    [EnableQuery]
    public class CommitmentController : ControllerBase
    {
        private readonly ICommitmentService _commitmentService;

        public CommitmentController(ICommitmentService commitmentService)
        {
            _commitmentService = commitmentService;
        }

        /// <summary>
        ///     Get commitments for lookup
        /// </summary>
        [HttpGet("lookup")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> GetCommitmentTypeLookupList(bool? showHidden)
        {
            var commitmentTypes = await _commitmentService.GetCommitmentTypeLookupList(showHidden);
            return Ok(commitmentTypes);
        }

        /// <summary>
        ///     Get Resource Commitments
        /// </summary>
        [HttpGet("commitmentsByEmployee")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> GetResourceCommitments(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var commitments = await _commitmentService.GetResourceCommitments(employeeCode, effectiveFromDate, effectiveToDate);
            return Ok(commitments);
        }

        /// <summary>
        ///     Get Resource Commitments By Ids
        ///    Input: comma spearated list of commitment ids
        /// </summary>
        /// <remarks>
        ///     Sample Request:
        ///     "abc, xyz, pqrs"
        /// </remarks>
        [HttpPost("commitmentByIds")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> GetResourceCommitmentsByIds([FromBody] string commitmentIds)
        {
            var commitments = await _commitmentService.GetResourceCommitmentsByIds(commitmentIds);
            return Ok(commitments);
        }

        /// <summary>
        ///     Get Resource Commitments before it was deleted By Ids
        ///    Input: comma spearated list of commitment ids
        /// </summary>
        /// <remarks>
        ///     Sample Request:
        ///     "abc, xyz, pqrs"
        /// </remarks>
        [HttpPost("commitmentByDeletedIds")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> GetResourceCommitmentsByDeletedIds([FromBody] string commitmentIds)
        {
            var commitments = await _commitmentService.GetResourceCommitmentsByDeletedIds(commitmentIds);
            return Ok(commitments);
        }

        [HttpPost("upsertRingfenceCommitmentAlerts")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> UpsertRingfenceCommitmentAlerts(CommitmentEnrichment commitmentDetails)
        {
            var upsertedCommitmentAlers = await _commitmentService.UpsertRingfenceCommitmentAlerts(commitmentDetails);
            return Ok(upsertedCommitmentAlers);
        }

        /// <summary>
        ///     Get Commitments by commitment type saved in staffing For employees between specified date range.
        ///     Pass null in dates for getting all future commitments. Pass null in commitmentTypeCode for getting all commitments
        /// </summary>
        /// <remarks>
        ///     Sample Request:
        ///     {"employeeCodes":"09PTS,31JWE","startDate":"2019-07-12 00:00:00","endDate":"2019-09-15 00:00:00",
        ///     "commitmentTypeCode":"P"}
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("commitmentsWithinDateRangeByEmployees")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> GetResourceCommitmentsWithinDateRangeByEmployees(dynamic payload)
        {
            var employeeCodes = payload["employeeCodes"].ToString();
            var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString())
                ? DateTime.Parse(payload["startDate"].ToString())
                : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString())
                ? DateTime.Parse(payload["endDate"].ToString())
                : null;
            var commitmentTypeCode = !string.IsNullOrEmpty(payload["commitmentTypeCode"].ToString())
                ? payload["commitmentTypeCode"].ToString()
                : null;

            var commitments =
                await _commitmentService.GetResourceCommitmentsWithinDateRangeByEmployees(employeeCodes, startDate,
                    endDate, commitmentTypeCode);
            return Ok(commitments);
        }

        /// <summary>
        ///     Get commitments within date range specified for supplied commitments
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet("commitmentsWithinDateRange")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> GetCommitmentsWithinDateRange(DateTime startDate, DateTime endDate)
        {
            var commitments =
                await _commitmentService.GetCommitmentsWithinDateRange(startDate, endDate);
            return Ok(commitments);
        }

        /// <summary>
        ///     Delete Resource Commitment
        /// </summary>
        [HttpDelete]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> DeleteResourceCommitmentById(Guid commitmentId, string lastUpdatedBy)
        {
            await _commitmentService.DeleteResourceCommitmentById(commitmentId, lastUpdatedBy);
            return Ok();
        }

        /// <summary>
        /// Delete commitment(s) by Commitment Id(s)
        /// </summary>
        /// <param name="commitmentIds">comma separated list of commitment Ids</param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("deleteCommitmentsByIds")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> DeleteResourceCommitmentByIds(string commitmentIds, string lastUpdatedBy)
        {
            await _commitmentService.DeleteResourceCommitmentByIds(commitmentIds, lastUpdatedBy);
            return Ok();
        }

        /// <summary>
        /// Gets commitment data on the basis of the parameters sent
        /// </summary>
        /// <remarks>
        ///     Sample Request:
        ///     {
        ///     "commitmentTypeCodes": "L",
        ///     "employeeCodes": "07GRP",
        ///     "startDate": "07-Jul-2019",
        ///     "endDate": "10-Jul-2019",
        ///     "ringfenceCommitmentsOnly": null
        ///     }
        /// </remarks>
        /// <param name="payload">Json representing Resources Commitments</param>
        /// <returns></returns>
        [HttpPost]
        [Route("commitmentBySelectedValues")]
        [ProducesResponseType(typeof(CommitmentViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCommitmentBySelectedValues(dynamic payload)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var clientId = identity?.Name;

            var commitmentTypeCodes = !string.IsNullOrEmpty(payload["commitmentTypeCodes"]?.ToString()) ? payload["commitmentTypeCodes"].ToString() : null;
            var employeeCodes = !string.IsNullOrEmpty(payload["employeeCodes"]?.ToString()) ? payload["employeeCodes"].ToString() : null;
            var startDate = !string.IsNullOrEmpty(payload["startDate"]?.ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"]?.ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
            var ringfenceCommitmentsOnly = !string.IsNullOrEmpty(payload["ringfenceCommitmentsOnly"]?.ToString())
                ? Convert.ToBoolean(payload["ringfenceCommitmentsOnly"].ToString())
                : null;

            var response = await _commitmentService.GetCommitmentBySelectedValues(commitmentTypeCodes,
                 employeeCodes, startDate, endDate, ringfenceCommitmentsOnly, clientId);

            // NOTE: Since we are using tuple as return type, here Item1 and Item2 represent "errorMessage" and "commitment data" respectively
            if (string.IsNullOrEmpty(response.Item1))
            {
                return Ok(response.Item2);
            }

            return Ok(response.Item1);
        }

        /// <summary>
        /// Gets employee ringfence data on the basis of the parameters sent
        /// </summary>
        /// <remarks>
        ///     Sample Request:
        ///     {
        ///     "employeeCodes": "07GRP",
        ///     "startDate": "07-Jan-2025",
        ///     "endDate": "10-Jul-2025",
        ///     }
        /// </remarks>
        /// <param name="payload">Json representing Resources Commitments</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ringfenceCommitmentBySelectedValues")]
        [ProducesResponseType(typeof(CommitmentViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRingfenceCommitmentBySelectedValues(dynamic payload)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var clientId = identity?.Name;

            string commitmentTypeCodes = !string.IsNullOrEmpty(payload["commitmentTypeCodes"]?.ToString()) ? payload["commitmentTypeCodes"].ToString() : null; ;
            var employeeCodes = !string.IsNullOrEmpty(payload["employeeCodes"]?.ToString()) ? payload["employeeCodes"].ToString() : null;
            var startDate = !string.IsNullOrEmpty(payload["startDate"]?.ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"]?.ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
            var ringfenceCommitmentsOnly = true;

            var response = await _commitmentService.GetCommitmentBySelectedValues(commitmentTypeCodes,
                 employeeCodes, startDate, endDate, ringfenceCommitmentsOnly, clientId);

            // NOTE: Since we are using tuple as return type, here Item1 and Item2 represent "errorMessage" and "commitment data" respectively
            if (string.IsNullOrEmpty(response.Item1))
            {
                return Ok(response.Item2);
            }

            return Ok(response.Item1);
        }

        /// <summary>
        ///     Insert Resources Commitments
        /// </summary>
        /// <remarks>
        ///     Sample Request:
        ///     {
        ///     "employeeCode": "07GRP",
        ///     "commitmentType": {
        ///     "commitmentTypeCode": "L",
        ///     "commitmentTypeName": "Leave",
        ///     "precedence": 2
        ///     },
        ///     "startDate": "07-Jul-2019",
        ///     "endDate": "10-Jul-2019",
        ///     "notes": "Testing",
        ///     "lastUpdatedBy": "37995"
        ///     },
        ///     {
        ///     "employeeCode": "39209",
        ///     "commitmentType": {
        ///     "commitmentTypeCode": "T",
        ///     "commitmentTypeName": "Training",
        ///     "precedence": 2
        ///     },
        ///     "startDate": "07-Jul-2019",
        ///     "endDate": "10-Jul-2019",
        ///     "notes": "Testing",
        ///     "lastUpdatedBy": "37995"
        ///     }
        /// </remarks>
        /// <param name="payload">Json representing Resources Commitments</param>
        /// <response code="201">Returns saved commitments</response>
        /// <response code="400">If Payload is null or wrongly formatted</response>
        [HttpPost]
        [Route("resourcesCommitments")]
        [ProducesResponseType(typeof(CommitmentViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpsertResourcesCommitments(dynamic payload)
        {
            var resourcesCommitments = JsonConvert.DeserializeObject<IList<Commitment>>(payload.ToString());
            var result = await _commitmentService.UpsertResourcesCommitments(resourcesCommitments);
            return Ok(result);
        }



        [HttpPost]
        [Route("upsertCommitmentTypes")]
        [ProducesResponseType(typeof(CommitmentType), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpsertCommitmentTypes(dynamic payload)
        {
            var practiceCommitment = JsonConvert.DeserializeObject<CommitmentType>(payload.ToString());
            var result = await _commitmentService.UpsertCommitmentTypes(practiceCommitment);
            return Ok(result);
        }

        /// <summary>
        ///     Check PEG Ringfence Allocations and Upsert Down Day Commitments after Case Allocation End Date.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("checkPegRingfenceAllocationAndInsertDownDayCommitments")]
        public async Task<IActionResult> CheckPegRingfenceAllocationAndInsertDownDayCommitments(dynamic payload)
        {
            var resourceallocations = !string.IsNullOrEmpty(payload["resourceAllocations"].ToString()) ? JsonConvert.DeserializeObject<IList<ResourceAllocation>>(payload["resourceAllocations"].ToString()) : null;
            var insertedDownDayCommitments = await _commitmentService.checkPegRingfenceAllocationAndInsertDownDayCommitments(resourceallocations);
            return Ok(insertedDownDayCommitments);
        }

        /// <summary>
        ///     Returns IsSTACommitmentCreated based on oldCaseCode, opportunityId or planningCardId
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("isSTACommitmentCreated")]
        public async Task<IActionResult> IsSTACommitmentCreated(dynamic payload)
        {
            var oldCaseCode = !string.IsNullOrEmpty(payload["oldCaseCode"]?.ToString()) ? payload["oldCaseCode"].ToString() : null;
            var opportunityId = !string.IsNullOrEmpty(payload["opportunityId"]?.ToString()) ? Guid.Parse(payload["opportunityId"].ToString()) : Guid.Empty;
            var planningCardId = !string.IsNullOrEmpty(payload["planningCardId"]?.ToString()) ? Guid.Parse(payload["planningCardId"].ToString()) : Guid.Empty;

            var isSTACommitmentCreated = await _commitmentService.IsSTACommitmentCreated(oldCaseCode, opportunityId, planningCardId);
            return Ok(isSTACommitmentCreated);
        }

        /// <summary>
        ///     Returns ProjectSTACommitmentDetails based on oldCaseCode, opportunityId or planningCardId
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("getProjectSTACommitmentDetails")]
        public async Task<IActionResult> GetProjectSTACommitmentDetails(dynamic payload)
        {

            var oldCaseCodes = !string.IsNullOrEmpty(payload["oldCaseCode"]?.ToString()) ? payload["oldCaseCode"].ToString() : null;

            var opportunityIds = !string.IsNullOrEmpty(payload["opportunityIds"]?.ToString())
                ? payload["opportunityIds"].ToString()
                : null;

            var planningCardIds = !string.IsNullOrEmpty(payload["planningCardIds"]?.ToString())
                ? payload["planningCardIds"].ToString()
                : null;


            var projectSTACommitmentDetails = await _commitmentService.GetProjectSTACommitmentDetails(oldCaseCodes, opportunityIds, planningCardIds);
            return Ok(projectSTACommitmentDetails);
        }

        [HttpPost]
        [Route("upsertCaseOppCommitments")]
        [ProducesResponseType(typeof(CommitmentViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> InsertCaseOppCommitments(dynamic payload)
        {
            var items = JsonConvert.DeserializeObject<IEnumerable<InsertCaseOppCommitmentViewModel>>(payload.ToString());

            var result  = await _commitmentService.UpsertCaseOppCommitments(items);

            return Ok(result);
        }

        /// <summary>
        /// Delete CaseOppcCommitment(s) by Commitment Id(s)
        /// </summary>
        /// <param name="commitmentIds">comma separated list of commitment Ids</param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete("deleteCaseOppCommitments")]
        public async Task<IActionResult> DeleteCaseOppCommitments([FromBody] CommitmentBatch request)
        {
            await _commitmentService.DeleteCaseOppCommitments(request.CommitmentIds, request.LastUpdatedBy);
            return Ok();
        }
    }
}