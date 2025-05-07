using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IEmployeeStaffingPreferenceRepository
    {
        Task<IEnumerable<EmployeeStaffingPreferences>> GetEmployeeStaffingPreferences(string employeeCode);
        Task<IEnumerable<EmployeeStaffingPreferences>> UpsertEmployeeStaffingPreferences(DataTable employeeStaffingPreferences);
        Task DeleteEmployeeStaffingPreferenceByType(string employeeCode, string preferenceTypeCode);
    }
}
