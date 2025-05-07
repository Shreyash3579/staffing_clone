using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class PlanningCardController : ControllerBase
    {
        private readonly IPlanningCardService _planningCardService;

        public PlanningCardController(IPlanningCardService planningCardService)
        {
            _planningCardService = planningCardService;
        }

        ///// <summary>
        ///// Get  planning cards data created by logged in user or shgared with logged in logged in user office + allocations data on them for logged in employee and date and other filters
        ///// </summary>
        ///// <returns></returns>
        [HttpGet]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> GetPlanningCardAndItsAllocationsByEmployeeCodeAndFilters(string employeeCode, string officeCodes, string staffingTags,
            DateTime? startDate, DateTime? endDate, string bucketIds = null)
        {
            var result = await _planningCardService.GetPlanningCardAndItsAllocationsByEmployeeCodeAndFilters(employeeCode, officeCodes, staffingTags, startDate, endDate, bucketIds);
            return Ok(result);
        }

        //commented on 06-jun-23 as it is not being used anymore
        ///// <summary>
        ///// Get  allocations on planning cards created by logged in user or shgared with logged in logged in user office and date and other filters
        ///// </summary>
        ///// <remarks>
        ///// Sample Request:
        ///// {
        ///// "employeeCodes": "39209",
        ///// "startDate": "2023-06-01",
        ///// "endDate": "2023-06-030",
        ///// }
        ///// </remarks>
        ///// <param name="payload"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("planningCardAllocations")]
        //public async Task<IActionResult> GetPlanningCardAllocationsByEmployeeCodesAndDuration(dynamic payload)
        //{
        //    var employeeCodes = payload["employeeCodes"].ToString();
        //    var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString())
        //        ? DateTime.Parse(payload["startDate"].ToString())
        //        : null;
        //    var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString())
        //        ? DateTime.Parse(payload["endDate"].ToString())
        //        : null;
        //    var result = await _planningCardService.GetPlanningCardAllocationsByEmployeeCodesAndDuration(employeeCodes, startDate, endDate);
        //    return Ok(result);
        //}

        ///// <summary>
        ///// Inserts the planning card
        ///// </summary>
        ///// <remarks>
        ///// Sample Request:
        ////{
        ////    "name": "Affiliated consultants",
        ////    "startDate": null,
        ////    "endDate": null,
        ////    "isShared": false,
        ////    "sharedOfficeCodes": null,
        ////    "sharedStaffingTags": null,
        ////    "lastUpdatedBy": "60074"
        ////}
        ///// </remarks>
        ///// <returns></returns>
        //ToDo: Remove this endpoint as moved to UpsertPlanningCard By : 26-March-2024
        [HttpPost]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> InsertPlanningCard(PlanningCard planningCard)
        {
            var result = await _planningCardService.InsertPlanningCard(planningCard);
            return Ok(result);
        }

        ///// <summary>
        ///// Update the planning card
        ///// </summary>
        ///// <remarks>
        ///// Sample Request:
        ////{
        ////    "id": "b594e120-8a9d-ed11-a9c4-005056accf10",
        ////    "name": "Affiliated consultants",
        ////    "startDate": null,
        ////    "endDate": null,
        ////    "isShared": true,
        ////    "sharedOfficeCodes": "115,125,128,110,160,539,127,152,153,542,511,165,400,112,504,521,150,151,120,116,508,524,176,395,170,175,535,177,500",
        ////    "sharedStaffingTags": "SL0001",
        ////    "lastUpdatedBy": "60074"
        ////}
        ///// </remarks>
        ///// <returns></returns>

        //ToDo: Remove this endpoint as moved to UpsertPlanningCard By : 26-March-2024
        [HttpPut]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> UpdatePlanningCard(PlanningCard planningCard)
        {
            var result = await _planningCardService.UpdatePlanningCard(planningCard);
            return Ok(result);
        }

        [HttpPost]
        [Route("upsertPlanningCard")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> UpsertPlanningCard(PlanningCard planningCard)
        {
            var result = await _planningCardService.UpsertPlanningCard(planningCard);
            return Ok(result);
        }

        /// <summary>
        /// Deletes Planning card and the allocations that were created on the planning cards
        /// </summary>
        /// <param name="id">Id of the planning card to be deleted</param>
        /// <param name="lastUpdatedBy">Employee code of the user who deleted it</param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> DeletePlanningCardAndItsAllocations(Guid id, string lastUpdatedBy)
        {
            await _planningCardService.DeletePlanningCardAndItsAllocations(id, lastUpdatedBy);
            return Ok();
        }

        /// <summary>
        /// Saves the shared planning card details like which office and staffing tag it has been shared with
        /// </summary>
        //// <param name="payload">Sku model object</param>
        /// <remarks>
        /// Sample Request:
        ///{
        ///    "id": "b594e120-8a9d-ed11-a9c4-005056accf10",
        ///    "name": "Affiliated consultants",
        ///    "startDate": null,
        ///    "endDate": null,
        ///    "isShared": true,
        ///    "sharedOfficeCodes": "115,125,128,110,160,539,127,152,153,542,511,165,400,112,504,521,150,151,120,116,508,524,176,395,170,175,535,177,500",
        ///    "sharedStaffingTags": "SL0001",
        ///    "lastUpdatedBy": "60074"
        ///}
        /// </remarks>
        /// <returns></returns>
        //
        //ToDo: Remove this endpoint as moved to UpsertPlanningCard By : 26-March-2024
        [HttpPost]
        [Route("sharePlanningCard")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> SharePlanningCard(PlanningCard planningCard)
        {
            var result = await _planningCardService.SharePlanningCard(planningCard);
            return Ok(result);
        }

        ///// <summary>
        /////   Gets the planning cards data by passing comma separated list of planning card ids
        ///// </summary>
        ///// <param name="planningCardIdList">Comma separated list of planning card ids</param>
        ///// <returns></returns>
        [HttpPost]
        [Route("planningCardsByPlanningCardIds")]
        [Authorize(Policy = Constants.Policy.PlanningCardDetailsRead)]

        public async Task<IActionResult> GetPlanningCardByPlanningCardIds([FromBody] string planningCardIdList)
        {
            var result = await _planningCardService.GetPlanningCardByPlanningCardIds(planningCardIdList);
            return Ok(result);
        }

        /// <summary>
        /// Get Planning Cards by Peg Opportunity Ids
        /// </summary>
        /// <param name="pegOpportunityIds">Comma separated list of Ids of the PEG lead coming from PEG systems</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getPlanningCardByPegOpportunityIds")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> GetPlanningCardByPegOpportunityIds(string pegOpportunityIds)
        {
            var result = await _planningCardService.GetPlanningCardByPegOpportunityIds(pegOpportunityIds);
            return Ok(result);
        }

        /// <summary>
        ///     Get planning card for typeahead
        /// </summary>
        /// <param name="searchString">Search planning card by its name</param>
        [HttpGet("planningCardTypeahead")]
        [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
        public async Task<IActionResult> GetPlanningCardsForTypeahead(string searchString)
        {
            var planningCards = await _planningCardService.GetPlanningCardsForTypeahead(searchString);
            return Ok(planningCards);
        }

    }
}
