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
    public class CasePlanningController : ControllerBase
    {
        private readonly ICasePlanningService _service;
        public CasePlanningController(ICasePlanningService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("getCasePlanningBoardDataByDateRange")]
        public async Task<IActionResult> GetCasePlanningBoardDataByDateRange(DateTime startDate, DateTime? endDate = null, string loggedInUser = null)
        {
            var casePlanningBoardData = await _service.GetCasePlanningBoardDataByDateRange(startDate, endDate, loggedInUser);
            return Ok(casePlanningBoardData);
        }

        [HttpGet]
        [Route("getCasePlanningBoardDataByProjectEndDateAndBucketIds")]
        public async Task<IActionResult> GetCasePlanningBoardDataByProjectEndDateAndBucketIds(DateTime startDate, string bucketIds)
        {
            var casePlanningBoardDataByProjectEndDateAndBucketIds = await _service.GetCasePlanningBoardDataByProjectEndDateAndBucketIds(startDate, bucketIds);
            return Ok(casePlanningBoardDataByProjectEndDateAndBucketIds);
        }


        /// <summary>
        /// Get Case Planning Board data by comma separated values of oldCases, pipelineIds, and planningCardIds
        /// Sample Request:
        /// {
        ///     "oldCaseCodes": "H3NB,P6VR,Y3PW",
        ///     "pipelineIds": "46D856E3-09AE-4A36-B210-958D0A173E88,CAE8B3E4-6BA4-4A86-9EB1-5FCAE952FDB4",
        ///     "planningCardIds": "EBE451F2-8F56-EB11-A9AD-005056ACCF10"
        /// }
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getCasePlanningBoardDataByProjectIds")]
        public async Task<IActionResult> GetCasePlanningBoardDataByProjectIds(dynamic payload)
        {
            var oldCaseCodes = string.IsNullOrEmpty(payload["oldCaseCodes"].ToString()) ? null : payload["oldCaseCodes"].ToString();
            var pipelineIds = string.IsNullOrEmpty(payload["pipelineIds"].ToString()) ? null : payload["pipelineIds"].ToString();
            var planningCardIds = string.IsNullOrEmpty(payload["planningCardIds"].ToString()) ? null : payload["planningCardIds"].ToString();

            var casePlanningBoardData = await _service.GetCasePlanningBoardDataByProjectIds(oldCaseCodes, pipelineIds, planningCardIds);
            return Ok(casePlanningBoardData);
        }

        [HttpGet]
        [Route("getOpportunityDataInCasePlanningBoard")]
        public async Task<IActionResult> GetOpportunityDataInCasePlanningBoard()
        {
            var casePlanningBoardData = await _service.GetOpportunityDataInCasePlanningBoard();
            return Ok(casePlanningBoardData);
        }

        [HttpPut]
        public async Task<IActionResult> UpsertCasePlanningBoard(CasePlanningBoard casePlanningBoard)
        {
            var upsertedCasePlanningBoardData = await _service.UpsertCasePlanningBoard(casePlanningBoard);
            return Ok(upsertedCasePlanningBoardData);
        }

        /// <summary>
        /// Used to insert/update case planning board data
        /// </summary>
        /// <param name="payload">IEnumerable of casePlanningBoard model data</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpsertCasePlanningBoardData(dynamic payload)
        {
            IEnumerable<CasePlanningBoard> casePlanningBoardData = JsonConvert.DeserializeObject<CasePlanningBoard[]>(payload.ToString());
            var upsertedCasePlanningBoardData = await _service.UpsertCasePlanningBoardData(casePlanningBoardData);

            return Ok(upsertedCasePlanningBoardData);
        }

        /// <summary>
        /// Use this API end point to delete case planning board data by passing the comma separated Ids
        /// that need to be deleted with ecode as lastUpdatedBy
        /// </summary>
        /// <param name="planningBoardIds"></param>
        /// <param name="lastUpdatedBy"></param>
        /// <remarks>
        /// Sample Request:
        /// {
        ///    planningBoardIds = '5DF132A8-7077-EC11-A9B9-B327A03BC311,0257B8C2-8A73-EC11-A9B9-B327A03BC311',
        ///    lastUpdatedBy = '45088'
        /// }
        /// </remarks>
        [HttpDelete]
        public async Task<IActionResult> DeleteCasePlanningBoardByIds(string planningBoardIds, string lastUpdatedBy)
        {
            await _service.DeleteCasePlanningBoardByIds(planningBoardIds, lastUpdatedBy);
            return Ok();
        }

        [HttpPut]
        [Route("upsertCasePlanningBoardBucketPreferences")]
        public async Task<IActionResult> UpsertCasePlanningBoardBucketPreferences(CasePlanningBoardBucketPreferences prefrencesData)
        {
            var upsertedPrefrencesForEmployee = await _service.UpsertCasePlanningBoardBucketPreferences(prefrencesData);

            return Ok(upsertedPrefrencesForEmployee);
        }

        [HttpPut]
        [Route("upsertCasePlanningBoardIncludeInDemandPreferences")]
        public async Task<IActionResult> UpsertCasePlanningBoardIncludeInDemandPreferences(CasePlanningBoardProjectPreferences prefrencesData)
        {
            var upsertedPrefrencesForEmployee = await _service.UpsertCasePlanningBoardIncludeInDemandPreferences(prefrencesData);
            return Ok(upsertedPrefrencesForEmployee);
        }

        [HttpPut]
        [Route("upsertCasePlanningProjectDetails")]
        public async Task<IActionResult> UpsertCasePlanningProjectDetails(CasePlanningProjectPreferences[] prefrencesData)
        {
            var upsertedPrefrences = await _service.UpsertCasePlanningProjectDetails(prefrencesData);
            return Ok(upsertedPrefrences);
        }

        [HttpPost]
        [Route("getCasePlanningProjectDetails")]
        public async Task<IActionResult> GetCasePlanningProjectDetails(dynamic payload)
        {
            var oldCaseCodes = payload["oldCaseCodes"].ToString();
            var pipelineIds = string.IsNullOrEmpty(payload["pipelineIds"].ToString())
                ? null
                : payload["pipelineIds"].ToString();
            var planningCardIds = string.IsNullOrEmpty(payload["planningCardIds"].ToString())
                ? null
                : payload["planningCardIds"].ToString();
            var includeInDemandProjects = await _service.GetCasePlanningProjectDetails(oldCaseCodes, pipelineIds, planningCardIds);
            return Ok(includeInDemandProjects);
        }

    }
}
