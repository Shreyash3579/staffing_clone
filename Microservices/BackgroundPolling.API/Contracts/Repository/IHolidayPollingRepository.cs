using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IHolidayPollingRepository
    {
        Task InsertHolidays(DataTable holidayDataTable);
    }
}
