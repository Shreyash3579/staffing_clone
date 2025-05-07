using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResourceAllocationAggregatorController : ControllerBase
    {
        private readonly IResourceAllocationService _resourceAllocationService;

        public ResourceAllocationAggregatorController(IResourceAllocationService resourceAllocationService)
        {
            _resourceAllocationService = resourceAllocationService;
        }

        /// <summary>
        /// Assign/Update resource allocation on case
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
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Route("upsertResourceAllocations")]
        public async Task<IActionResult> UpsertResourceAllocations(dynamic payload)
        {
            IEnumerable<ResourceAssignmentViewModel> resourceAllocations = JsonConvert.DeserializeObject<ResourceAssignmentViewModel[]>(payload.ToString());
            var result = await _resourceAllocationService.UpsertResourceAllocations(resourceAllocations);
            return Ok(result);
        }


        //commented on 06-jun-23 as it is not being used from UI anymore
        ///// <summary>
        ///// Get Resource allocations to cases on the basis of selected employees and Date Range
        ///// </summary>
        ///// <remarks>
        ///// Sample Request:
        ///// {
        /////     "employeeCodes" : "37995,42AKS,42624", 
        /////     "startDate": "2019-10-01,
        /////     "endDate": "2020-01-31"
        ///// }
        ///// </remarks>
        ///// <param name="payload"></param>
        //[HttpPost]
        //[Route("allocationsByEmployees")]
        //public async Task<IActionResult> GetResourceAllocationsByEmployeeCodes(dynamic payload)
        //{
        //    var employeeCodes = payload["employeeCodes"].ToString();
        //    var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
        //    var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
        //    var allocations = await _resourceAllocationService.GetResourceAllocationsByEmployeeCodes(employeeCodes, startDate, endDate);
        //    return Ok(allocations);
        //}

        /// <summary>
        /// Upserts case roll data and rolled allocations when a case is rolled or existing case rolls are updated
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "caseRolls" : [{
        ///         "rolledFromOldCaseCode": "K7FC", "rolledFromOldCaseCode": null, "currentCaseEndDate": "10-Jul-2020",
        ///         "expectedCaseEndDate": "30-Jul-2020", "lastUpdatedBy": "39209"
        ///     }], 
        ///     "resourceAllocations": [{},{}]
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        [HttpPost]
        [Route("upsertCaseRollsAndAllocations")]
        public async Task<IActionResult> UpsertCaseRollsAndAllocations(dynamic payload)
        {
            var caseRoll = JsonConvert.DeserializeObject<CaseRoll[]>(payload["caseRolls"].ToString());
            IEnumerable<ResourceAssignmentViewModel> resourceAllocations = JsonConvert.DeserializeObject<ResourceAssignmentViewModel[]>(payload["resourceAllocations"].ToString());

            var result = await _resourceAllocationService.UpsertCaseRollsAndAllocations(caseRoll, resourceAllocations);

            return Ok(result);
        }

        /// <summary>
        /// reverts the rolled allocations to their original case date and delete the case roll data
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "caseRoll" : {
        ///         "rolledFromOldCaseCode": "K7FC", "rolledFromOldCaseCode": null, "currentCaseEndDate": "10-Jul-2020",
        ///         "expectedCaseEndDate": "30-Jul-2020", "lastUpdatedBy": "39209"
        ///     }, 
        ///     "resourceAllocations": [{},{}]
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        [HttpPost]
        [Route("revertCaseRollsAndAllocations")]
        public async Task<IActionResult> RevertCaseRollsAndAllocations(dynamic payload)
        {
            var caseRoll = JsonConvert.DeserializeObject<CaseRoll>(payload["caseRoll"].ToString());
            IEnumerable<ResourceAssignmentViewModel> resourceAllocations = JsonConvert.DeserializeObject<ResourceAssignmentViewModel[]>(payload["resourceAllocations"].ToString());

            var result = await _resourceAllocationService.RevertCaseRollAndAllocations(caseRoll, resourceAllocations);

            return Ok(result);
        }

        /// <summary>
        /// To fetch which employees are working on a opportunity has a Role specified
        /// Mostly used to find partners working on case
        /// </summary>
        /// /// <remarks>
        /// Sample Request:
        /// {
        ///     "oldCaseCodes" : "K3NE,A2FJ"
        /// }
        /// </remarks>
        /// <param name="payload">comma separated list of oldCaseCodes</param>
        /// <returns></returns>
        [HttpPost]
        [Route("getCaseRoleAllocationsByOldCaseCodes")]
        public async Task<IActionResult> GetCaseRoleAllocationsByOldCaseCodes(dynamic payload)
        {
            string oldCaseCodes = payload["oldCaseCodes"].ToString();
            var result = await _resourceAllocationService.GetCaseRoleAllocationsByOldCaseCodes(oldCaseCodes);
            return Ok(result);
        }

        /// <summary>
        /// To fetch which employees are working on a opportunity has a Role specified
        /// Mostly used to find partners working on case
        /// </summary>
        /// /// <remarks>
        /// Sample Request:
        /// {
        ///     "pipelineIds" : "eaad4f85-1b87-4691-ace2-77cd81379b31"
        /// }
        /// </remarks>
        /// <param name="payload">comma separated list of pipelineIds</param>
        /// <returns></returns>
        [HttpPost]
        [Route("getCaseRoleAllocationsByPipelineIds")]
        public async Task<IActionResult> GetCaseRoleAllocationsByPipelineIds(dynamic payload)
        {
            string pipelineIds = payload["pipelineIds"].ToString();
            var result = await _resourceAllocationService.GetCaseRoleAllocationsByPipelineIds(pipelineIds);
            return Ok(result);
        }

        /// <summary>
        /// Get the team that the employee worked on the last case
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="employeeCode"></param>
        /// <param name="date">Optional</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getLastTeamByEmployeeCode")]
        public async Task<IActionResult> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date)
        {
            var result = await _resourceAllocationService.GetLastTeamByEmployeeCode(employeeCode, date);
            return Ok(result);
        }

        /// <summary>
        /// This API end point is used to fetch allocations for office closure based on filter values
        /// </summary>
        /// <remarks>
        /// Sample Request: {"officeCodes": "110","caseTypeCodes": "4","startDate": "2021-12-25","endDate": "2021-12-31"}
        /// </remarks>
        /// <param name="payload">comma separated list of officeCodes, caseTypeCodes and start date and end date</param>
        /// <returns></returns>
        [HttpPost]
        [Route("getAllocationsForOfficeClosure")]
        public async Task<IActionResult> GetAllocationsWithinDateRangeForOfficeClosure(dynamic payload)
        {
            var officeCodes = payload["officeCodes"].ToString();
            var caseTypeCodes = payload["caseTypeCodes"].ToString();
            var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
            var staffingTags = payload["staffingTags"].ToString();

            var result = await _resourceAllocationService.GetAllocationsWithinDateRangeForOfficeClosure(officeCodes, caseTypeCodes, startDate, endDate, staffingTags);
            return Ok(result);
        }

    }
}