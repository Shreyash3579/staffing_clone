using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StaffingAggregatorController : ControllerBase
    {
        private readonly IOpportunityService _opportunityService;
        private readonly ICaseService _caseService;
        private readonly IAuditTrailService _auditTrailService;
        private readonly IStaffingService _staffingService;
        private readonly INoteLogService _noteLogService;


        public StaffingAggregatorController(IOpportunityService opportunityService, ICaseService caseService,
            IAuditTrailService auditTrailService, IStaffingService staffingService, INoteLogService noteLogServce)
        {
            _opportunityService = opportunityService;
            _caseService = caseService;
            _auditTrailService = auditTrailService;
            _staffingService = staffingService;
            _noteLogService = noteLogServce;
        }

        /// <summary>
        /// This method is used to get the opportunity data on the basis of pipelineId.
        /// </summary>
        /// <param name="pipelineId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("opportunityByPipelineId")]
        public async Task<IActionResult> GetOpportunityByPipelineId(Guid pipelineId)
        {
            var opportunity = await _opportunityService.GetOpportunity(pipelineId);
            return Ok(opportunity);
        }

        /// <summary>
        ///     Get opportunity and resources allocated on it
        /// </summary>
        /// <param name="pipelineId"></param>
        /// <returns>Opportunity details and resources allocated on it</returns>
        [HttpGet("opportunityAndAllocationsByPipelineId")]
        public async Task<IActionResult> GetOpportunityAndAllocationsByPipelineId(Guid pipelineId)
        {
            var opportunity = await _opportunityService.GetOpportunityAndAllocationsByPipelineId(pipelineId);
            return Ok(opportunity);
        }

        /// <summary>
        ///     Get opportunities for typeahead
        /// </summary>
        /// <param name="searchString">Search opportunities by its name, client Code or Client Name</param>
        [HttpGet("typeaheadOpportunities")]
        public async Task<IActionResult> GetOpportunitiesForTypeahead(string searchString)
        {
            var opportunities = await _opportunityService.GetOpportunitiesForTypeahead(searchString);
            return Ok(opportunities);
        }

        /// <summary>
        /// Get details of multiple cases on the basis of oldCaseCodes
        /// </summary>
        /// <param name="oldCaseCodes"></param>
        /// <returns></returns>
        [HttpPost("caseDataByCaseCodes")]
        public async Task<IActionResult> GetCaseDataByCaseCodes([FromBody] string oldCaseCodes)
        {
            var cases =
                await _caseService.GetCaseDataByCaseCodes(oldCaseCodes);
            return Ok(cases);
        }

        [HttpGet("caseDetailsByCaseCode")]
        public async Task<IActionResult> GetCaseDetailsByCaseCode(string oldCaseCode)
        {
            var cases =
                await _caseService.GetCaseDetailsByCaseCode(oldCaseCode);
            return Ok(cases);
        }

        /// <summary>
        ///     Get case details and resources allocated on it
        /// </summary>
        /// <param name="oldCaseCode"></param>
        /// <returns>Case details for a specific case and resources allocated on it</returns>
        [HttpGet("caseAndAllocationsByCaseCode")]
        public async Task<IActionResult> GetCaseAndAllocationsByCaseCode(string oldCaseCode)
        {
            var cases =
                await _caseService.GetCaseAndAllocationsByCaseCode(oldCaseCode);
            return Ok(cases);
        }

        /// <summary>
        /// Get Audit trail for case or opportunity
        /// </summary>
        /// <param name="oldCaseCode">Case code to get audit trail for case</param>
        /// <param name="pipelineId">Id to get audit trail for opportunity</param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns>Audit history</returns>
        [HttpGet]
        [Route("auditCase")]
        public async Task<IActionResult> GetAuditTrailForCaseOrOpportunity(string oldCaseCode, string pipelineId, int? limit, int? offset)
        {
            var auditTrails = await _auditTrailService.GetAuditTrailForCaseOrOpportunity(oldCaseCode, pipelineId, limit, offset);
            return Ok(auditTrails);
        }

        /// <summary>
        /// Get Audit trail for an employee
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns>Audit history</returns>
        [HttpGet]
        [Route("auditEmployee")]
        public async Task<IActionResult> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset)
        {
            var auditTrails = await _auditTrailService.GetAuditTrailForEmployee(employeeCode, limit, offset);
            return Ok(auditTrails);
        }

        [HttpGet]
        [Route("resourceNotes")]
        public async Task<IActionResult> GetResourceNotes(string employeeCode, string loggedInEmployeeCode, string noteTypeCode)
        {
            var resourceNotes = await _noteLogService.GetResourceNotes(employeeCode, loggedInEmployeeCode, noteTypeCode);
            return Ok(resourceNotes);
        }

        /// <summary>
        /// Gets all staffing allocations for a resource
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <returns>List of staffing assignments</returns>
        [HttpGet]
        [Route("historicalStaffingAllocationsByEmployee")]
        public async Task<IActionResult> GetHistoricalStaffingAllocationsForEmployee(string employeeCode)
        {
            var historyData = await _staffingService.GetHistoricalStaffingAllocationsForEmployee(employeeCode);
            return Ok(historyData);
        }

        /// <summary>
        /// Gets all staffing allocations for a project
        /// </summary>
        /// <param name="oldCaseCode"></param>
        /// <param name="pipelineId"></param>
        /// <returns>List of staffing assignments</returns>
        [HttpGet]
        [Route("historicalStaffingAllocationsForProject")]
        public async Task<IActionResult> GetHistoricalStaffingAllocationsForProject(string oldCaseCode, string pipelineId)
        {
            var historyData = await _staffingService.GetHistoricalStaffingAllocationsForProject(oldCaseCode, pipelineId);
            return Ok(historyData);
        }

        /// <summary>
        /// Gets staffing allocations for a resource based on date range
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="effectiveFromDate"></param>
        /// <param name="effectiveToDate"></param>
        /// <returns>List of staffing assignments</returns>
        [HttpGet]
        [Route("staffingAllocationsByEmployee")]
        public async Task<IActionResult> GetStaffingAllocationsForEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var historyData = await _staffingService.GetStaffingAllocationsForEmployee(employeeCode, effectiveFromDate, effectiveToDate);
            return Ok(historyData);
        }

        /// <summary>
        ///     Get Resources and their staffing on cases/opportunities
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>List of Resources</returns>
        [HttpPost]
        [Route("resourcesStaffing")]
        public async Task<IActionResult> GetFilteredResourcesWithAllocations(dynamic payload)
        {
            // TODO: To be removed. Comment added for testing purpose.
            var supplyFilterCriteria = JsonConvert.DeserializeObject<SupplyFilterCriteria>(payload["supplyFilterCriteria"].ToString());
            var loggedInUser = string.IsNullOrEmpty(payload["loggedInUser"].ToString()) ?
                null :
                payload["loggedInUser"].ToString();
            var pageNumber = string.IsNullOrEmpty(payload["pageNumber"].ToString()) ?
                null :
                Convert.ToInt32(payload["pageNumber"].ToString());
            var resourcesPerPage = string.IsNullOrEmpty(payload["resourcesPerPage"].ToString()) ?
                null :
                Convert.ToInt32(payload["resourcesPerPage"].ToString());
            var resourceAllocations = await _staffingService.GetResourcesStaffingAndCommitments(supplyFilterCriteria, loggedInUser);
            return Ok(resourceAllocations);
        }

        /// <summary>
        ///     Get Resources and their staffing on cases/opportunities
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>List of Resources</returns>
        [HttpPost]
        [Route("resourcesStaffingBySupplyGroup")]
        public async Task<IActionResult> GetFilteredResourcesGroupWithAllocations(dynamic payload)
        {
            var supplyGroupFilterCriteria = JsonConvert.DeserializeObject<SupplyGroupFilterCriteria>(payload["suppplyGroupFilterCriteria"].ToString());
            var loggedInUser = string.IsNullOrEmpty(payload["loggedInUser"].ToString()) ?
                null :
                payload["loggedInUser"].ToString();
            //var pageNumber = string.IsNullOrEmpty(payload["pageNumber"].ToString()) ?
            //    null :
            //    Convert.ToInt32(payload["pageNumber"].ToString());
            //var resourcesPerPage = string.IsNullOrEmpty(payload["resourcesPerPage"].ToString()) ?
            //    null :
            //    Convert.ToInt32(payload["resourcesPerPage"].ToString());
            var resourceAllocations = await _staffingService.GetFilteredResourcesGroupWithAllocations(supplyGroupFilterCriteria, loggedInUser);
            return Ok(resourceAllocations);
        }

        [HttpGet]
        [Route("notesAlert")]
        public async Task<IActionResult> GetNotesAlert(string employeeCode)
        {
            var notesAlert = await _staffingService.GetNotesAlert(employeeCode);
            return Ok(notesAlert);
        }


        [HttpGet]
        [Route("mostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode")]
        public async Task<IActionResult> GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(string employeeCode)
        {
            var recentNoteSharedWithGroups = await _staffingService.GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(employeeCode);
            return Ok(recentNoteSharedWithGroups);
        }

        /// <summary>
        /// Get Resources and their staffing on cases/opportunities
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("resourcesIncludingTerminatedStaffingBySearchString")]
        public async Task<IActionResult> GetResourcesIncludingTerminatedWithAllocationsBySearchString(string searchString, string loggedInUser = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var resourceAllocations = await _staffingService.GetResourcesIncludingTerminatedWithAllocationsBySearchString(searchString, loggedInUser, startDate, endDate);
            return Ok(resourceAllocations);
        }

        /// <summary>
        /// Get Resource Assignments for specific Opportunity
        /// </summary>
        /// <param name="pipelineId"></param>
        /// <param name="effectiveFromDate"></param>
        /// <param name="effectiveToDate"></param>
        [HttpGet]
        [Route("allocationsByPipelineId")]
        public async Task<IActionResult> GetResourceAllocationsByPipelineId(string pipelineId, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var allocations = await _staffingService.GetResourceAllocationsByPipelineId(pipelineId, effectiveFromDate, effectiveToDate);
            return Ok(allocations);
        }

        /// <summary>
        /// Get Resource Allocations to specific case
        /// </summary>
        /// <param name="oldCaseCode"></param>
        /// <param name="effectiveFromDate"></param>
        /// <param name="effectiveToDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("allocationsByCaseCode")]
        public async Task<IActionResult> GetResourceAllocationsByCaseCode(string oldCaseCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var allocations = await _staffingService.GetResourceAllocationsByCaseCode(oldCaseCode, effectiveFromDate, effectiveToDate);
            return Ok(allocations);
        }

        /// <summary>
        /// Get all security users with additional details
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAllSecurityUsersDetails")]
        public async Task<IActionResult> GetAllSecurityUsersDetails()
        {
            var securityUsersDetails = await _staffingService.GetAllSecurityUsersDetails();
            return Ok(securityUsersDetails);
        }

        /// <summary>
        /// Get all security users with additional details
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertResourceViewNote")]
        public async Task<IActionResult> UpsertResourceViewNote(dynamic payload)
        {
            ResourceViewNote resourceViewNote = JsonConvert.DeserializeObject<ResourceViewNote>(payload.ToString());
            var securityUsersDetails = await _staffingService.UpsertResourceViewNote(resourceViewNote);
            return Ok(securityUsersDetails);
        }

        [HttpPost]
        [Route("upsertResourceViewCD")]
        public async Task<IActionResult> upsertResourceViewCD(dynamic payload)
        {
            ResourceCD resourceViewCD = JsonConvert.DeserializeObject<ResourceCD>(payload.ToString());
            var upsertedCdDetaisls = await _staffingService.UpsertResourceRecentCD(resourceViewCD);
            return Ok(upsertedCdDetaisls);
        }

        [HttpPost]
        [Route("upsertResourceViewCommercialModel")]
        public async Task<IActionResult> upsertResourceViewCommercialModel(dynamic payload)
        {
            ResourceCommercialModel resourceViewCommercialModel = JsonConvert.DeserializeObject<ResourceCommercialModel>(payload.ToString());
            var upsertedCommercialModelDetails = await _staffingService.UpsertResourceCommercialModel(resourceViewCommercialModel);
            return Ok(upsertedCommercialModelDetails);
        }




        [HttpGet]
        [Route("getResourceStaffingResponsibeByEmployeeCode")]
        public async Task<IActionResult> GetResourceStaffingResponsibeDataByEmployeeCode(string employeeCode)
        {
            var resourceStaffingResponsibleData = await _staffingService.GetResourceStaffingResponsibeDataByEmployeeCode(employeeCode);
            return Ok(resourceStaffingResponsibleData);

        }
        /// <summary>
        /// Get all security users with additional details
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertPlanningCard")]
        public async Task<IActionResult> UpsertPlanningCard(dynamic payload)
        {
            var planningCard = JsonConvert.DeserializeObject<PlanningCard>(payload["planningCard"].ToString());
            var loggedInUser = string.IsNullOrEmpty(payload["loggedInUser"].ToString()) ?
                null :
                payload["loggedInUser"].ToString();

            var result = await _staffingService.UpsertPlanningCard(planningCard, loggedInUser);
            return Ok(result);
        }

        [HttpPost]
        [Route("resourcesCommitments")]

        public async Task<IActionResult> UpsertResourcesCommitments(dynamic payload)
        {
            var resourcesCommitments = JsonConvert.DeserializeObject<IList<Commitment>>(payload.ToString());
            var result = await _staffingService.UpsertResourcesCommitments(resourcesCommitments);
            return Ok(result);

        }

    }
}