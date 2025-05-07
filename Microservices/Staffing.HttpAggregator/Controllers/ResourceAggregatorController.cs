using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResourceAggregatorController : ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourceAggregatorController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        /// <summary>
        ///     Get Resources filtered based on selected values 
        /// </summary>
        /// <param name="payload"></param>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "startDate":"2020-7-6",
        ///       "endDate":"2020-8-3",
        ///       "officeCodes":"404",
        ///       "levelGrades":"",
        ///       "staffingTags":"SL0001"
        ///    }
        /// </remarks>
        /// <returns>List of Resources</returns>
        [HttpPost]
        [Route("resourcesFilteredBySelectedValues")]
        public async Task<IActionResult> GetResourcesFilteredBySelectedValues(dynamic payload)
        {
            var supplyFilterCriteria = payload != null && payload.ContainsKey("supplyFilterCriteria")
                ? JsonConvert.DeserializeObject<SupplyFilterCriteria>(payload["supplyFilterCriteria"].ToString())
                : JsonConvert.DeserializeObject<SupplyFilterCriteria>(payload.ToString());
            var loggedInUser = payload != null && payload.ContainsKey("loggedInUser") ? $"{payload["loggedInUser"]}" : null;
            var resources =
                await _resourceService.GetResourcesFilteredBySelectedValues(supplyFilterCriteria, loggedInUser);
            return Ok(resources);
        }

        /// <summary>
        ///     Get Resources filtered based on selected values 
        /// </summary>
        /// <param name="payload"></param>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "startDate":"2020-7-6",
        ///       "endDate":"2020-8-3",
        ///       "officeCodes":"404",
        ///       "levelGrades":"",
        ///       "staffingTags":"SL0001"
        ///    }
        /// </remarks>
        /// <returns>List of Resources</returns>
        [HttpPost]
        [Route("resourcesFilteredBySelectedGroupValues")]
        public async Task<IActionResult> GetResourcesFilteredBySelectedGroupValues(dynamic payload)
        {
            var supplyGroupFilterCriteria = payload != null && payload.ContainsKey("supplyGroupFilterCriteria")
                ? JsonConvert.DeserializeObject<SupplyGroupFilterCriteria>(payload["supplyGroupFilterCriteria"].ToString())
                : JsonConvert.DeserializeObject<SupplyGroupFilterCriteria>(payload.ToString());
            var loggedInUser = payload != null && payload.ContainsKey("loggedInUser") ? $"{payload["loggedInUser"]}" : null;
            var resources =
                await _resourceService.GetResourcesFilteredBySelectedGroupValues(supplyGroupFilterCriteria, loggedInUser);
            return Ok(resources);
        }

        /// <summary>
        /// Get Active Resources and its commitments by search string
        /// </summary>
        /// <param searchString="string">searches by firtname, lastname and fullname</param>
        /// <param addTransfers="bool">TRUE: adds row if resources has transfer, faslse: does not add transfer row</param>
        /// <returns></returns>
        [HttpGet]
        [Route("resourcesBySearchString")]
        public async Task<IActionResult> GetResourcesAllocationsAndCommitmentsBySearchString(string searchString, bool? addTransfers = false)
        {
            var resources =
                await _resourceService.GetResourcesAllocationsAndCommitmentsBySearchString(searchString, addTransfers);
            return Ok(resources);
        }

        /// <summary>
        /// Get Resources (active + terminated) and its commitments by search string
        /// </summary>
        /// <param searchString="string">searches by firtname, lastname and fullname</param>
        /// <param addTransfers="bool">TRUE: adds row if resources has transfer, faslse: does not add transfer row</param>
        /// <returns></returns>
        [HttpGet]
        [Route("resourcesIncludingTerminatedBySearchString")]
        public async Task<IActionResult> GetResourcesIncludingTerminatedAllocationsAndCommitmentsBySearchString(string searchString, bool? addTransfers = false)
        {
            var resources =
                await _resourceService.GetResourcesIncludingTerminatedAllocationsAndCommitmentsBySearchString(searchString, addTransfers);
            return Ok(resources);
        }

        /// <summary>
        ///  Gets all resource commitments within the date range
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        /// 
        /// {
        ///     "startDate":"2023-06-01T15:41:18.009Z",
        ///     "endDate":"2023-06-15T15:41:18.009Z",
        ///     "serviceLineCodes":"P",
        ///     "officeCodes":"210,512,215,536"
        /// }
        /// </remarks>
        [HttpPost]
        [Route("resourcescommitmentsWithinDateRange")]
        public async Task<IActionResult> GetResourcesAllocationsAndCommitmentsWithinDateRange(dynamic payload)
        {
            var employeeCodes = payload["employeeCodes"].ToString();
            var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString())
                ? DateTime.Parse(payload["startDate"].ToString())
                : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString())
                ? DateTime.Parse(payload["endDate"].ToString())
                : null;
            var commitmentTypes = payload["commitmentTypes"].ToString();

            var resources =
                await _resourceService.GetResourcesAllocationsAndCommitmentsWithinDateRange(employeeCodes, startDate, endDate, commitmentTypes);

            return Ok(resources);
        }

        /// <summary>
        /// Get HCPD advisor for employee
        /// </summary>
        /// <param employeeCode="string"></param>
        /// <returns>{"fullName": null} when employee does not have any advisor</returns>
        [HttpGet]
        [Route("advisorByEmployeeCode")]
        public async Task<IActionResult> GetAdvisorByEmployeeCode(string employeeCode)
        {
            var resource =
                await _resourceService.GetAdvisorNameByEmployeeCode(employeeCode);
            return Ok(resource);
        }
    }
}