//using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Helpers;
using Staffing.API.ViewModels;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Staffing.API.Controllers
{
    [Produces("application/json")]
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/resourceAllocation")]
    [ApiController]
    [EnableQuery]
    [Authorize(Policy = Constants.Policy.ResourceAllocationRead)]
    public class ResourceAllocationV3Controller : ControllerBase
    {

        private readonly IResourceAllocationService _resourceAllocationService;

        public ResourceAllocationV3Controller(IResourceAllocationService resourceAllocationService)
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
        ///       "caseRoleCodes": null,
        ///       "action": "upserted" or "deleted"
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
            var action = !string.IsNullOrEmpty(payload["action"]?.ToString()) ? payload["action"].ToString() : null;

            var response = await _resourceAllocationService.GetResourceAllocationsBySelectedValues(action, oldCaseCodes,
            employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes, clientId);

            // NOTE: Since we are using tuple as return type, here Item1 and Item2 represent "errorMessage" and "allocation's data" respectively
            if (string.IsNullOrEmpty(response.Item1))
            {
                return Ok(response.Item2);
            }

            return Ok(response.Item1);
        }
    }
}
