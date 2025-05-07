using CCM.API.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using CCM.API.Core.Helpers;

namespace CCM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.CCMAllAccess)]
    public class CaseAttributeController : ControllerBase
    {
        public readonly ICaseAttributeService _caseAttributeService;
        public CaseAttributeController(ICaseAttributeService caseAttributeService)
        {
            _caseAttributeService = caseAttributeService;
        }
        /// <summary>
        /// Get Case Attributes as per Last Updated Date
        /// </summary>
        /// <param name="lastupdateddate"></param>
        /// <returns>Collection of Case Attributes with their respective case attribute code and names</returns>
        [HttpGet("getcaseattributesbylastupdateddate")]
        public async Task<IActionResult> GetCaseAttributeByLastUpdatedDate(DateTime? lastupdateddate)
        {
            var result = await _caseAttributeService.GetCaseAttributesByLastUpdatedDate(lastupdateddate);
            return Ok(result);
        }
    }
}
