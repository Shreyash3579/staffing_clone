using CCM.API.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CCM.API.Core.Helpers;

namespace CCM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.CCMAllAccess)]
    public class LookupController : ControllerBase
    {

        private readonly ILookupService _service;

        public LookupController(ILookupService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get Case Attribute for lookup
        /// </summary>
        /// <returns></returns>
        [HttpGet("caseAttributes")]
        public async Task<IActionResult> GetCaseAttributeLookupList()
        {
            var caseRoleTypes = await _service.GetCaseAttributeLookupList();
            return Ok(caseRoleTypes);
        }
    }
}