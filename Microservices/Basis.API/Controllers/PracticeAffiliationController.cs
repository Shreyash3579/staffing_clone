using Basis.API.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basis.API.Core.Helpers;

namespace Basis.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.BasisAllAccess)]

    public class PracticeAffiliationController : ControllerBase
    {
        private readonly IPracticeAffiliationService _service;

        public PracticeAffiliationController(IPracticeAffiliationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get Practice Affiliations for employees
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAllPracticeAffiliation")]
        public async Task<IActionResult> GetAllPracticeAffiliation()
        {
            var result = await _service.GetAllPracticeAffiliation();
            return Ok(result);
        }
    }
}
