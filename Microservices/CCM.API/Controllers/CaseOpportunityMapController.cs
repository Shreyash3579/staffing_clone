using CCM.API.Contracts.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CCM.API.Core.Helpers;

namespace CCM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.CCMAllAccess)]
    public class CaseOpportunityMapController : ControllerBase
    {
        private readonly ICaseOpportunityMapService _caeCaseOpportunityMapService;

        public CaseOpportunityMapController(ICaseOpportunityMapService caeCaseOpportunityMapService)
        {
            _caeCaseOpportunityMapService = caeCaseOpportunityMapService;
        }

        /// <summary>
        ///     Get opportunities converted to case
        /// </summary>
        /// <param name="payload">Comma separated list of Pipeline Id that might converted to case</param>
        /// <returns>Case Code, Client Code and Old Case code for opportunity converted to case</returns>
        [HttpPost("casesByPipelineIds")]
        public async Task<IActionResult> GetCasesForOpportunityConversion(dynamic payload)
        {
            var pipelineIds = $"{payload["pipelineIds"]}";
            var caseOpportunityMapList =
                await _caeCaseOpportunityMapService.GetCasesForOpportunityConversion(pipelineIds);
            return Ok(caseOpportunityMapList);
        }
    }
}