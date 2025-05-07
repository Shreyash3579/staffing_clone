using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoidayPollingController : ControllerBase
    {
        public IHolidayPollingService _holidayPollingService;
        public HoidayPollingController(IHolidayPollingService holidayPollingService)
        {
            _holidayPollingService = holidayPollingService;
        }

        /// <summary>
        /// Saves all Holiday data from Basis to Analytics database
        /// 
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InsertHolidays()
        {
            await _holidayPollingService.InsertHolidays();
            return Ok();
        }
    }
}
