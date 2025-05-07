using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IVacationPollingService
    {
        public Task upsertVacations();
    }
}
