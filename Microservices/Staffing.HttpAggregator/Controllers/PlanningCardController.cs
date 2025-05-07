using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlanningCardController : ControllerBase
    {
        private readonly IPlanningCardService _planningCardService;

        public PlanningCardController(IPlanningCardService planningCardService)
        {
            _planningCardService = planningCardService;
        }

        /// <summary>
        /// Updates Case Code on the planning card and triggers PEG service bus for PEG planning cards
        /// </summary>
        /// <param name="payload">Planning Card data that is merged</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "oldCaseCodes":"Q6BK",
        ///       "employeeCodes":"39980",
        ///       "lastUpdated":"2020-04-09",
        ///       "startDate":"2020-04-01",
        ///       "endDate":"2020-04-17",
        ///       "caseRoleCodes": null
        ///    }
        /// </remarks>
        [HttpPost]
        [Route("mergePlanningCard")]
        // [ProducesResponseType(typeof(PlanningCard), StatusCodes.Status200OK)]
        public async Task<IActionResult> MergePlanningCard(dynamic payload)
        {
            var mergedPlanningCard = JsonConvert.DeserializeObject<PlanningCard>(payload["planningCard"].ToString());
            IEnumerable<ResourceAssignmentViewModel> resourceAllocations = JsonConvert.DeserializeObject<ResourceAssignmentViewModel[]>(payload["resourceAllocations"].ToString());
            IEnumerable<ResourceAssignmentViewModel> placeholderAllocations = JsonConvert.DeserializeObject<ResourceAssignmentViewModel[]>(payload["placeholderAllocations"].ToString());

            var isSuccess = await _planningCardService.MergePlanningCard(mergedPlanningCard, resourceAllocations, placeholderAllocations);

            return Ok(isSuccess);
        }
    }
}
