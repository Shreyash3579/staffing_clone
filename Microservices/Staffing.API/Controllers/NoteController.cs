using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Linq;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;
using System;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    //[Authorize(Policy = Constants.Policy.StaffingApiLookupRead)]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }

        /// <summary>
        /// Get all Resource Profile or Resource Tab/Allocation Notes created for resource(s) that are private to or shared with the loggedInuser
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        ///      {
        ///         "employeeCodes":"37995",
        ///         "loggedInUser": "39209", (gets all notes that are private to this user or shared with this user)
        ///         "noteTypeCode":"RP"   ("RP" for resource profile notes or "RA" for resource notes created for allocations)      
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("getResourceViewNotes")]
        public async Task<IActionResult> GetResourceViewNotes(dynamic payload)
        {
            var employeeCodes = $"{payload["employeeCodes"]}";
            var loggedInUser = $"{payload["loggedInUser"]}";
            var noteTypeCode = $"{payload["noteTypeCode"]}";

            var resourceNotes = await _noteService.GetResourceViewNotes(employeeCodes, loggedInUser, noteTypeCode);
            return Ok(resourceNotes);
        }

        [HttpPost]
        [Route("upsertResourceViewNote")]
        public async Task<IActionResult> UpsertResourceViewNote(dynamic payload)
        {
            var resourceViewNote = payload != null ? JsonConvert.DeserializeObject<ResourceViewNote>(payload.ToString()) : Enumerable.Empty<ResourceViewNote>();
            var insertedResourceNote = await _noteService.UpsertResourceViewNote(resourceViewNote);
            return Ok(insertedResourceNote);
        }

        [HttpDelete]
        [Route("deleteResourceViewNotes")]
        public async Task<IActionResult> DeleteResourceViewNotes(string idsToDelete, string lastUpdatedBy)
        {
            var insertedResourceNote = await _noteService.DeleteResourceViewNotes(idsToDelete, lastUpdatedBy);
            return Ok(insertedResourceNote);
        }

        /// <summary>
        /// Get all Case/Opportunity/Planning Card Notes that are private to or shared with the loggedInuser
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        ///      {
        ///         "oldCaseCodes":"U8UB",
        ///         "pipelineIds": null,
        ///         "planningCardIds": null,
        ///         "loggedInUser": "39209" (gets all notes that are private to this user or shared with this user)
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("getCaseViewNotes")]
        public async Task<IActionResult> GetCaseViewNotes(dynamic payload)
        {
            var oldCaseCodes = payload != null && payload.ContainsKey("oldCaseCodes") ? $"{payload["oldCaseCodes"]}" : null;
            var pipelineIds = payload != null && payload.ContainsKey("pipelineIds") ? $"{payload["pipelineIds"]}" : null;
            var planningCardIds = payload != null && payload.ContainsKey("planningCardIds") ? $"{payload["planningCardIds"]}" : null;
            var loggedInUser = payload != null && payload.ContainsKey("loggedInUser") ? $"{payload["loggedInUser"]}" : null;

            var caseNotes = await _noteService.GetCaseViewNotes(oldCaseCodes, pipelineIds, planningCardIds, loggedInUser);
            return Ok(caseNotes);
        }

        /// <summary>
        /// Get latest created/updated Case/Opportunity/Planning Card Notes that are private to or shared with the loggedInuser
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        ///      {
        ///         "oldCaseCodes":"U8UB",
        ///         "pipelineIds": null,
        ///         "planningCardIds": null,
        ///         "loggedInUser": "39209" (gets all notes that are private to this user or shared with this user)
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("getLatestCaseViewNotes")]
        public async Task<IActionResult> GetLatestCaseViewNotes(dynamic payload)
        {
            var oldCaseCodes = payload != null && payload.ContainsKey("oldCaseCodes") ? $"{payload["oldCaseCodes"]}" : null;
            var pipelineIds = payload != null && payload.ContainsKey("pipelineIds") ? $"{payload["pipelineIds"]}" : null;
            var planningCardIds = payload != null && payload.ContainsKey("planningCardIds") ? $"{payload["planningCardIds"]}" : null;
            var loggedInUser = payload != null && payload.ContainsKey("loggedInUser") ? $"{payload["loggedInUser"]}" : null;

            var caseNotes = await _noteService.GetLatestCaseViewNotes(oldCaseCodes, pipelineIds, planningCardIds, loggedInUser);
            return Ok(caseNotes);
        }

        [HttpPost]
        [Route("upsertCaseViewNote")]
        public async Task<IActionResult> UpsertCaseViewNote(dynamic payload)
        {
            var caseViewNote = payload != null ? JsonConvert.DeserializeObject<CaseViewNote>(payload.ToString()) : Enumerable.Empty<CaseViewNote>();
            var upsertedCaseViewNote = await _noteService.UpsertCaseViewNote(caseViewNote);
            return Ok(upsertedCaseViewNote);
        }

        [HttpDelete]
        [Route("deleteCaseViewNotes")]
        public async Task<IActionResult> DeleteCaseViewNotes(string idsToDelete, string lastUpdatedBy)
        {
            var deletedCaseViewNotes = await _noteService.DeleteCaseViewNotes(idsToDelete, lastUpdatedBy);
            return Ok(deletedCaseViewNotes);
        }

        [HttpGet]
        [Route("notesalert")]
        public async Task<IActionResult> GetNotesAlert(string employeeCode)
        {
            var notesAlert = await _noteService.GetNotesAlert(employeeCode);
            return Ok(notesAlert);
        }

        [HttpGet]
        [Route("mostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode")]
        public async Task<IActionResult> GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(string employeeCode)
        {
            var recentNoteSharedWithGroups = await _noteService.GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(employeeCode);
            return Ok(recentNoteSharedWithGroups);
        }
        [HttpPost]
        [Route("updateNotesAlertStatus")]
        public async Task<IActionResult> UpdateNotesAlertStaus(string employeeCode)
        {
            await _noteService.UpdateNotesAlertStaus(employeeCode);
            return Ok();
        }

        [HttpGet]
        [Route("getCommercialModelList")]
        public async Task<IActionResult> GetCommercialModelList()
        {
            var commercialModel = await _noteService.GetCommercialModelList();
            return Ok(commercialModel);
        }

        [HttpPost]
        [Route("getResourceCommercialModel")]
        public async Task<IActionResult> GetResourceCommercialModel(dynamic payload)
        {
            var employeeCodes = $"{payload["employeeCodes"]}";

            var resourceCommercialModel = await _noteService.GetResourceCommercialModel(employeeCodes);
            return Ok(resourceCommercialModel);
        }

        [HttpPost]
        [Route("getResourceRecentCD")]
        public async Task<IActionResult> GetResourceRecentCD(dynamic payload)
        {
            var employeeCodes = $"{payload["employeeCodes"]}";

            var recentCD = await _noteService.GetResourceRecentCD(employeeCodes);
            return Ok(recentCD);
        }


        [HttpPost]
        [Route("upsertResourceRecentCD")]
        public async Task<IActionResult> UpsertResourceRecentCD(dynamic payload)
        {
            var resourceCD = payload != null ? JsonConvert.DeserializeObject<ResourceCD>(payload.ToString()) : Enumerable.Empty<ResourceCD>();
            var upsertedResourceCD = await _noteService.UpsertResourceRecentCD(resourceCD);
            return Ok(upsertedResourceCD);
        }

        [HttpGet]
        [Route("getRecentCDList")]
        public async Task<IActionResult> GetRecentCDList()
        {
            var skills = await _noteService.GetRecentCDList();
            return Ok(skills);
        }

        [HttpPost]
        [Route("upsertResourceCommercialModel")]
        public async Task<IActionResult> UpsertResourceSkills(dynamic payload)
        {
            var resourceSkills = payload != null ? JsonConvert.DeserializeObject < ResourceCommercialModel>(payload.ToString()) : Enumerable.Empty<ResourceCommercialModel>();
            var upsertedResourceSkills = await _noteService.UpsertResourceCommercialModel(resourceSkills);
            return Ok(upsertedResourceSkills);
        }


        [HttpDelete]
        [Route("deleteResourceViewCD")]
        public async Task<IActionResult> DeleteResourceViewCD(string idsToDelete, string lastUpdatedBy)
        {
            var insertedResourceNote = await _noteService.DeleteResourceViewCD(idsToDelete, lastUpdatedBy);
            return Ok(insertedResourceNote);
        }

        [HttpDelete]
        [Route("deleteResourceCommercialModel")]
        public async Task<IActionResult> DeleteResourceCommercialModel(string idsToDelete, string lastUpdatedBy)
        {
            var insertedResourceCommercialModel = await _noteService.DeleteResourceCommercialModel(idsToDelete, lastUpdatedBy);
            return Ok(insertedResourceCommercialModel);
            
        }
        /// <summary>
        /// Get all Resource Notes modified after the selecetd date
        /// </summary>
        /// <param name="lastupdatedAfter"></param>
        /// <param name="noteTypeCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getResourceNotesByLastUpdatedDate")]
        public async Task<IActionResult> GetResourceNotesByLastUpdatedDate(DateTime lastupdatedAfter, string noteTypeCode = "RP")
        {
            var resourceNotes = await _noteService.GetResourceNotesByLastUpdatedDate(lastupdatedAfter, noteTypeCode);
            return Ok(resourceNotes);
        }
    }
}