using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IStaffingPreferencesRepository
    {
        Task<EmployeeStaffingPreferencesForInsightsTool> GetEmployeePreferences(string employeeCode);
        Task<IEnumerable<EmployeeStaffingPreferencesForInsightsTool>> GetAllEmployeePreferences();
        Task<EmployeeStaffingPreferencesForInsightsTool> UpsertEmployeePreferences(EmployeeStaffingPreferencesForInsightsTool securityUser);
    }
}
