using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface ITrainingPollingRepository
    {
        Task UpsertTrainings(DataTable trainingDataTable);
    }
}
