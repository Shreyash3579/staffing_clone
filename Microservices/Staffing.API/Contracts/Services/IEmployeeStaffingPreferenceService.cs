using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IEmployeeStaffingPreferenceService
    {
        Task<IEnumerable<EmployeeStaffingPreferences>> GetEmployeeStaffingPreferences(string employeeCode);
        Task<IEnumerable<EmployeeStaffingPreferences>> UpsertEmployeeStaffingPreferences(IEnumerable<EmployeeStaffingPreferences> employeeStaffingPreferences);

        Task DeleteEmployeeStaffingPreferenceByType(string employeeCode, string preferenceTypeCode);
    }
}
