using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VacationPollingController : ControllerBase
    {
        public IVacationPollingService _vacationPollingService;
        public VacationPollingController(IVacationPollingService vacationPollingService)
        {
            _vacationPollingService = vacationPollingService;
        }

        /// <summary>
        /// Saves all Vacations from VRS database to Analytics database
        /// 
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UpsertVacations()
        {
            await _vacationPollingService.upsertVacations();
            return Ok();
        }
    }
}