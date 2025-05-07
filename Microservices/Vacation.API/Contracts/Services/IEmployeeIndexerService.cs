using System.Collections.Generic;
using System.Threading.Tasks;
using Vacation.API.Models;

namespace Vacation.API.Contracts.Services
{
    public interface IEmployeeIndexerService
    {
        Task UploadDataToEmployeeConsolidatedIndex(IEnumerable<ResourcePartial> dataToIndex);
    }
}
