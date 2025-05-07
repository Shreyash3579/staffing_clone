using Staffing.AzureSearch.Models;

namespace Staffing.AzureSearch.Contracts.Services
{
    public interface IEmployeeIndexerService
    {
        Task UploadDataToEmployeeConsolidatedIndex(IEnumerable<ResourcePartial> dataToIndex);
        Task<string> IndexResourceNotesByLastUpdatedDate(DateTime dateTime);
    }
}
