using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Vacation.API.Contracts.Services;
using Vacation.API.Models;

namespace Vacation.API.Controllers
{
    /// <summary>
    /// Vacation Requests Controller
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class VacationRequestController : ControllerBase
    {
        private readonly IVacationRequestService _service;
        public VacationRequestController(IVacationRequestService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get vacation requests for employee
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="effectiveFromDate">optional</param>
        /// <param name="effectiveToDate">optional</param>
        /// <returns>Approved/Pending vacations</returns>
        [HttpGet]
        [ProducesResponseType(typeof(VacationRequest), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVacationRequestsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var vacations = await _service.GetVacationRequestsByEmployee(employeeCode, effectiveFromDate, effectiveToDate);
            return Ok(vacations);
        }

        /// <summary>
        /// Get Vacation requests for employees between specified date range
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {"employeeCodes":"09PTS,31JWE","startDate":"2019-07-12 00:00:00","endDate":"2019-09-15 00:00:00"}
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns>Approved/Pending vacations</returns>
        [HttpPost]
        [Route("vacationsWithinDateRangeByEmployees")]
        [ProducesResponseType(typeof(VacationRequest), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> GetVacationsWithinDateRangeByEmployeeCodes(dynamic payload)
        {
            var employeeCodes = payload["employeeCodes"].ToString();
            var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;

            var employeesVacations = await _service.GetVacationsWithinDateRangeByEmployeeCodes(employeeCodes, startDate, endDate);
            return Ok(employeesVacations);
        }

        [HttpGet]
        [Route("vacations")]
        [ProducesResponseType(typeof(VacationRequest), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVacationRequests(DateTime? lastPolledDateTime)
        {
            var vacations = await _service.GetVacationRequests(lastPolledDateTime);
            return Ok(vacations);
        }
    }
}
