using Basis.API.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Basis.API.Core.Helpers;

namespace Basis.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.PracticeAreaLookupRead)]
    public class PracticeAreaController : ControllerBase
    {
        public readonly IPracticeAreaService _practiceAreaService;

        public PracticeAreaController(IPracticeAreaService practiceAreaService)
        {
            _practiceAreaService = practiceAreaService;
        }

        /// <summary>
        /// get all practice areas from basis db
        /// </summary>
        /// <returns>collection of distinct practice areas</returns>
        [HttpGet("getAllPracticeArea")]
        public async Task<IActionResult> GetAllPracticeArea()
        {
            var result = await _practiceAreaService.GetAllPracticeArea();
            return Ok(result);
        }

        /// <summary>
        /// get affiliations by employee codes and practice areas
        /// </summary>
        /// <param name="payload">list of employee codes and practice area codes</param>
        /// <returns>collection of distinct employee codes with their corresponding practice areas</returns>
        [HttpPost("getAffiliationsByEmployeeCodesAndPracticeAreaCodes")]
        public async Task<IActionResult> GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(dynamic payload)
        {
            var employeeCodes = !string.IsNullOrEmpty(payload["listEmployeeCodes"]?.ToString()) ? payload["listEmployeeCodes"].ToString() :
                    !string.IsNullOrEmpty(payload["employeeCode"]?.ToString()) ? payload["employeeCode"].ToString()  : string.Empty;
            var practiceAreaCodes = !string.IsNullOrEmpty(payload["practiceAreaCodes"]?.ToString()) ? payload["practiceAreaCodes"].ToString() : string.Empty;
            var affiliationRoleCodes= !string.IsNullOrEmpty(payload["affiliationRoleCodes"]?.ToString()) ? payload["affiliationRoleCodes"].ToString() : string.Empty;
            var result = await _practiceAreaService.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes, affiliationRoleCodes);
            return Ok(result);
        }

        /// <summary>
        /// get all Affiliation Role names from basis db
        /// </summary>
        /// <returns>collection of distinct practice affiliation roles areas</returns>
        [HttpGet("getAffiliationRoles")]
        public async Task<IActionResult> GetAffiliationRoleList()
        {
            var result = await _practiceAreaService.GetAffiliationRoleList();
            return Ok(result);
        }

        /// <summary>
        /// Get Industry Practice Area for lookup list
        /// </summary>
        /// <returns></returns>
        [HttpGet("industryPracticeArea")]
        public async Task<IActionResult> GetIndustryPracticeAreaLookupList()
        {
            var industryPracticeAreas = await _practiceAreaService.GetIndustryPracticeAreaLookupList();
            return Ok(industryPracticeAreas);
        }

        /// <summary>
        /// Get Capability Practice Area for lookup list
        /// </summary>
        /// <returns></returns>
        [HttpGet("capabilityPracticeArea")]
        public async Task<IActionResult> GetCapabilityPracticeAreaLookupList()
        {
            var capabilityPracticeAreas = await _practiceAreaService.GetCapabilityPracticeAreaLookupList();
            return Ok(capabilityPracticeAreas);
        }

    }
}
