using BackgroundPolling.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> InsertCasesEndingNotification();
        Task<IEnumerable<Notification>> InsertBackFillNotification();
    }
}
