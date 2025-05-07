using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface ITrainingPollingService
    {
        Task upsertTrainings();
    }
}
