using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CasePlanningMetricsController : Controller
    {
        private readonly ICasePlanningMetricsService _casePlanningMetricsService;

        public CasePlanningMetricsController(ICasePlanningMetricsService casePlanningMetricsService)
        {
            _casePlanningMetricsService = casePlanningMetricsService;
        }

        /// <summary>
        /// This API end point is used to fetch the case plannig supply metrics based on filter values
        /// </summary>
        /// <param name="payload"></param>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "startDate":"2022-11-14","endDate":"2022-12-23","officeCodes":"110",
        ///     "levelGrades":"A0,A1,A2,A3,A4,A5,A6,A7,M1,M2,M3,M4,M5,M6,M7,M8,M9","staffingTags":"SL0001",
        ///     "availabilityIncludes":"transition","groupBy":"office,levelGrade,dateFirstAvailable","sortBy":"dateFirstAvailable",
        ///     "affiliationRoleCodes":"","certificates":"","languages":"","practiceAreaCodes":"","employeeStatuses":"",
        ///     "positionCodes":"","staffableAsTypeCodes":""
        /// }
        /// </remarks>
        /// <returns>
        ///     1) Weekly aggregate of availablity for 6 weeks
        ///     2) Employee details with "First day available" within each week for all available employees
        /// </returns>
        [HttpPost("getAvailabilityMetrics")]
        public async Task<IActionResult> GetAvailabilityMetricsByFilterValues(dynamic payload)
        {
            SupplyFilterCriteria supplyFilterCriteria = JsonConvert.DeserializeObject<SupplyFilterCriteria>(payload.ToString());

            var resourcesBecomingAvailable = await _casePlanningMetricsService.GetAvailabilityMetricsByFilterValues(supplyFilterCriteria);
            return Ok(resourcesBecomingAvailable);
        }

        /// <summary>
        /// This API end point is used to fetch the case plannig supply metrics from cache for the playground session 
        /// Used to fetch metrics data when users join a playground session
        /// </summary>
        /// <param name="playgroundId">Id of the session</param>
        /// <returns>
        ///     1) Weekly aggregate of availablity for 6 weeks from playground cache
        ///     2) Employee details with "First day available" within each week for all available employees from playground cache
        /// </returns>
        [HttpGet("getAvailabilityMetricsForPlaygroundById")]
        public async Task<IActionResult> GetAvailabilityMetricsForPlaygroundById(string playgroundId)
        {
            var availabilityMetricsView = await _casePlanningMetricsService.GetAvailabilityMetricsForPlaygroundById(playgroundId);
            return Ok(availabilityMetricsView);
        }

        /// <summary>
        /// This API end point is used to recalculate the metrics in cache when allocations are upserted/deleted during playground session
        /// </summary>
        /// <param name="payload"></param>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "playgroundId":"","employeeCode":"39209","newStartDate":"2022-10-22","newEndDate": "2022-11-22", "newAllocation":100,
        ///     "oldStartDate":NULL,"oldEndDate": NULL, "oldAllocation": NULL, "isOpportunity": false,
        ///     "lastUpdatedBy":"60074"
        /// }
        /// </remarks>
        /// <returns>
        ///     1) Weekly aggregate of availablity for 6 weeks from playground cache
        ///     2) Employee details with "First day available" within each week for all available employees from playground cache
        /// </returns>
        [HttpPost("upsertAndGetCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations")]
        public async Task<IActionResult> UpsertAndGetCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(dynamic payload)
        {
            var playgroundId = string.IsNullOrEmpty(payload["playgroundId"].ToString())
                ? null
                : Guid.Parse(payload["playgroundId"].ToString());

            var playgroundAllocations = JsonConvert.DeserializeObject<IEnumerable<CasePlanningBoardPlaygroundAllocation>>(payload["playgroundAllocations"].ToString());

            var lastUpdatedBy = payload["lastUpdatedBy"].ToString();

            var availabilityMetricsView = await _casePlanningMetricsService.UpsertAndGetCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(playgroundId, playgroundAllocations, lastUpdatedBy);
            return Ok(availabilityMetricsView);
        }

        /// <summary>
        /// Delete Playground Cache data by Id. Deletes all the saved filters, cache data and the playground session
        /// </summary>
        /// <param name="playgroundId"></param>
        /// <param name="lastUpdatedBy"></param>
        /// <returns></returns>
        [HttpDelete("deleteCasePlanningBoardMetricsPlaygroundById")]
        public async Task<IActionResult> DeleteCasePlanningBoardMetricsPlaygroundById(Guid playgroundId, string lastUpdatedBy)
        {
            var deletedPlaygroundId = await _casePlanningMetricsService.DeleteCasePlanningBoardMetricsPlaygroundById(playgroundId, lastUpdatedBy);

            return Ok(deletedPlaygroundId);
        }

        /// <summary>
        /// This API end point is used to fetch the case plannig supply metrics Filters that were used to create the playground session
        /// Used when user joins a session
        /// </summary>
        /// <param name="playgroundId">Id of the session</param>
        /// <returns>Supply And Demand Filters that were used to create the playground session</returns>
        [HttpGet("getCasePlanningBoardPlaygroundFiltersByPlaygroundId")]
        public async Task<IActionResult> GetCasePlanningBoardPlaygroundFiltersByPlaygroundId(string playgroundId)
        {
            var filters = await _casePlanningMetricsService.GetCasePlanningBoardPlaygroundFiltersByPlaygroundId(playgroundId);
            return Ok(filters);
        }
    }
}
