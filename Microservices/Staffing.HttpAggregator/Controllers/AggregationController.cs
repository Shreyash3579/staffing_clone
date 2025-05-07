using Microsoft.AspNetCore.Mvc;
using Staffing.HttpAggregator.Contracts.Services;
using System;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AggregationController : ControllerBase
    {
        private readonly IAggregationService _aggregationService;

        public AggregationController(IAggregationService aggregationService)
        {
            _aggregationService = aggregationService;
        }

        /// <summary>
        /// Get Commitments of Resource
        /// </summary>
        /// <param name="employeeCode">employee code of resource to get data for</param>
        /// <param name="effectiveFromDate">date in which commitment should lie</param>
        /// <param name="effectiveToDate">date in which commitment should lie</param>
        /// <returns></returns>
        [HttpGet("resourcecommitments")]
        public async Task<IActionResult> GetAllCommitmentsForEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var resourceCommitments = await _aggregationService.GetAllCommitmentsForEmployee(employeeCode, effectiveFromDate, effectiveToDate);
            return Ok(resourceCommitments);
        }

        /// <summary>
        /// Get aggregated info of Resource along with its Practice Affiliations
        /// </summary>
        /// <param name="employeeCode">employee code of resource to get data for</param>
        /// <returns></returns>
        [HttpGet("employeeInfoWithGxcAffiliations")]
        public async Task<IActionResult> GetEmployeeInfoWithPracticeAffiliations(string employeeCode)
        {
            var employeeInfoWithGxcAffiliations = await _aggregationService.GetEmployeeInfoWithGxcAffiliations(employeeCode);
            return Ok(employeeInfoWithGxcAffiliations);
        }
    }
}