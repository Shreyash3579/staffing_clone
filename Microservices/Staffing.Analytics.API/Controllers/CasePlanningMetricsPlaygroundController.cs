using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CasePlanningMetricsPlaygroundController : ControllerBase
    {
        private readonly ICasePlanningMetricsPlaygroundService _casePlanningPlaygroundService;

        public CasePlanningMetricsPlaygroundController(ICasePlanningMetricsPlaygroundService casePlanningPlaygroundService)
        {
            _casePlanningPlaygroundService = casePlanningPlaygroundService;
        }

        /// <summary>
        /// This API end point is used to create API end point using the demand and supply filters of the user.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        ///        {
        ///   "demandFilterCriteriaObj":{
        ///      "startDate":"2022-11-14",
        ///      "endDate":"2022-12-23",
        ///      "officeCodes":"115,110,210,512,215,536",
        ///      "caseTypeCodes":"1",
        ///      "demandTypes":"Opportunity,NewDemand,PlanningCards",
        ///      "opportunityStatusTypeCodes":"0,1,2,3,4,5",
        ///      "caseAttributeNames":"",
        ///      "minOpportunityProbability":0,
        ///      "industryPracticeAreaCodes":"",
        ///      "capabilityPracticeAreaCodes":"",
        ///      "caseExceptionShowList":"",
        ///      "caseExceptionHideList":"",
        ///      "opportunityExceptionShowList":"",
        ///      "opportunityExceptionHideList":"",
        ///      "caseAllocationsSortBy":"",
        ///      "planningCardsSortOrder":"",
        ///      "caseOppSortOrder":""
        ///   },
        ///   "supplyFilterCriteriaObj":{
        ///      "startDate":"2022-11-14",
        ///      "endDate":"2022-12-23",
        ///      "officeCodes":"115",
        ///      "levelGrades":"",
        ///      "staffingTags":"SL0001",
        ///      "availabilityIncludes":"",
        ///      "groupBy":"serviceLine",
        ///      "sortBy":"levelGrade",
        ///      "affiliationRoleCodes":"",
        ///      "certificates":"",
        ///      "languages":"",
        ///      "practiceAreaCodes":"",
        ///      "employeeStatuses":"",
        ///      "positionCodes":"",
        ///      "staffableAsTypeCodes":""
        ///   },
        ///   "lastUpdatedBy":"45088"
        ///}
        /// </remarks>
        [HttpPost("createCasePlanningBoardMetricsPlayground")]
        public async Task<IActionResult> CreateCasePlanningBoardMetricsPlayground([FromBody] dynamic payload)
        {
            var demandFilterCriteria = JsonConvert.DeserializeObject<DemandFilterCriteria>(payload["demandFilterCriteriaObj"].ToString());
            var supplyFilterCriteria = JsonConvert.DeserializeObject<SupplyFilterCriteria>(payload["supplyFilterCriteriaObj"].ToString());
            var isCountOfIndividualResourcesToggle = Convert.ToBoolean(payload["isCountOfIndividualResourcesToggle"].ToString());
            var enableMemberGrouping = Convert.ToBoolean(payload["enableMemberGrouping"].ToString());
            var enableNewlyAvailableHighlighting = Convert.ToBoolean(payload["enableNewlyAvailableHighlighting"].ToString());
            var lastUpdatedBy = payload["lastUpdatedBy"].ToString();

            var casePlanningBoardsData = await _casePlanningPlaygroundService.CreateCasePlanningBoardMetricsPlayground(demandFilterCriteria, supplyFilterCriteria,
                isCountOfIndividualResourcesToggle, enableMemberGrouping, enableNewlyAvailableHighlighting, lastUpdatedBy);
            return Ok(casePlanningBoardsData);
        }
    }
}
