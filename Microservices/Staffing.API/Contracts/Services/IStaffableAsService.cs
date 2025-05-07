using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IStaffableAsService
    {
        Task<IEnumerable<StaffableAs>> GetResourceActiveStaffableAsByEmployeeCodes(string employeeCodes);
        Task<IEnumerable<StaffableAs>> UpsertResourceStaffableAs(IEnumerable<StaffableAs> employeeStaffableAsData);
        Task<string> DeleteResourceStaffableAsById(string idToDelete, string lastUpdatedBy);
    }
}
