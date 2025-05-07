using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IrisPollingController : ControllerBase
    {
        public IIrisPollingService _irisPollingService;
        public IrisPollingController(IIrisPollingService irisPollingService)
        {
            _irisPollingService = irisPollingService;
        }

        /// <summary>
        /// Saves all school and employment data for all active employees from Iris to Analytics database
        /// 
        /// </summary>
        [HttpPost]
        [Route("workAndEducationHistoryForAllActiveEmployees")]
        public async Task<IActionResult> UpsertWorkAndSchoolHistoryForAllActiveEmployeesFromIris()
        {
            await _irisPollingService.UpsertWorkAndSchoolHistoryForAllActiveEmployeesFromIris();
            return Ok("Success");
        }

        /// <summary>
        /// Saves all school and employment data for active employees who data has been updated after the modified date
        /// 
        /// </summary>
        [HttpPost]
        [Route("workAndEducationHistoryAfterLastModifiedDateForActiveEmployeesFromIris")]
        public async Task<IActionResult> InsertWorkAndEducationHistoryAfterLastModifiedDateForActiveEmployeesFromIris(DateTime? lastModifiedAfter)
        {
            var updatedEmployeeCodes = await _irisPollingService.InsertWorkAndEducationHistoryAfterLastModifiedDateForActiveEmployeesFromIris(lastModifiedAfter);
            return Ok(updatedEmployeeCodes);
        }
    }
}
