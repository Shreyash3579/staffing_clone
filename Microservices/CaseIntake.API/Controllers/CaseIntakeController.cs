using CaseIntake.API.Contracts.Services;
using CaseIntake.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseIntake.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CaseIntakeController : ControllerBase
    {
        private readonly ICaseIntakeService _service;
        public CaseIntakeController(ICaseIntakeService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("leadershipDetails")]
        public async Task<IActionResult> GetLeadershipDetailsForCaseOrOpportunityId(dynamic payload)
        {
            var oldCaseCode = string.IsNullOrEmpty($"{payload.oldCaseCode}") ? null : $"{payload.oldCaseCode}";
            var opportunityId = string.IsNullOrEmpty($"{payload["opportunityId"]}") ? null : $"{payload["opportunityId"]}";
            var planningCardId = string.IsNullOrEmpty($"{payload["planningCardId"]}") ? null : $"{payload["planningCardId"]}";

            var leadershipDetails = await _service.GetLeadershipDetailsForCaseOrOpportunityId(oldCaseCode, opportunityId, planningCardId);
            return Ok(leadershipDetails);
        }


        [HttpPost]
        [Route("caseIntakeDetails")]
        public async Task<IActionResult> GetCaseIntakeDetailsForCaseOrOpportunityId(dynamic payload)
        {
            var oldCaseCode = string.IsNullOrEmpty($"{payload.oldCaseCode}") ? null : $"{payload.oldCaseCode}";
            var opportunityId = string.IsNullOrEmpty($"{payload["opportunityId"]}") ? null : $"{payload["opportunityId"]}";
            var planningCardId = string.IsNullOrEmpty($"{payload["planningCardId"]}") ? null : $"{payload["planningCardId"]}";

            var caseIntakeDetails = await _service.GetCaseIntakeDetailsByCaseCodeOrOpportunityId(oldCaseCode, opportunityId, planningCardId);
            return Ok(caseIntakeDetails);
        }

        [HttpPost]
        [Route("upsertLeadershipDetails")]
        public async Task<IActionResult> UpsertLeadershipDetails(dynamic payload)
        {
            var leadershipDetailsList = JsonConvert.DeserializeObject<IEnumerable<CaseIntakeLeadership>>(payload.ToString());
            var upsertedData = await _service.UpsertLeadershipDetails(leadershipDetailsList);
            return Ok(upsertedData);
        }

        [HttpDelete]
        [Route("deleteLeadershipById")]
        public async Task<IActionResult> DeleteLeadershipById([FromBody] CaseIntakeBasicDetail deleteLeadershipDetail)
        {
            await _service.DeleteLeadershipById(deleteLeadershipDetail);
            return Ok();
        }


        [HttpDelete]
        [Route("deleteLeadershipByCaseRoleCode")]
        public async Task<IActionResult> DeleteLeadershipByCaseRoleCode([FromBody] CaseIntakeBasicDetail deleteLeadershipDetail)
        {
            await _service.DeleteLeadershipByCaseRoleCode(deleteLeadershipDetail);
            return Ok();
        }

        [HttpPost]
        [Route("upsertCaseIntakeDetails")]
        public async Task<IActionResult> UpsertCaseIntakeDetails(dynamic payload)
        {
            var caseIntakeDetails = JsonConvert.DeserializeObject<CaseIntakeDetail>(payload.ToString());
            var upsertedData = await _service.UpsertCaseIntakeDetails(caseIntakeDetails);
            return Ok(upsertedData);
        }

        [HttpPost]
        [Route("getRoleAndWorkStreamDetails")]
        public async Task<IActionResult> GetRoleDetailsForCaseOrOpportunityId(dynamic payload)
        {
            var oldCaseCode = string.IsNullOrEmpty($"{payload.oldCaseCode}") ? null : $"{payload.oldCaseCode}";
            var opportunityId = string.IsNullOrEmpty($"{payload["opportunityId"]}") ? null : $"{payload["opportunityId"]}";
            var planningCardId = string.IsNullOrEmpty($"{payload["planningCardId"]}") ? null : $"{payload["planningCardId"]}";
            
            var leadershipDetails = await _service.GetRoleAndWorkstreamByCaseCodeOrOpportunityId(oldCaseCode, opportunityId, planningCardId);
            return Ok(leadershipDetails);
        }

        [HttpPost]
        [Route("upsertRoles")]
        public async Task<IActionResult> UpsertRoleDetailsForCaseOrOpportunityId(dynamic payload)
        {
            var roleDetails = JsonConvert.DeserializeObject<CaseIntakeRoleDetails[]>(payload.ToString());
            // if we want to implement delete from here also, can send oldCaseCode or OpportunityId, for now we dont have option to delete role, 
            // we have option to delete once workstream is created
            var upsertedRoleDetails = await _service.UpsertRoleDetailsByCaseCodeOrOpportunityId(roleDetails, null, null, null);

            return Ok(upsertedRoleDetails);
        }


        [HttpPost]
        [Route("upsertWorkstreamAndRole")]
        public async Task<IActionResult> UpsertCaseIntakeRoleAndWorkstream(dynamic payload)
        {

            var workstreamAndRoleDetails = JsonConvert.DeserializeObject<CaseIntakeRoleAndWorkstream>(payload.ToString());

            var upsertedWorkstreamAndRoleDetails = await _service.UpsertRoleAndWorkStreamDetails(workstreamAndRoleDetails);
            return Ok(upsertedWorkstreamAndRoleDetails);

        }

        [HttpDelete]
        [Route("deleteRolesByIds")]
        public async Task<IActionResult> DeleteRolesByIds([FromBody] CaseIntakeBasicDetail rolesToBedeleted)
        {
            await _service.DeleteRoleByIds(rolesToBedeleted);
            return Ok();
        }


        [HttpDelete]
        [Route("deleteWorkstreamsByIds")]
        public async Task<IActionResult> DeleteWorkstreamsByIds([FromBody] CaseIntakeBasicDetail workstreamsToBeDeleted)
        {
            await _service.DeleteWorkStreamByIds(workstreamsToBeDeleted);
            return Ok();
        }


        [HttpPost]
        [Route("getMostRecentUpdateInCaseIntake")]
        public async Task<IActionResult> GetMostRecentUpdateInCaseIntake(dynamic payload)
        {
            var oldCaseCode = string.IsNullOrEmpty($"{payload.oldCaseCode}") ? null : $"{payload.oldCaseCode}";
            var opportunityId = string.IsNullOrEmpty($"{payload["opportunityId"]}") ? null : $"{payload["opportunityId"]}";
            var planningCardId = string.IsNullOrEmpty($"{payload["planningCardId"]}") ? null : $"{payload["planningCardId"]}";

            var leadershipDetails = await _service.GetMostRecentUpdateInCaseIntake(oldCaseCode, opportunityId, planningCardId);
            return Ok(leadershipDetails);
        }

        [HttpGet]
        [Route("expertiseRequirementList")]
        public async Task<IActionResult> GetExpertiseRequirementList()
        {
            var list = await _service.GetExpertiseRequirementList();
            return Ok(list);
        }

        [HttpPost]
        [Route("upsertexpertiseRequirementList")]
        public async Task<IActionResult> UpsertExpertiseRequirementList(dynamic payload)
        {
            var expertiseRequirement = JsonConvert.DeserializeObject<CaseIntakeExpertise>(payload.ToString());

            var list = await _service.UpsertExpertiseRequirementList(expertiseRequirement);
            return Ok(list);
        }

        [HttpGet]
        [Route("caseIntakeAlert")]
        public async Task<IActionResult> GetCaseIntakeAlert(string employeeCode)
        {
            var notesAlert = await _service.GetCaseIntakeAlert(employeeCode);
            return Ok(notesAlert);
        }

        [HttpGet]
        [Route("getPlacesTypeAhead")]
        public async Task<IActionResult> GetPlacesForTypeAhead(string searchString)
        {
            var list = await _service.GetPlacesForTypeAhead(searchString);
            return Ok(list);
        }


    }
}