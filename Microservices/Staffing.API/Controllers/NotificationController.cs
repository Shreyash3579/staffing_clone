using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using System;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        /// <summary>
        ///     Get user notification for selected Offices in Demand view
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="officeCodes"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(string employeeCode, string officeCodes)
        {
            var userNotifications = await _notificationService.GetUserNotifications(employeeCode, officeCodes);
            return Ok(userNotifications);
        }

        /// <summary>
        /// Update notification status of notification from unread to read or archive
        /// </summary>
        /// <param name="notificationId"></param>
        /// <param name="employeeCode"></param>
        /// <param name="notificationStatus"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateUserNotificationStatus(dynamic payload)
        {
            var notificationId = new Guid(payload["notificationId"].ToString());
            var employeeCode = payload["employeeCode"].ToString();
            var notificationStatus = Convert.ToChar(payload["notificationStatus"]);
            await _notificationService
                .UpdateUserNotificationStatus(notificationId, employeeCode, notificationStatus);
            return Ok();
        }
    }
}