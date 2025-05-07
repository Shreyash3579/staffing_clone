using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.AnalyticsService;
using Staffing.Analytics.API.Models;

namespace Staffing.Analytics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaceholderAnalyticsController : ControllerBase
    {
        private readonly IPlaceholderAnalyticsService _placeholderAnalyticsService;
        public PlaceholderAnalyticsController(IPlaceholderAnalyticsService placeholderAnalyticsService)
        {
            _placeholderAnalyticsService = placeholderAnalyticsService;
        }

        [HttpPost]
        [Route("placeholderAnalyticsReport")]
        public async Task<IActionResult> CreatePlaceholderAnalyticsReport([FromBody] string scheduleMasterPlaceholderIds)
        {
            var result = await _placeholderAnalyticsService.CreatePlaceholderAnalyticsReport(scheduleMasterPlaceholderIds);
            return Ok(result);
        }

        [HttpDelete]
        [Route("deletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds")]
        public async Task<IActionResult> DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(string scheduleMasterPlaceholderIds)
        {
            await _placeholderAnalyticsService.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(scheduleMasterPlaceholderIds);
            return Ok();
        }

        [HttpPost]
        [Route("correctPlaceholderAnalyticsData")]
        public async Task<IActionResult> CorrectPlaceholderAnalyticsData()
        {
            var result = await _placeholderAnalyticsService.CorrectPlaceholderAnalyticsData();
            return Ok(result);
        }
    }
}
