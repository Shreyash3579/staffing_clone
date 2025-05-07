using Staffing.API.Models;
using Staffing.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IUserCustomFilterService
    {
        Task<IEnumerable<ResourceFilterViewModel>> GetCustomResourceFiltersByEmployeeCode(string employeeCode);
        Task<IEnumerable<ResourceFilterViewModel>> UpsertCustomResourceFilters(IEnumerable<ResourceFilterViewModel> upsertedResourceFilters);
        Task<string> DeleteCustomResourceFilterById(string filterIdToDelete, string lastUpdatedBy);
    }
}
