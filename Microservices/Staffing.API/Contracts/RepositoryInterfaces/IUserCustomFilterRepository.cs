using Staffing.API.Models;
using Staffing.API.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IUserCustomFilterRepository
    {
        Task<IEnumerable<ResourceFilterViewModel>> GetCustomResourceFiltersByEmployeeCode(string employeeCode);
        Task<IEnumerable<ResourceFilterViewModel>> UpsertCustomResourceFilters(DataTable upsertedResourceFilters, DataTable savedGroupFiltersDataTable);
        Task<string> DeleteCustomResourceFilterById(string filterIdToDelete, string lastUpdatedBy);
    }
}
