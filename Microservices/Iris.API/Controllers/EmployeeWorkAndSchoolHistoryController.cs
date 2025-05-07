using Iris.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Iris.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeWorkAndSchoolHistoryController : ControllerBase
    {
        private readonly IEmployeeWorkAndSchoolHistoryService _employeeWorkAndSchoolHistoryService;
        public EmployeeWorkAndSchoolHistoryController(IEmployeeWorkAndSchoolHistoryService employeeWorkAndSchoolHistoryService)
        {
            _employeeWorkAndSchoolHistoryService = employeeWorkAndSchoolHistoryService;
        }
        /// <summary>
        /// Get employee's work and school history
        /// </summary>
        /// <param name="employeeCode"></param>
        [HttpGet("workSchoolHistoryByEmployeeCode")]
        public async Task<IActionResult> GetWorkAndShoolHistoryByEmployeeCode(string employeeCode)
        {
            var employeeWorkSchoolHistory = await _employeeWorkAndSchoolHistoryService.GetEmployeeWorkAndSchoolHistory(employeeCode);
            return Ok(employeeWorkSchoolHistory);
        }

        /// <summary>
        /// Get work and school history for all employees paginated
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageCount"></param>
        /// <param name="includeAlumni"></param>
        [HttpGet("workSchoolHistoryForAll")]
        public async Task<IActionResult> GetWorkSchoolHistoryForAll(int pageNumber, int pageCount, bool includeAlumni = false)
        {
            var employeeWorkSchoolHistory = await _employeeWorkAndSchoolHistoryService.GetWorkSchoolHistoryForAll(pageNumber, pageCount, includeAlumni);
            return Ok(employeeWorkSchoolHistory);
        }

        /// <summary>
        /// Get work and school history for employees after last modifed date
        /// </summary>
        /// <param name="lastModifiedAfter"></param>
        /// <param name="includeAlumni"></param>
        [HttpGet("workSchoolHistoryByModifiedDate")]
        public async Task<IActionResult> GetWorkSchoolHistoryByModifiedDate(DateTime lastModifiedAfter, bool includeAlumni = false)
        {
            var employeeWorkSchoolHistory = await _employeeWorkAndSchoolHistoryService.GetWorkSchoolHistoryByModifiedDate(lastModifiedAfter, includeAlumni);
            return Ok(employeeWorkSchoolHistory);
        }
    }
}
