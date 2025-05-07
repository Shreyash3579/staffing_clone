using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class AzureSearchQueryLogService : IAzureSearchQueryLogService
    {
        private readonly IAzureSearchQueryLogRepository _azureSearchQueryLogRepository;

        public AzureSearchQueryLogService(IAzureSearchQueryLogRepository azureSearchQueryLogRepository)
        {
            _azureSearchQueryLogRepository = azureSearchQueryLogRepository;
        }

        public async Task InsertAzureSearchQueryLog(AzureSearchQueryLog azureSearchQueryLog)
        {
            await _azureSearchQueryLogRepository.InsertAzureSearchQueryLog(azureSearchQueryLog);
        }
    }
}
