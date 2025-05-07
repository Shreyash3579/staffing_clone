using Basis.API.Contracts.Services;
using Basis.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Basis.API.Core.Helpers;

namespace Basis.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.BasisAllAccess)]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _service;
        public HolidayController(IHolidayService service)
        {
            _service = service;
        }

        // TODO: This is being used in getting holiday data for resource overlay gantt. The route is defined inside global.json of Ocelot. Think about utilizing the officeholidaysWithinDateRangeByEmployees method.
        /// <summary>
        /// Get holidays for employee's scheduling office effective from date
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="effectiveFromDate"></param>
        /// <param name="effectiveToDate"></param>
        /// <returns>Get holidays for employee's scheduling office</returns>
        [HttpGet]
        public async Task<IActionResult> GetOfficeHolidaysByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var officeHolidays = await _service.GetOfficeHolidaysByEmployee(employeeCode, effectiveFromDate, effectiveToDate);
            return Ok(officeHolidays);
        }

        /// <summary>
        /// Get Holidays For employees between date range specified
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {"employeeCodes":"09PTS,31JWE","startDate":"2019-07-12 00:00:00","endDate":"2019-09-15 00:00:00"}
        /// or {"employeeCodes":"09PTS,31JWE","startDate":"2019-07-12 00:00:00","endDate": null}
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("officeholidaysWithinDateRangeByEmployees")]
        [ProducesResponseType(typeof(HolidayViewModel), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> GetOfficeHolidaysWithinDateRangeByEmployees(dynamic payload)
        {
            var employeeCodes = payload["employeeCodes"].ToString();
            var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;

            var employeesHolidays = await _service.GetOfficeHolidaysWithinDateRangeByEmployees(employeeCodes, startDate, endDate);
            return Ok(employeesHolidays);
        }

        /// <summary>
        /// Get all holidays
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("holidays")]
        public async Task<IActionResult> GetHolidays()
        {
            var trainings = await _service.GetHolidays();
            return Ok(trainings);
        }

        /// <summary>
        /// Get Holidays For employees between date range specified
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {"officeCodes":110,416","startDate":"2019-07-12 00:00:00","endDate":"2019-09-15 00:00:00"}
        /// or {"officeCodes":110,416","startDate":"2019-07-12 00:00:00","endDate": null}
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("officeholidaysWithinDateRangeByOffices")]
        [ProducesResponseType(typeof(HolidayViewModel), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> GetOfficeHolidaysWithinDateRangeByOffices(dynamic payload)
        {
            var officeCodes = payload["officeCodes"].ToString();
            var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;

            var officeHolidays = await _service.GetOfficeHolidaysWithinDateRangeByOffices(officeCodes, startDate, endDate);
            return Ok(officeHolidays);
        }
    }
}
