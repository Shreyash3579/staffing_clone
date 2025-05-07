using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<UserNotification>> GetUserNotifications(string employeeCode, string officeCodes);
        Task UpdateUserNotificationStatus(Guid notificationId, string employeeCode, char notificationStatus);
    }
}
