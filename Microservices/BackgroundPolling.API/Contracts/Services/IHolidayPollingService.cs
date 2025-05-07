using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IHolidayPollingService
    {
        public Task InsertHolidays();
    }
}
