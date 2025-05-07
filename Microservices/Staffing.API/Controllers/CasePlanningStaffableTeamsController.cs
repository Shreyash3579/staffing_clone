using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class CasePlanningStaffableTeamsController : ControllerBase
    {
        private readonly ICasePlanningStaffableTeamsService _service;
        public CasePlanningStaffableTeamsController(ICasePlanningStaffableTeamsService service)
        {
            _service = service;
        }
        
        /// <summary>
        /// Get Staffable Team data by comma separated values of officeCodes, startWeek, and endWeek
        /// Sample Request:
        /// {
        ///     "officeCodes": "110,115",
        ///     "startWeek": "2023-01-09",
        ///     "endWeek": "2023-02-13"
        /// }
        /// Only pass startWeek if you want to get staffable team for one week. Otherwise pass endWeek with startWeek only if you need the Staffable Teams for more than one week
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getCasePlanningBoardStaffableTeamsByOfficesAndDateRange")]
        public async Task<IActionResult> GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(dynamic payload)
        {
            var officeCodes = string.IsNullOrEmpty(payload["officeCodes"].ToString()) ? null : payload["officeCodes"].ToString();
            var startWeek = !string.IsNullOrEmpty(payload["startWeek"].ToString()) ? DateTime.Parse(payload["startWeek"].ToString()) : null;
            var endWeek = !string.IsNullOrEmpty(payload["endWeek"].ToString()) ? DateTime.Parse(payload["endWeek"].ToString()) : null;

            var staffableTeams = await _service.GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(officeCodes, startWeek, endWeek);
            return Ok(staffableTeams);
        }

        /// <summary>
        /// Used to insert/update case planning board staffable teams data
        /// </summary>
        /// <param name="staffableTeamsToUpsert">IEnumerable of staffableTeams model data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertCasePlanningBoardStaffableTeams")]
        public async Task<IActionResult> UpsertCasePlanningBoardStaffableTeams(IEnumerable<CasePlanningBoardStaffableTeams> staffableTeamsToUpsert)
        {
            var upsertedStaffableTeams = await _service.UpsertCasePlanningBoardStaffableTeams(staffableTeamsToUpsert);

            return Ok(upsertedStaffableTeams);
        }
    }
}
