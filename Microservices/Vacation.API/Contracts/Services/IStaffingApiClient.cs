using System.Threading.Tasks;
using Vacation.API.Models;

namespace Vacation.API.Contracts.Services
{
    public interface IStaffingApiClient
    {
        Task InsertAzureSearchQueryLog(AzureSearchQueryLog searchQueryLog);
    }
}