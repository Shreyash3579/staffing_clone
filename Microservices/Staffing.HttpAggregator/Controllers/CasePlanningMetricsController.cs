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
    public class CasePlanningMetricsController : ControllerBase
    {
        private readonly ICasePlanningMetricsService _casePlanningMetricsService;

        public CasePlanningMetricsController(ICasePlanningMetricsService casePlanningMetricsService)
        {
            _casePlanningMetricsService = casePlanningMetricsService;
        }

        //todo: delete this since it's not being used now and thsi call has been splitted into ttwo - getCasePlanningBoardColumnsData & getCasePlanningBoardNewDemandsData
        //[HttpPost("getCasePlanningBoardData")]
        //public async Task<IActionResult> GetCasePlanningBoardData([FromBody] dynamic payload)
        //{
        //    var demandFilterCriteria = JsonConvert.DeserializeObject<DemandFilterCriteria>(payload["demandFilterCriteria"].ToString());
        //    var employeeCode = payload["employeeCode"].ToString();

        //    var casePlanningBoardsData = await _casePlanningMetricsService.GetCasePlanningBoardData(demandFilterCriteria, employeeCode);
        //    return Ok(casePlanningBoardsData);
        //}

        /// <summary>
        /// Get case planning board data that has been assigned to board columns between date range and by selected demand filters
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///    "demandFilterCriteria": {
        ///        "startDate": ""2023-01-01",
        ///        "endDate": "2023-01-31",
        ///        "officeCodes": "110",
        ///        "caseTypeCodes": "1,2,4,5",
        ///        "demandTypes": "Opportunity,NewDemand,PlanningCards",
        ///        "opportunityStatusTypeCodes": "0,1,2,3,4,5",
        ///        "caseAttributeNames": "",
        ///        "minOpportunityProbability": 0,
        ///        "industryPracticeAreaCodes": "",
        ///        "capabilityPracticeAreaCodes": "",
        ///        "caseExceptionShowList": "",
        ///        "caseExceptionHideList": "",
        ///        "opportunityExceptionShowList": "",
        ///        "opportunityExceptionHideList": "",
        ///        "caseAllocationsSortBy": "",
        ///        "planningCardsSortOrder": "",
        ///        "caseOppSortOrder": ""
        ///    },
        ///    "employeeCode": "39209",
        ///    "date": "{{startDate}}"
        ///}
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("getCasePlanningBoardColumnsData")]
        public async Task<IActionResult> GetCasePlanningBoardColumnsData([FromBody] dynamic payload)
        {
            var demandFilterCriteria = JsonConvert.DeserializeObject<DemandFilterCriteria>(payload["demandFilterCriteria"].ToString());
            var employeeCode = payload["employeeCode"].ToString();

            var casePlanningBoardsData = await _casePlanningMetricsService.GetCasePlanningBoardColumnsData(demandFilterCriteria, employeeCode);
            return Ok(casePlanningBoardsData);
        }

        /// <summary>
        /// Get new demands that are not assigned to board between date range and by selected demand filters
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///    "demandFilterCriteria": {
        ///        "startDate": ""2023-01-01",
        ///        "endDate": "2023-01-31",
        ///        "officeCodes": "110",
        ///        "caseTypeCodes": "1,2,4,5",
        ///        "demandTypes": "Opportunity,NewDemand,PlanningCards",
        ///        "opportunityStatusTypeCodes": "0,1,2,3,4,5",
        ///        "caseAttributeNames": "",
        ///        "minOpportunityProbability": 0,
        ///        "industryPracticeAreaCodes": "",
        ///        "capabilityPracticeAreaCodes": "",
        ///        "caseExceptionShowList": "",
        ///        "caseExceptionHideList": "",
        ///        "opportunityExceptionShowList": "",
        ///        "opportunityExceptionHideList": "",
        ///        "caseAllocationsSortBy": "",
        ///        "planningCardsSortOrder": "",
        ///        "caseOppSortOrder": ""
        ///    },
        ///    "employeeCode": "39209",
        ///    "date": "{{startDate}}"
        ///}
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("getCasePlanningBoardNewDemandsData")]
        public async Task<IActionResult> GetCasePlanningBoardNewDemandsData([FromBody] dynamic payload)
        {
            var demandFilterCriteria = JsonConvert.DeserializeObject<DemandFilterCriteria>(payload["demandFilterCriteria"].ToString());
            var employeeCode = payload["employeeCode"].ToString();

            var casePlanningBoardsData = await _casePlanningMetricsService.GetCasePlanningBoardNewDemandsData(demandFilterCriteria, employeeCode);
            return Ok(casePlanningBoardsData);
        }

        /// <summary>
        /// Get Staffable teams data for planning board columns
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///        "startWeek": ""2023-01-01",
        ///        "endWeek": "2023-01-31",
        ///        "officeCodes": "110",
        ///}
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("getCasePlanningBoardStaffableTeams")]
        public async Task<IActionResult> GetCasePlanningBoardStaffableTeams(dynamic payload)
        {
            var officeCodes = string.IsNullOrEmpty(payload["officeCodes"].ToString()) ? null : payload["officeCodes"].ToString();
            var startWeek = !string.IsNullOrEmpty(payload["startWeek"].ToString()) ? DateTime.Parse(payload["startWeek"].ToString()) : null;
            var endWeek = !string.IsNullOrEmpty(payload["endWeek"].ToString()) ? DateTime.Parse(payload["endWeek"].ToString()) : null;

            var casePlanningBoardStaffableTeams = await _casePlanningMetricsService.GetCasePlanningBoardStaffableTeams(officeCodes, startWeek, endWeek);

            return Ok(casePlanningBoardStaffableTeams);
        }
    }
}
