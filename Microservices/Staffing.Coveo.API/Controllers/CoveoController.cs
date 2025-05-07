using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.Coveo.API.Contracts.Services;
using Staffing.Coveo.API.Models;
using Staffing.Coveo.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Coveo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoveoController : ControllerBase
    {
        public readonly ICoveoService _coveoService;
        public CoveoController(ICoveoService coveoService)
        {
            _coveoService = coveoService;
        }

        /// <summary>
        /// This method gets the data from Coveo based on the source and searchTerm passed by user as params
        /// </summary>
        /// <param name="searchTerm">Pass search term like e-code or old case code here. For Example: Molly</param>
        /// <param name="source">Pass name of the source here.
        /// For Example: resource or project.
        /// In case nothing is mentioned, everthing will be searched</param>
        /// <param name="userDisplayName">Name of user who is searching</param>
        /// <param name="username">Email Id of user who is searching</param>
        /// <returns></returns>
        [HttpGet]
        [Route("searchByQuery")]
        public async Task<IActionResult> SearchByQuery(string searchTerm, string source, string userDisplayName, string username)
        {
            var userIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var searchData = await _coveoService.SearchByQuery(searchTerm, source, userDisplayName, username, userIPAddress);
            return Ok(searchData);
        }

        /// <summary>
        /// This method gets the data from Coveo based on the source and searchTerm passed by user as params
        /// </summary>
        /// <param name="searchTerm">Pass search term like e-code or old case code here. For Example: Molly</param>
        /// <param name="source">Pass name of the source here.
        /// For Example: resource or project.
        /// In case nothing is mentioned, everthing will be searched</param>
        /// <param name="userDisplayName">Name of user who is searching</param>
        /// <param name="username">Email Id of user who is searching</param>
        /// <returns></returns>
        [HttpGet]
        [Route("searchByQueryTest")]
        public async Task<IActionResult> SearchByQueryTest(string searchTerm, string source, string userDisplayName, string username, bool? test = true)
        {
            var userIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var searchData = await _coveoService.SearchByQuery(searchTerm, source, userDisplayName, username, userIPAddress, test);
            return Ok(searchData);
        }

        /// <summary>
        /// This method is used to upsert or delete the allocations indexes in Coveo Cloud Platform using Push APIs.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>The allocations that were successfully indexed in coveo cloud platform.
        /// If return Empty object then indexing failed.</returns>
        [HttpPost]
        [Route("upsertOrDeleteAllocationIndexes")]
        public async Task<IActionResult> UpsertOrDeleteAllocationIndexes(dynamic payload)
        {
            IEnumerable<ResourceAllocation> allocations = JsonConvert.DeserializeObject<ResourceAllocation[]>(payload.ToString());
            var searchData = await _coveoService.UpsertOrDeleteAllocationIndexes(allocations);
            return Ok(searchData);
        }

        /// <summary>
        /// Log click event unsing this API end point
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("logClickEvent")]
        public async Task<IActionResult> LogClickEventInCoveoAnalytics(dynamic payload)
        {
            var userIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            AnalyticsClickViewModel analyticsClickParams = JsonConvert.DeserializeObject<AnalyticsClickViewModel>(payload.ToString());

            var loggedData = await _coveoService.LogClickEventInCoveoAnalytics(analyticsClickParams, userIPAddress);
            return Ok(loggedData);
        }

    }
}
