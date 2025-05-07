using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IBaseRepository<UserNotification> _baseRepository;

        public NotificationRepository(IBaseRepository<UserNotification> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<UserNotification>> GetUserNotifications(string employeeCode, string officeCodes)
        {
            var userNotifications = await _baseRepository.GetAllAsync(new { employeeCode, officeCodes },
                StoredProcedureMap.GetUserNotifications);
            return userNotifications;
        }

        public async Task UpdateUserNotificationStatus(Guid notificationId, string employeeCode, char notificationStatus)
        {
            await _baseRepository.UpdateAsync(StoredProcedureMap.UpdateUserNotificationStatus, new
            {
                notificationId,
                employeeCode,
                notificationStatus
            });
        }
    }
}
