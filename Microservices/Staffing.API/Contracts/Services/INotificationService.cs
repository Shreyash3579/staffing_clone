using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<UserNotificationViewModel>> GetUserNotifications(string employeeCode, string officeCodes);
        Task UpdateUserNotificationStatus(Guid notificationId, string employeeCode, char notificationStatus);
    }
}
