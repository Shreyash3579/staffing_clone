using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class CaseOppChangesController : ControllerBase
    {
        private readonly ICaseOppChangesService _caseOppChangesService;

        public CaseOppChangesController(ICaseOppChangesService caseOppChangesService)
        {
            _caseOppChangesService = caseOppChangesService;
        }

        /// <summary>
        ///      Get pipeline changes saved in staffing db id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("pipelineChangesByPipelineId")]
        public async Task<IActionResult> GetPipelineChangesByPipelineId(string pipelineId)
        {
            var data = await _caseOppChangesService.GetPipelineChangesByPipelineIds(pipelineId);
            return Ok(data.FirstOrDefault());
        }

        /// <summary>
        ///      Get pipeline changes saved in staffing db by ids
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("pipelineChangesByPipelineIds")]
        public async Task<IActionResult> GetPipelineChangesByPipelineIds(dynamic payload)
        {
            var pipelineIds = $"{payload["pipelineIds"]}";
            var data = await _caseOppChangesService.GetPipelineChangesByPipelineIds(pipelineIds);
            return Ok(data);
        }
        /// <summary>
        ///     Update pipeline changes in staffing db
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("upsertPipelineChanges")]
        public async Task<IActionResult> UpsertPipelineChanges(CaseOppChanges updatedData)
        {
            var data = await _caseOppChangesService.UpsertPipelineChanges(updatedData);
            return Ok(data);
        }

        /// <summary>
        ///      Get pipeline changes based on date range
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("pipelineChangesByDateRange")]
        public async Task<IActionResult> GetPipelineChangesByDateRange(DateTime startDate, DateTime? endDate)
        {
            var data = await _caseOppChangesService.GetPipelineChangesByDateRange(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        ///      Get Case changes saved in staffing db
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("caseChangesByOldCaseCode")]
        public async Task<IActionResult> GetCaseChangesByOldCaseCode(string oldCaseCode)
        {
            var data = await _caseOppChangesService.GetCaseChangesByOldCaseCodes(oldCaseCode);
            return Ok(data.FirstOrDefault());
        }

        /// <summary>
        ///      Get Case changes saved in staffing db
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("caseChangesByOldCaseCodes")]
        public async Task<IActionResult> GetCaseChangesByOldCaseCodes(dynamic payload)
        {
            var oldCaseCodes = $"{payload["oldCaseCodes"]}";
            var data = await _caseOppChangesService.GetCaseChangesByOldCaseCodes(oldCaseCodes);
            return Ok(data);
        }

        /// <summary>
        ///      Get case teamsize with oldCaseCode
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("caseTeamSizeByOldCaseCodes")]
        public async Task<IActionResult> GetCaseTeamSizeByOldCaseCodes(string oldCaseCode)
        {
            var data = await _caseOppChangesService.GetCaseTeamSizeByOldCaseCodes(oldCaseCode);
            return Ok(data);
        }

        /// <summary>
        ///     Update Case changes in staffing db
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("upsertCaseChanges")]
        public async Task<IActionResult> UpsertCaseChanges(CaseOppChanges updatedData)
        {
            var data = await _caseOppChangesService.UpsertCaseChanges(updatedData);
            return Ok(data);
        }


        /// <summary>
        ///      Get case changes based on date range
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("caseChangesByDateRange")]
        public async Task<IActionResult> GetCaseChangesByDateRange(DateTime startDate, DateTime? endDate)
        {
            var data = await _caseOppChangesService.GetCaseChangesByDateRange(startDate, endDate);
            return Ok(data);
        }


        /// <summary>
        ///      Get case changes based on office codes and date range
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("caseOppChangesByOfficesAndDateRange")]
        public async Task<IActionResult> GetCaseOppChangesByOfficesAndDateRange(string officeCodes, DateTime? startDate = null, DateTime? endDate = null)
        {
            var data = await _caseOppChangesService.GetCaseOppChangesByOfficesAndDateRange(officeCodes, startDate, endDate);
            return Ok(data);
        }
    }
}