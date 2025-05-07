using CCM.API.Contracts.Services;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Query;
using CCM.API.Core.Helpers;

namespace CCM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [EnableQuery]
    public class FinAPIController : ControllerBase
    {

        private readonly IFinApiService _service;

        public FinAPIController(IFinApiService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get Flat list of offices within the specified region or cluster
        /// </summary>
        /// <returns></returns>
        [HttpGet("officesFlatListByRegionOrCluster")]
        [Authorize(Policy = Constants.Policy.CCMAllAccess)]
        public async Task<IActionResult> GetOfficesFlatListByRegionOrCluster(int clusterRegionCode)
        {
            var caseRoleTypes = await _service.GetOfficesFlatListByRegionOrCluster(clusterRegionCode);
            return Ok(caseRoleTypes);
        }

        /// <summary>
        /// Get flat lookup list for active offices
        /// </summary>
        /// <returns></returns>
        
        [HttpGet]
        [Route("officeList")]
        [Authorize(Policy = Constants.Policy.OfficeLookupReadAccess)]

        public async Task<IActionResult> GetOfficeList()
        {
            var accessibleOffices = JWTHelper.GetAccessibleOffices(HttpContext)?.ToList();
            var offices = await _service.GetOfficeList(accessibleOffices);

            return Ok(offices);
        }

        /// <summary>
        /// Get office lookup hierarchy list for active offices
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("officeHierarchy")]
        [Authorize(Policy = Constants.Policy.OfficeLookupReadAccess)]
        public async Task<IActionResult> GetOfficeHierarchy()
        {
            var accessibleOffices = JWTHelper.GetAccessibleOffices(HttpContext)?.ToList();
            var result = await _service.GetOfficeHierarchy(accessibleOffices);
            return Ok(result);
        }

        /// <summary>
        /// Get office hierarchy list for selected Offices
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("officeHierarchyByOffices")]
        [Authorize(Policy = Constants.Policy.CCMAllAccess)]
        public async Task<IActionResult> GetOfficeHierarchyByOffices(string officeCodes)
        {

            var result = await _service.GetOfficeHierarchyByOffices(officeCodes);
            return Ok(result);
        }

        /// <summary>
        /// Get all office data from REV to be saved in database for tableau
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("revOfficeList")]
        [Authorize(Policy = Constants.Policy.CCMAllAccess)]
        public async Task<IActionResult> GetOfficeListFromFinance()
        {
            var result = await _service.GetOfficeListFromFinance();
            return Ok(result);
        }

        /// <summary>
        /// Get all historical bill rates for selected offices
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("billRateByOffices")]
        [Authorize(Policy = Constants.Policy.CCMAllAccess)]
        public async Task<IActionResult> GetBillRateByOffices(string officeCodes)
        {
            var result = await _service.GetBillRateByOffices(officeCodes);
            return Ok(result);
        }

        /// <summary>
        /// Get all historical bill rates for worldwide
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("billRates")]
        [Authorize(Policy = Constants.Policy.CCMAllAccess)]
        public async Task<IActionResult> GetBillRates()
        {
            var result = await _service.GetBillRates();
            return Ok(result);
        }

    }
}