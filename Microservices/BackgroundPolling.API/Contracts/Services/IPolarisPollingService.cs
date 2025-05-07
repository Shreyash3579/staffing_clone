using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IPolarisPollingService
    {
        public Task UpsertSecurityUsers();
    }
}
