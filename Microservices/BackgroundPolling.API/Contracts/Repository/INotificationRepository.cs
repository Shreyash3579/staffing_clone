using BackgroundPolling.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface INotificationRepository
    {
        Task InsertNotification(DataTable notificationDataTable);
        Task<IEnumerable<ResourceAllocation>> GetEmployeesRequiresBackFillBySpecificDate(int allocationEndingBeforeNumberOfDays);
    }
}
