using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IVacationPollingRepository
    {
        Task UpsertVacations(DataTable vacationDataTable);
    }
}
