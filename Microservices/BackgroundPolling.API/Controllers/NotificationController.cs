using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Insert cases that are ending soon
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("caseEnding")]
        public async Task<IActionResult> InsertCasesEndingNotification()
        {
            var notifications = await _notificationService.InsertCasesEndingNotification();
            return Ok(notifications);
        }


        /// <summary>
        /// Insert employees who needs backfill
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("backFill")]
        public async Task<IActionResult> InsertBackFillNotification()
        {
            var notifications = await _notificationService.InsertBackFillNotification();
            return Ok(notifications);
        }
    }
}
