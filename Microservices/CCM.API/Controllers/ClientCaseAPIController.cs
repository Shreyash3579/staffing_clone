using CCM.API.Contracts.Services;
using CCM.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using CCM.API.Core.Helpers;

namespace CCM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.CCMAllAccess)]
    public class ClientCaseAPIController : ControllerBase
    {
        private readonly IClientCaseAPIService _service;

        public ClientCaseAPIController(IClientCaseAPIService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all the cases modified after a specific date.
        /// For e.g. On passing lastPolledTime as 2022-10-25, result will be of all the cases modified on or after 26th Oct 2022.
        /// If lastPolledTime is NULL then by default, todays date will be picked.
        /// </summary>
        /// <param name="lastPolledTime">NULL for today</param>
        /// <returns></returns>
        [HttpGet("getModifiedCasesAfterLastPolledTime")]
        public async Task<IActionResult> GetModifiedCasesAfterLastPolledTime(DateTime? lastPolledTime)
        {
            var caseRoleTypes = await _service.GetModifiedCasesAfterLastPolledTime(lastPolledTime);
            return Ok(caseRoleTypes);
        }

        /// <summary>
        ///     Get clients for typeahead
        /// </summary>
        /// <param name="searchString">Search cases by client name</param>
        /// <returns></returns>
        [HttpGet("typeaheadClients")]
        [ProducesResponseType(typeof(ClientViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientsForTypeahead(string searchString)
        {
            var cases = await _service.GetClientsForTypeahead(searchString);
            return Ok(cases);
        }
    }
}
