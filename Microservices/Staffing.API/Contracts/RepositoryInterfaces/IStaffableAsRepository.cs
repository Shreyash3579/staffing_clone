using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IStaffableAsRepository
    {
        Task<IEnumerable<StaffableAs>> GetResourceActiveStaffableAsByEmployeeCodes(string employeeCodes);
        Task<IEnumerable<StaffableAs>> UpsertResourceStaffableAs(DataTable employeeStaffableAsData);
        Task DeleteResourceStaffableAsById(string idToDelete, string lastUpdatedBy);
    }
}
