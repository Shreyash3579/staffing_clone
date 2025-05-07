using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<UserNotificationViewModel>> GetUserNotifications(string employeeCode,
            string officeCodes)
        {
            var notifications = await _notificationRepository.GetUserNotifications(employeeCode, officeCodes);
            var userNotifications = notifications.Select(n => new UserNotificationViewModel
            {
                NotificationId = n.NotificationId,
                Description = n.NotificationNotes,
                NotificationStatus = n.NotificationStatus,
                NotificationDueDate = n.EndDate,
                NotificationDate = n.LastUpdated,
                OldCaseCode = n.OldCaseCode,
                EmployeeCode = n.EmployeeCode
            }).OrderBy(x => x.NotificationDueDate).ToList();

            return userNotifications;
        }

        public async Task UpdateUserNotificationStatus(Guid notificationId, string employeeCode, char notificationStatus)
        {
            await _notificationRepository.UpdateUserNotificationStatus(notificationId, employeeCode,
                notificationStatus);
        }
    }
}