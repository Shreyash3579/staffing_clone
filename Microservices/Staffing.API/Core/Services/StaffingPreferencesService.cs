using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class StaffingPreferencesService : IStaffingPreferencesService
    {
        private readonly IStaffingPreferencesRepository _staffingPreferencesRepository;
        public StaffingPreferencesService(IStaffingPreferencesRepository staffingPreferencesRepository)
        {
            _staffingPreferencesRepository = staffingPreferencesRepository;
        }

        public async Task<EmployeeStaffingPreferencesForInsightsTool> GetEmployeePreferences(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
                throw new ArgumentException("EmployeeCode cannot be null or empty");

            return await _staffingPreferencesRepository.GetEmployeePreferences(employeeCode);
        }

        public async Task<IEnumerable<EmployeeStaffingPreferencesForInsightsTool>> GetAllEmployeePreferences()
        {
            return await _staffingPreferencesRepository.GetAllEmployeePreferences();
        }

        public async Task<EmployeeStaffingPreferencesForInsightsTool> UpsertEmployeePreferences(EmployeeStaffingPreferencesForInsightsTool staffingPreferencesForEmployee)
        {
            return await _staffingPreferencesRepository.UpsertEmployeePreferences(staffingPreferencesForEmployee);

        }
    }
}
