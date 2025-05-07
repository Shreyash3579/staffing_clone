using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pipeline.API.Contracts.Services;
using System.Threading.Tasks;
using Pipeline.API.Core.Helpers;

namespace Pipeline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.OpportunityLookupReadAccess)]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        /// <summary>
        /// Get Opporunity Status Types from Pipeline
        /// </summary>
        [HttpGet("opportunityStatusTypes")]
        public async Task<IActionResult> GetOpportunityStatusTypeList()
        {
            var caseTypes = await _lookupService.GetOpportunityStatusTypeList();
            return Ok(caseTypes);
        }

    }
}