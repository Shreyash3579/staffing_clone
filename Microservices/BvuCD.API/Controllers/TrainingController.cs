using BvuCD.API.Contracts.Services;
using BvuCD.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BvuCD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TrainingController : ControllerBase
    {
        private readonly ITrainingService _service;
        public TrainingController(ITrainingService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get Trainings by employee
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="effectiveFromDate"></param>
        /// <param name="effectiveToDate"></param>
        /// <returns>Gets Trainings for employee</returns>
        [HttpGet]
        public async Task<IActionResult> GetTrainingsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var trainings = await _service.GetTrainingsByEmployee(employeeCode, effectiveFromDate, effectiveToDate);
            return Ok(trainings);
        }

        [HttpGet]
        [Route("trainings")]
        public async Task<IActionResult> GetTrainings(DateTime? lastPolledDateTime)
        {
            var trainings = await _service.GetTrainings(lastPolledDateTime);
            return Ok(trainings);
        }

        /// <summary>
        /// Get Trainings For employees between date range specified
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {"employeeCodes":"09PTS,31JWE","startDate":"2019-07-12 00:00:00","endDate":"2019-09-15 00:00:00"}
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("trainingsWithinDateRangeByEmployees")]
        [ProducesResponseType(typeof(TrainingViewModel), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> GetTrainingsWithinDateRangeByEmployeeCodes(dynamic payload)
        {
            var employeeCodes = payload["employeeCodes"].ToString();
            var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;

            var employeesTrainings = await _service.GetTrainingsWithinDateRangeByEmployeeCodes(employeeCodes, startDate, endDate);
            return Ok(employeesTrainings);
        }
    }
}