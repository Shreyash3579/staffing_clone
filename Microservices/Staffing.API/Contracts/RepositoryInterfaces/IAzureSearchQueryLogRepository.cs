using Staffing.API.Models;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IAzureSearchQueryLogRepository
    {
        Task InsertAzureSearchQueryLog(AzureSearchQueryLog azureSearchQueryLog);
    }
}
