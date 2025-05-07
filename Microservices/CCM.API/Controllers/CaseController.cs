using CCM.API.Contracts.Services;
//using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using CCM.API.Core.Helpers;

namespace CCM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableQuery]
    [Authorize(Policy = Constants.Policy.CaseInfoReadAccess)]
    public class CaseController : ControllerBase
    {
        private readonly ICaseService _caseService;

        public CaseController(ICaseService caseService)
        {
            _caseService = caseService;
        }

        /// <summary>
        /// Get new demand cases for multiples offices active in specified date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="officeCodes">comma separated list of office codes</param>
        /// <param name="caseTypeCodes">comma separated list of case type codes</param>
        /// <param name="clientCodes">[Optional] comma separated list of client codes</param>
        /// <returns>cases that start between the given dates filtered by offices, case types and optional clients</returns>
        [HttpGet("newDemandCasesByOffices")]
        public async Task<IActionResult> GetNewDemandCasesByOffices(DateTime startDate, DateTime endDate, string officeCodes, string caseTypeCodes, string clientCodes)
        {
            var cases = await _caseService.GetNewDemandCasesByOffices(startDate, endDate, officeCodes, caseTypeCodes, clientCodes);
            return Ok(cases);
        }

        /// <summary>
        /// Get active cases other than new demand for multiples offices active in specified date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="officeCodes">comma separated list of office codes</param>
        /// <param name="caseTypeCodes">comma separated list of case type codes</param>
        /// <param name="clientCodes">[Optional] comma separated list of client codes</param>
        /// <returns>cases that start or are in progress between the given dates filtered by offices, case types and optional clients</returns>
        [HttpGet("activeCasesExceptNewDemandsByOffices")]
        public async Task<IActionResult> GetActiveCasesExceptNewDemandsByOffices(DateTime startDate, DateTime endDate, string officeCodes, string caseTypeCodes, string clientCodes)
        {
            var cases = await _caseService.GetActiveCasesExceptNewDemandsByOffices(startDate, endDate, officeCodes, caseTypeCodes, clientCodes);
            return Ok(cases);
        }

        /// <summary>
        ///     Get case details for single case by old case code
        /// </summary>
        /// <param name="oldCaseCode">old case code of a case</param>
        [HttpGet("caseDetailsByCaseCode")]
        public async Task<IActionResult> GetCaseDetailsByCaseCode(string oldCaseCode)
        {
            var cases = await _caseService.GetCaseDetailsByCaseCodes(oldCaseCode);
            return Ok(cases.FirstOrDefault());
        }

        /// <summary>
        ///     Get case details for by multiple old case code
        /// </summary>
        [HttpPost("caseDetailsByCaseCodes")]
        public async Task<IActionResult> GetCaseDetailsByCaseCodes([FromBody] string oldCaseCodes)
        {
            var cases = await _caseService.GetCaseDetailsByCaseCodes(oldCaseCodes);
            return Ok(cases);
        }

        /// <summary>
        ///     Get basic case data like case name, client name along with taxonomy data like case primary industry and capability
        /// </summary>
        /// <param name="oldCaseCodeList">comma separated list of case codes</param>
        [HttpPost("caseDataBasicByCaseCodes")]
        public async Task<IActionResult> GetCaseDataBasicByCaseCodes([FromBody] string oldCaseCodeList)
        {
            var cases = await _caseService.GetCasesWithTaxonomiesByCaseCodes(oldCaseCodeList);
            return Ok(cases);
        }

        /// <summary>
        ///     Get case details for one or more cases
        /// </summary>
        /// <param name="oldCaseCodes">comma seperated list of one or more case codes</param>
        [HttpPost("caseDataByCaseCodes")]
        public async Task<IActionResult> GetCaseDataByCaseCodes([FromBody] string oldCaseCodes)
        {
            var cases = await _caseService.GetCaseDataByCaseCodes(oldCaseCodes);
            return Ok(cases);
        }

        /// <summary>
        ///     Get cases for typeahead
        /// </summary>
        /// <param name="searchString">Search cases by case name or case Code or client name</param>
        [HttpGet("typeaheadCases")]
        public async Task<IActionResult> GetCasesForTypeahead(string searchString)
        {
            var cases = await _caseService.GetCasesForTypeahead(searchString);
            return Ok(cases);
        }

        /// <summary>
        ///     Get cases ending soon
        /// </summary>
        /// <param name="caseEndsBeforeNumberOfDays"></param>
        [HttpGet("casesEndingSoon")]
        public async Task<IActionResult> GetCasesEndingBySpecificDate(int caseEndsBeforeNumberOfDays)
        {
            var cases = await _caseService.GetCasesEndingBySpecificDate(caseEndsBeforeNumberOfDays);
            return Ok(cases);
        }

        /// <summary>
        ///     Get basic case data like case name, client name along with taxonomy data like case primary industry and capability
        /// </summary>
        /// <param name="oldCaseCodeList">comma separated list of case codes</param>
        [HttpPost("casesWithTaxonomiesByCaseCodes")]
        public async Task<IActionResult> GetCasesWithTopLevelTaxonomiesByCaseCodes([FromBody] string oldCaseCodeList)
        {
            var cases = await _caseService.GetCasesWithTaxonomiesByCaseCodes(oldCaseCodeList);
            return Ok(cases);
        }

        

        /// <summary>
        /// Get Cases active after specified date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("casesActiveAfterSpecifiedDate")]
        public async Task<IActionResult> GetCasesActiveAfterSpecifiedDate(DateTime? date)
        {
            var cases = await _caseService.GetCasesActiveAfterSpecifiedDate(date);
            return Ok(cases);
        }

        /// <summary>
        /// Gets all the cases whose start date or end date have been updated in CCM
        /// </summary>
        /// <param name="columnName">Its value can be "startdate" or "enddate"</param>
        /// <param name="lastPollDateTime">It's value can be "null" - to get updates after NOW or specific datetime - if we want updates after that time</param>
        /// <returns></returns>
        [HttpGet("casesWithStartOrEndDateUpdatedInCCM")]
        public async Task<IActionResult> GetCasesWithStartOrEndDateUpdatedInCCM(string columnName, DateTime? lastPollDateTime)
        {
            var cases = await _caseService.GetCasesWithStartOrEndDateUpdatedInCCM(columnName, lastPollDateTime);
            return Ok(cases);
        }

        /// <summary>
        /// Gets all the records from case_master and case_master_history that have been updated in CCM after a specific datetime
        /// </summary>
        /// <param name="lastPolledDateTime">Pass null to get all the data from case_master and case_master_history</param>
        /// <returns></returns>
        [HttpGet("caseMasterAndCaseMasterHistoryChangesSinceLastPolled")]
        public async Task<IActionResult> GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(DateTime? lastPolledDateTime)
        {
            var caseMasterAndCaseMasterHistoryDataChanges = await _caseService.GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(lastPolledDateTime);
            return Ok(caseMasterAndCaseMasterHistoryDataChanges);
        }

        
        /// <summary>
        /// Get case additional info for analytics
        /// </summary>
        /// <returns></returns>
        [HttpGet("caseAdditionalInfo")]
        public async Task<IActionResult> GetCaseAdditionalInfo(DateTime? lastUpdated)
        {
            var caseAdditionalInfo = await _caseService.GetCaseAdditionalInfo(lastUpdated);
            return Ok(caseAdditionalInfo);
        }
    }
}