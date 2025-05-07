using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;
using Staffing.API.Core.Services;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingApiLookupRead)]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _service;

        public LookupController(ILookupService service)
        {
            _service = service;
        }

        /// <summary>
        ///     Get Investments for lookup
        /// </summary>
        [HttpGet("investmentTypes")]
        public async Task<IActionResult> GetInvestmentCategoryLookupList()
        {
            var investmentCategories = await _service.GetInvestmentCategoryLookupList();
            return Ok(investmentCategories);
        }

        /// <summary>
        ///     Get Investments for lookup
        /// </summary>
        [HttpGet("caseRoleTypes")]
        public async Task<IActionResult> GetCaseRoleTypeLookupList()
        {
            var caseRoleTypes = await _service.GetCaseRoleTypeLookupList();
            return Ok(caseRoleTypes);
        }

        /// <summary>
        /// Get staffable as look up data
        /// </summary>
        /// <returns></returns>
        [HttpGet("staffableAsTypes")]
        public async Task<IActionResult> GetStaffableAsTypeLookupList()
        {
            var staffableAsTypes = await _service.GetStaffableAsTypeLookupList();
            return Ok(staffableAsTypes);
        }

        /// <summary>
        /// Get Case Planning Board Bucket for lookup
        /// </summary>
        /// <returns></returns>
        [HttpGet("casePlanningBucketsByEmployee")]
        public async Task<IActionResult> GetCasePlanningBoardBucketLookupListByEmployee(string employeeCode)
        {
            var casePlanningBoardBuckets = await _service.GetCasePlanningBoardBucketLookupListByEmployee(employeeCode);
            return Ok(casePlanningBoardBuckets);
        }

        [HttpGet("userPersonaTypes")]
        public async Task<IActionResult> GetuserPersonaTypeLookupList()
        {
            var casePlanningBoardBuckets = await _service.GetUserPersonaTypeLookupList();
            return Ok(casePlanningBoardBuckets);
        }

        [HttpGet("securityRoles")]
        public async Task<IActionResult> GetSecurityRoles()
        {
            var securityRoles = await _service.GetSecurityRoles();
            return Ok(securityRoles);
        }


        [HttpGet("securityFeatures")]
        public async Task<IActionResult> GetSecurityFeatures()
        {
            var securityFeatures = await _service.GetSecurityFeatures();
            return Ok(securityFeatures);
        }

        /// <summary>
        /// Get all  preferences options look up data for PD, Priorites, Travel etc.
        /// </summary>
        /// <returns>staffing preferences object containing data about employee's staffing needs </returns>
        [HttpGet("staffingPreferences")]
        public async Task<IActionResult> GetStaffingPreferences()
        {
            var securityRoles = await _service.GetStaffingPreferences();
            return Ok(securityRoles);
        }

        /// <summary>
        /// Get all SKU Terms
        /// </summary>
        /// <returns>Saved Master Data for SKU Terms</returns>
        [HttpGet]
        [Route("getskutermlist")]
        public async Task<IActionResult> GetSKUTermList()
        {
            var skuTermList = await _service.GetSKUTermList();
            return Ok(skuTermList);
        }

        [HttpGet]
        [Route("userPreferences")]
        public async Task<IActionResult> GetUserPreferences(string employeeCode)
        {
            var userPreferences = await _service.GetUserPreferences(employeeCode);
            return Ok(userPreferences);
        }

        /// <summary>
        ///  Get commitments for lookup
        /// </summary>
        [HttpGet("getcommitmenttypelist")]
        public async Task<IActionResult> GetCommitmentTypeLookupList(bool? showHidden)
        {
            var commitmentTypes = await _service.GetCommitmentTypeLookupList(showHidden);
            return Ok(commitmentTypes);
        }

        /// <summary>
        ///  Get commitments for lookup
        /// </summary>
        [HttpGet("getcommitmentTypeReasonlist")]
        public async Task<IActionResult> GetCommitmentTypeReasonLookupList()
        {
            var commitmentTypes = await _service.GetCommitmentTypeReasonLookupList();
            return Ok(commitmentTypes);
        }
    }
}