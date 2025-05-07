using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IIrisPollingRepository
    {
        Task UpsertWorkAndSchoolHistory(DataTable workHistoryDataTable, DataTable schoolHistoryDataTable);
    }
}
