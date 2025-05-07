using Staffing.API.Models;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IAzureSearchQueryLogService
    {
        Task InsertAzureSearchQueryLog(AzureSearchQueryLog azureSearchQueryLog);
    }
}
