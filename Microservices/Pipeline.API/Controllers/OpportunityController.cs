using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Pipeline.API.Contracts.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Pipeline.API.Core.Helpers;

namespace Pipeline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableQuery]
    [Authorize]
    public class OpportunityController : ControllerBase
    {
        private readonly IOpportunityService _opportunityService;

        public OpportunityController(IOpportunityService opportunityService)
        {
            _opportunityService = opportunityService;
        }

        /// <summary>
        ///     Get opportunities for one or more offices active in specified date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="officeCodes"></param>
        /// <param name="opportunityStatusTypeCodes"></param>
        /// <param name="clientCodes"></param>
        /// <returns>Opportunities active within the selected date range</returns>
        [HttpGet("opportunitiesByOffices")]
        [Authorize(Policy = Constants.Policy.PipelineAllAccess)]
        public async Task<IActionResult> GetOpportunitiesByOfficesActiveInDateRange(DateTime startDate, DateTime endDate, string officeCodes, string opportunityStatusTypeCodes, string clientCodes)
        {
            var opportunities = await _opportunityService.GetOpportunitiesByOfficesActiveInDateRange(startDate, endDate, officeCodes, opportunityStatusTypeCodes, clientCodes);
            return Ok(opportunities);
        }

        /// <summary>
        /// TODO: Right now using the taxonomy Sp. Create a sperate SP and service method   
        /// Get basic opportunity data like opportunity name, client name 
        /// </summary>
        /// <param name="payload"></param>
        /// <remarks>
        /// Sample Request Metadata:
        /// {
        /// "pipelineIds:"",
        /// "officeCodes":"",
        /// "opportunityStatusTypeCodes": ""
        /// }
        /// </remarks>
        [HttpPost("opportunitiesByPipelineIds")]
        [Authorize(Policy = Constants.Policy.PipelineAllAccess)]
        public async Task<IActionResult> GetOpportunitiesByPipelineIds(dynamic payload)
        {
            var pipelineIdList = payload["pipelineIds"]?.ToString(); 
            var officeCodes = payload["officeCodes"]?.ToString();
            var opportunityStatusTypeCodes = payload["opportunityStatusTypeCodes"]?.ToString();
            var opportunities = await _opportunityService.GetOpportunitiesWithTaxonomiesByPipelineIds(pipelineIdList, officeCodes, opportunityStatusTypeCodes);
            return Ok(opportunities);
        }

        /// <summary>
        ///     Get basic opportunity data like opportunity name, client name along with taxonomy data like primary industry and capability names
        /// </summary>
        /// <param name="pipelineIdList"></param>
        [HttpPost("opportunitiesWithTaxonomiesByPipelineIds")]
        [Authorize(Policy = Constants.Policy.PipelineAllAccess)]
        public async Task<IActionResult> GetOpportunitiesWithTaxonomiesByPipelineIds([FromBody] string pipelineIdList)
        {
            var opportunities = await _opportunityService.GetOpportunitiesWithTaxonomiesByPipelineIds(pipelineIdList);
            return Ok(opportunities);
        }


        /// <summary>
        ///     Get opportunity by pipeline ID
        /// </summary>
        /// <param name="pipelineId"></param>
        /// <returns>OpportunityDetails details and resources allocated on it</returns>
        [HttpGet("opportunityDetails")]
        [Authorize(Policy = Constants.Policy.OpportunityDetailsReadAccess)]
        public async Task<IActionResult> GetOpportunityDetailsByPipelineId(Guid pipelineId)
        {
            var opportunities = await _opportunityService.GetOpportunityDetailsByPipelineIds(pipelineId == Guid.Empty ? string.Empty :pipelineId.ToString());
            return Ok(opportunities.FirstOrDefault());
        }
        
        /// <summary>
         ///     Get opportunity by pipeline ID
         /// </summary>
         /// <param name="pipelineIds"></param>
         /// <returns>OpportunityDetails details and resources allocated on it</returns>
        [HttpPost("opportunitiesDetails")]
        [Authorize(Policy = Constants.Policy.PipelineAllAccess)]
        public async Task<IActionResult> GetOpportunityDetailsByPipelineIds([FromBody] string pipelineIds)
        {
            var opportunities = await _opportunityService.GetOpportunityDetailsByPipelineIds(pipelineIds);
            return Ok(opportunities);
        }

        /// <summary>
        ///     Get opportunities for typeahead
        /// </summary>
        /// <param name="searchString">Search opportunities by its name, client Code or Client Name</param>
        [HttpGet("typeaheadOpportunities")]
        [Authorize(Policy = Constants.Policy.PipelineAllAccess)]
        public async Task<IActionResult> GetOpportunitiesForTypeahead(string searchString)
        {
            var opportunities = await _opportunityService.GetOpportunitiesForTypeahead(searchString);
            return Ok(opportunities);
        }

        /// <summary>
        /// Gets all the records from opportunity_master that have been updated in pipeline after a specific datetime
        /// </summary>
        /// <param name="lastPolledDateTime">Do not pass anything to get the records with last updated >= yesterday </param>
        /// <returns></returns>
        [HttpGet("opportunityMasterChangesSinceLastPolled")]
        [Authorize(Policy = Constants.Policy.PipelineAllAccess)]
        public async Task<IActionResult> GetOpportunityMasterChangesSinceLastPolled(DateTime? lastPolledDateTime)
        {
            var caseMasterAndCaseMasterHistoryDataChanges = await _opportunityService.GetOpportunityMasterChangesSinceLastPolled(lastPolledDateTime);
            return Ok(caseMasterAndCaseMasterHistoryDataChanges);
        }

        /// <summary>
        /// Gets all the records from opportunity_master and related tables for analyticss
        /// </summary>
        /// <param name="lastUpdated">[Optional] Do not pass anything to get the all records, If passed, gets opps updated after that time</param>
        /// <returns></returns>
        [HttpGet("opportunitiesFlatData")]
        [Authorize(Policy = Constants.Policy.PipelineAllAccess)]
        public async Task<IActionResult> GetOpportunitiesFlatData(DateTime? lastUpdated)
        {
            var opportunities = await _opportunityService.GetOpportunitiesFlatData(lastUpdated);
            return Ok(opportunities);
        }

        /// <summary>
        /// Gets opportunity by cortex Id
        /// </summary>
        /// <param name="cortexOpportunityId"></param>
        /// <returns></returns>
        [HttpGet("getOppDataFromCortex")]
        [Authorize(Policy = Constants.Policy.PipelineAllAccess)]
        public async Task<IActionResult> GetOppDataFromCortex(string cortexOpportunityId)
        {
            var opportunities = await _opportunityService.GetOppDataFromCortex(cortexOpportunityId);
            return Ok(opportunities);
        }

    }
}