using CCM.API.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CCM.API.Core.Helpers;

namespace CCM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.CaseTypeLookupReadAccess)]
    public class CaseTypeController : ControllerBase
    {
        private readonly ICaseTypeService _caseTypeService;

        public CaseTypeController(ICaseTypeService caseTypeService)
        {
            _caseTypeService = caseTypeService;
        }

        /// <summary>
        /// Get Case Types from CCM
        /// </summary>
        [HttpGet("casetypes")]
        public async Task<IActionResult> GetCaseTypeList()
        {
            var caseTypes = await _caseTypeService.GetCaseTypeList();
            return Ok(caseTypes);
        }
    }
}