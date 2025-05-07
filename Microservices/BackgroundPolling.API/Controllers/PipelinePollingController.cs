using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PipelinePollingController : ControllerBase
    {
        private readonly IPipelinePollingService _pipelinePollingService;

        public PipelinePollingController(IPipelinePollingService pipelinePollingService)
        {
            _pipelinePollingService = pipelinePollingService;
        }

        [HttpPost]
        [Route("updateOpportunityEndDate")]
        public async Task<IActionResult> UpdateOpportunityEndDateFromPipeline()
        {
            await _pipelinePollingService.UpdateOpportunityEndDateFromPipeline();
            return Ok();
        }

        /// <summary>
        /// Gets all the records from opportunity_master and related tables for analyticss
        /// </summary>
        /// <param name="isFullLoad">[True]: If need full refresh of table, [False]: For incremental</param>
        /// <param name="lastUpdated">[Optional] Gets cases updated after the specified date</param>
        /// <returns></returns>
        //TODO: get data  via polling . Right now it uses direct basis joins to pupulate thie table
        [HttpPost]
        [Route("upsertOpportunitiesFlatDataFromPipeline")]
        public async Task<IActionResult> UpsertOpportunitiesFlatDataFromPipeline(bool isFullLoad, DateTime? lastUpdated)
        {
            await _pipelinePollingService.UpsertOpportunitiesFlatDataFromPipeline(isFullLoad, lastUpdated);
            return Ok();
        }

    }
}
