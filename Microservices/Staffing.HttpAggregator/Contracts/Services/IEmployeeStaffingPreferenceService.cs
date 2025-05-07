using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IEmployeeStaffingPreferenceService
    {
        Task<IEnumerable<EmployeeStaffingPreferencesViewModel>> GetEmployeeStaffingPreferences(string employeeCode);
        Task<EmployeeStaffingPreferencesViewModel> UpsertEmployeeStaffingPreferences(EmployeeStaffingPreferencesViewModel employeeStaffingPreferences);
    }
}
