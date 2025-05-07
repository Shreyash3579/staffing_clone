using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IAzureSearchPollingService
    {
        public Task<string> UpsertEmployeeConsildatedDataForSearch(bool isFullLoad);
    }
}
