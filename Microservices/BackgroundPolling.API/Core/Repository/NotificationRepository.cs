using BackgroundPolling.API.Contracts.Helpers;
using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IBaseRepository<Notification> _baseRepository;

        public NotificationRepository(IBaseRepository<Notification> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task InsertNotification(DataTable notificationDataTable)
        {
            await _baseRepository.Context.Connection.QueryAsync<Notification>(
                StoredProcedureMap.InsertNotifications,
                new
                {
                    notifications =
                        notificationDataTable.AsTableValuedParameter(
                            "[dbo].[notificationTableType]"),
                    lastUpdatedBy = "PollingApi"
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task<IEnumerable<ResourceAllocation>> GetEmployeesRequiresBackFillBySpecificDate(int allocationEndingBeforeNumberOfDays)
        {
            var resourcesRequiresBackFill = await Task.Run(() => _baseRepository.Context.Connection.Query<ResourceAllocation>(
                StoredProcedureMap.GetEmployeesRequiresBackFillBySpecificDate,
                new { allocationEndingBeforeNumberOfDays },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return resourcesRequiresBackFill;
        }
    }
}
