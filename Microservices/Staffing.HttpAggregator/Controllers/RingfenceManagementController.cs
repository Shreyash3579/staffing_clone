using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RingfenceManagementController : ControllerBase
    {
        private readonly IRingfenceManagementService _ringfenceManagementService;

        public RingfenceManagementController(IRingfenceManagementService ringfenceManagementService)
        {
            _ringfenceManagementService = ringfenceManagementService;
        }

        /// <summary>
        /// gets count of resources available in specific ringfence and office as of today
        /// </summary>
        /// <remarks>
        /// Sample Request: {"officeCodes": "115,125,128,110,160,539,127,152,153,542,511,165,400,112,504,521,150,151,120,116,508,524,176,395,170,175,535,177,500",    "commitmentTypeCodes": "P"}
        /// </remarks>
        /// <param name="payload">comma separated list of officeCodes, ringfence codes</param>
        /// <returns></returns>
        [HttpPost]
        [Route("totalResourcesByOfficesAndRingfences")]
        public async Task<IActionResult> GetTotalResourcesByOfficesAndRingfences(dynamic payload)
        {
            var officeCodes = payload["officeCodes"].ToString();
            var commitmentTypeCodes = payload["commitmentTypeCodes"].ToString();
            var data = await _ringfenceManagementService.GetTotalResourcesByOfficesAndRingfences(officeCodes, commitmentTypeCodes);
            return Ok(data);
        }

        /// <summary>
        /// Gets audit log of what changes were made to ringfence management data 
        /// </summary>
        /// <param name="officeCode">office code for which to get log</param>
        /// <param name="commitmentTypeCode">Ringfence Code (P for Peg)</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ringfenceAuditLogByOfficeAndCommitmentCode")]
        public async Task<IActionResult> GetRingfenceAuditLogByOfficeAndCommitmentCode(string officeCode, string commitmentTypeCode)
        {
            var data = await _ringfenceManagementService.GetRingfenceAuditLogByOfficeAndCommitmentCode(officeCode, commitmentTypeCode);
            return Ok(data);
        }
    }
}
