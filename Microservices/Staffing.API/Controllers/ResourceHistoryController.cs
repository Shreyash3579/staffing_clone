using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class ResourceHistoryController : ControllerBase
    {
        private readonly IResourceHistoryService _resourceHistoryService;
        public ResourceHistoryController(IResourceHistoryService resourceHistoryService)
        {
            _resourceHistoryService = resourceHistoryService;
        }
        /// <summary>
        /// Get historical staffing data for the past one year of a specific employee.
        /// </summary>
        /// <param name="employeeCode"></param>
        [HttpGet]
        public async Task<IActionResult> GetResourceHistoryData(string employeeCode)
        {
            var resourceHistory = await _resourceHistoryService.GetHistoricalStaffingAllocationsForEmployee(employeeCode);
            return Ok(resourceHistory);
        }

        //commented on 06-jun-23 as it is not being used from UI anymore
        ///// <summary>
        ///// Get future staffing data for a specific employee.
        ///// </summary>
        ///// <param name="employeeCode"></param>
        //[HttpGet]
        //[Route("futureallocations")]
        //public async Task<IActionResult> GetFutureAllocationsForResource(string employeeCode)
        //{
        //    var resourceHistory = await _resourceHistoryService.GetHistoricalStaffingAllocationsForEmployee(employeeCode);
        //    return Ok(resourceHistory.Where(x => x.EndDate >= DateTime.Today));
        //}
    }
}