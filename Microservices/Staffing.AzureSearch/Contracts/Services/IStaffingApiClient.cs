using Staffing.AzureSearch.Models;

namespace Staffing.AzureSearch.Contracts.Services
{
    public interface IStaffingApiClient
    {
        Task InsertAzureSearchQueryLog(AzureSearchQueryLog searchQueryLog);
        Task<IEnumerable<ResourceViewNote>> GetResourceNotesAfterLastPolledTime(DateTime lastupdatedAfter, string noteTypeCode);
    }
}