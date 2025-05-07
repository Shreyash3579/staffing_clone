//using Microsoft.AspNet.OData;

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
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Produces("application/json")]
    [Route("api/resourceAllocation")]
    [ApiController]
    [EnableQuery]
    [Authorize(Policy = Constants.Policy.ResourceAllocationRead)]
    public class ResourceAllocationController : ControllerBase
    {

        private readonly IResourceAllocationService _resourceAllocationService;

        public ResourceAllocationController(IResourceAllocationService resourceAllocationService)
        {
            _resourceAllocationService = resourceAllocationService;
        }

        /// <summary>
        /// Get resources allocations filtered by selected values
        /// </summary>
        /// <param name="payload">One Parameter needs to pass</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "oldCaseCodes":"Q6BK",
        ///       "employeeCodes":"39980",
        ///       "lastUpdated":"2020-04-09",
        ///       "startDate":"2020-04-01",
        ///       "endDate":"2020-04-17",
        ///       "caseRoleCodes": null
        ///    }
        /// </remarks>
        [HttpPost]
        [Route("allocationsBySelectedValues")]
        [ProducesResponseType(typeof(ResourceAllocationViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResourceAllocationsBySelectedValues(dynamic payload)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var clientId = identity?.Name;
            var oldCaseCodes = !string.IsNullOrEmpty(payload["oldCaseCodes"]?.ToString()) ? payload["oldCaseCodes"].ToString() : null;
            var employeeCodes = !string.IsNullOrEmpty(payload["employeeCodes"]?.ToString()) ? payload["employeeCodes"].ToString() : null;
            var lastUpdated = !string.IsNullOrEmpty(payload["lastUpdated"]?.ToString()) ? DateTime.Parse(payload["lastUpdated"].ToString()) : null;
            var startDate = !string.IsNullOrEmpty(payload["startDate"]?.ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"]?.ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
            var caseRoleCodes = !string.IsNullOrEmpty(payload["caseRoleCodes"]?.ToString()) ? payload["caseRoleCodes"].ToString() : null;

            var response = await _resourceAllocationService.GetResourceAllocationsBySelectedValues(oldCaseCodes,
            employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes, clientId);

            // NOTE: Since we are using tuple as return type, here Item1 and Item2 represent "errorMessage" and "allocation's data" respectively
            if (string.IsNullOrEmpty(response.Item1))
            {
                return Ok(response.Item2);
            }

            return Ok(response.Item1);
        }

        /// <summary>
        /// Get Allocations by Ids
        /// </summary>
        /// <param name="scheduleIds">Comma separated Schedule Ids</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("allocationsByScheduleIds")]
        public async Task<IActionResult> GetResourceAllocationsByScheduleIds([FromBody] string scheduleIds)
        {
            var allocations = await _resourceAllocationService.GetResourceAllocationsByScheduleIds(scheduleIds);
            return Ok(allocations);
        }

        /// <summary>
        /// Get Resource Allocations to selected opportunities
        /// </summary>
        /// <param name="payload">comma separated list of pipeline Id</param>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("allocationsByPipelineIds")]
        public async Task<IActionResult> GetResourceAllocationsByPipelineIds(dynamic payload)
        {
            var pipelineIds = $"{payload["pipelineIds"]}";
            var allocations = await _resourceAllocationService.GetResourceAllocationsByPipelineIds(pipelineIds);
            return Ok(allocations);
        }

        /// <summary>
        /// Get Resource Allocations to a single Opportunity
        /// </summary>
        /// <param name="pipelineId"></param>
        [HttpGet]
        [Authorize(Policy = Constants.Policy.ResourceAllocationRead)]
        [Route("allocationsByPipelineId")]
        public async Task<IActionResult> GetResourceAllocationsByPipelineId(string pipelineId)
        {
            var allocations = await _resourceAllocationService.GetResourceAllocationsByPipelineIds(pipelineId);
            return Ok(allocations);
        }


        /// <summary>
        /// Get Resource Allocations to selected cases
        /// </summary>
        /// <param name="payload">comma separated list of old case code</param>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("allocationsByCaseCodes")]
        public async Task<IActionResult> GetResourceAllocationsByCaseCodes(dynamic payload)
        {
            var oldCaseCodes = $"{payload["oldCaseCodes"]}";
            var allocations = await _resourceAllocationService.GetResourceAllocationsByCaseCodes(oldCaseCodes);
            return Ok(allocations);
        }

        /// <summary>
        /// Get Resource Allocations to selected cases
        /// </summary>
        /// <param name="payload">comma separated list of old case code</param>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("allocationsCountByCaseCodes")]
        public async Task<IActionResult> GetResourceAllocationsCountByCaseCodes(dynamic payload)
        {
            var oldCaseCodes = $"{payload["oldCaseCodes"]}";
            var allocations = await _resourceAllocationService.GetResourceAllocationsCountByCaseCodes(oldCaseCodes);
            return Ok(allocations);
        }

        /// <summary>
        /// Get Resource Allocations to specific case
        /// </summary>
        /// <param name="oldCaseCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = Constants.Policy.ResourceAllocationRead)]
        [Route("allocationsByCaseCode")]
        public async Task<IActionResult> GetResourceAllocationsByCaseCode(string oldCaseCode)
        {
            var allocations = await _resourceAllocationService.GetResourceAllocationsByCaseCodes(oldCaseCode);
            return Ok(allocations);
        }

        //commented on 06-jun-23 as it is not being used anymore
        ///// <summary>
        ///// Get Resource allocations to cases on the basis of selected offices and date range
        ///// </summary>
        ///// <param name="officeCodes"></param>
        ///// <param name="startDate"></param>
        ///// <param name="endDate"></param>
        //[HttpGet]
        //[Authorize(Policy = Constants.Policy.StaffingUserOnly)]
        //[Route("allocationsByOffices")]
        //public async Task<IActionResult> GetResourceAllocationsByOfficeCodes(string officeCodes, DateTime startDate, DateTime endDate)
        //{
        //    var allocations = await _resourceAllocationService.GetResourceAllocationsByOfficeCodes(officeCodes, startDate, endDate);
        //    return Ok(allocations);
        //}

        /// <summary>
        /// Get Resource allocations to cases on the basis of selected employees and Date Range
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
        [Route("allocationsByEmployees")]
        public async Task<IActionResult> GetResourceAllocationsByEmployeeCodes(dynamic payload)
        {
            var employeeCodes = payload["employeeCodes"].ToString();
            var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
            var allocations = await _resourceAllocationService.GetResourceAllocationsByEmployeeCodes(employeeCodes, startDate, endDate);
            return Ok(allocations);
        }

        /// <summary>
        /// Assign Employee to case
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// [
        ///    {
        ///       "oldCaseCode":"D6GR",
        ///       "caseCode":2,
        ///       "caseName":"ADAPT-Unilodgers",
        ///       "clientCode":31021,
        ///       "clientName":"Unilodgers",
        ///       "caseTypeCode": 1,
        ///       "caseTypeName": "Billable",
        ///       "pipelineId":null,
        ///       "opportunityName":null,
        ///       "employeeCode":"47910",
        ///       "employeeName":"Agarwal, Ayushya",
        ///       "fte":1,
        ///       "serviceLineName":"Traditional Consulting",
        ///       "position":"Associate Consultant",
        ///       "currentLevelGrade":"A1",
        ///       "operatingOfficeCode":334,
        ///       "operatingOfficeName":"Mumbai",
        ///       "operatingOfficeAbbreviation":"MUB",
        ///       "managingOfficeCode":334,
        ///       "managingOfficeName":"Mumbai",
        ///       "managingOfficeAbbreviation":"MUB",
        ///       "billingOfficeCode":334,
        ///       "billingOfficeName":"Mumbai",
        ///       "billingOfficeAbbreviation":"MUB",
        ///       "allocation":100,
        ///       "startDate":"12/5/2019",
        ///       "endDate":"22-Jan-2020",
        ///       "investmentCode":null,
        ///       "investmentName":null,
        ///       "caseRoleCode":null,
        ///       "caseRoleName":null,
        ///       "lastUpdatedBy":"45088"
        ///    }
        /// ]
        /// </remarks>
        /// <returns>Assigned resource(s) to case</returns>
        /// <param name="payload">Json representing one or more Resource Allocation</param>
        /// <response code="201">Returns Added and Updated resource(s) to case</response>
        /// <response code="400">If Payload is null or wrongly formatted</response>
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Route("upsertResourceAllocations")]
        public async Task<IActionResult> UpsertResourceAllocations(dynamic payload)
        {
            IEnumerable<ResourceAllocation> resourceAllocation = JsonConvert.DeserializeObject<ResourceAllocation[]>(payload.ToString());
            var result = await _resourceAllocationService.UpsertResourceAllocations(resourceAllocation);
            return Ok(result);
        }

        /// <summary>
        /// Delete Resource Allocation by schedule id
        /// </summary>
        /// <param name="allocationId"></param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> DeleteResourceAllocationById(Guid allocationId, string lastUpdatedBy)
        {
            await _resourceAllocationService.DeleteResourceAllocationById(allocationId, lastUpdatedBy);
            return Ok();
        }

        /// <summary>
        /// Delete allocation by Schedule Ids
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deleteAllocationsByIds")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> DeleteResourceAllocationByIds(dynamic payload)
        {
            var allocationIds = payload["allocationIds"]?.ToString();
            var lastUpdatedBy = payload["lastUpdatedBy"]?.ToString();

            await _resourceAllocationService.DeleteResourceAllocationByIds(allocationIds, lastUpdatedBy);
            return Ok();
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        [Route("allocationsOnCaseRollByCaseCodes")]
        public async Task<IActionResult> GetResourceAllocationsOnCaseRollByCaseCodes(dynamic payload)
        {
            var listOldCaseCodes = $"{payload["listOldCaseCodes"]}";
            var result = await _resourceAllocationService.GetResourceAllocationsOnCaseRollByCaseCodes(listOldCaseCodes);

            return Ok(result);
        }

        /// <summary>
        /// Gets all allocations on prepost having end date more that last 2 months
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("allocationsForEmployeesOnPrePost")]
        public async Task<IActionResult> GetAllocationsForEmployeesOnPrePost()
        {
            var result = await _resourceAllocationService.GetAllocationsForEmployeesOnPrePost();

            return Ok(result);
        }

        /// <summary>
        /// Get resources allocations filtered by selected supply side values
        /// </summary>
        /// <param name="payload">Parameter needs to pass</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "startDate" : '2022-09-01',
        ///       "endDate": '2023-10-01',
        ///       "officeCodes": null,
        ///       "staffingTags": null,
        ///       "currentLevelGrades": ""
        ///    }
        /// </remarks>
        [HttpPost]
        [Route("allocationsBySelectedSupplyValues")]
        [ProducesResponseType(typeof(ResourceAllocationViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResourceAllocationsBySelectedSupplyValues(dynamic payload)
        {
            var startDate = !string.IsNullOrEmpty(payload["startDate"]?.ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"]?.ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
            var officeCodes = !string.IsNullOrEmpty(payload["officeCodes"]?.ToString()) ? payload["officeCodes"].ToString() : null;
            var staffingTags = !string.IsNullOrEmpty(payload["staffingTags"]?.ToString()) ? payload["staffingTags"].ToString() : null;
            var currentLevelGrades = !string.IsNullOrEmpty(payload["currentLevelGrades"]?.ToString()) ? payload["currentLevelGrades"].ToString() : null;

            var allocations = await _resourceAllocationService.GetResourceAllocationsBySelectedSupplyValues(officeCodes, startDate, endDate,
                staffingTags, currentLevelGrades);
            return Ok(allocations);
        }

        /// <summary>
        /// Get the overlapping allocations in the last cases for an employee
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="date">Default value is today's date</param>
        /// <returns>Overlapping allocations with other team members from the previous cases the resource has worked on based on the end date of the previous allocation</returns>
        [HttpGet]
        [Route("lastTeamByEmployeeCode")]
        public async Task<IActionResult> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date)
        {
            var result = await _resourceAllocationService.GetLastTeamByEmployeeCode(employeeCode, date);

            return Ok(result);
        }

        /// <summary>
        /// Get the last billable date of employees in the last cases for an employee
        /// </summary>
        /// <param name="employeeCodes"></param>
        /// <param name="date">Default value is today's date</param>
        /// <returns>Overlapping allocations with other team members from the previous cases the resource has worked on based on the end date of the previous allocation</returns>
        [HttpPost]
        [Route("lastBillableDateByEmployeeCodes")]
        public async Task<IActionResult> GetLastBillableDateByEmployeeCodes(dynamic payload)
        {
            var employeeCodes = !string.IsNullOrEmpty(payload["employeeCodes"]?.ToString()) ? payload["employeeCodes"].ToString() : null;
            var date = !string.IsNullOrEmpty(payload["date"]?.ToString()) ? DateTime.Parse(payload["date"].ToString()) : null;

            var result = await _resourceAllocationService.GetLastBillableDateByEmployeeCodes(employeeCodes, date);

            return Ok(result);
        }
    }
}