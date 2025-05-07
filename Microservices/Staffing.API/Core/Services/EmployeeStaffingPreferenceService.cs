using NuGet.Packaging.Signing;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class EmployeeStaffingPreferenceService : IEmployeeStaffingPreferenceService
    {
        private readonly IEmployeeStaffingPreferenceRepository _employeeStaffingPreferenceRepository;

        public EmployeeStaffingPreferenceService(IEmployeeStaffingPreferenceRepository employeeStaffingPreferenceRepository)
        {
            _employeeStaffingPreferenceRepository = employeeStaffingPreferenceRepository;
        }
        public async Task<IEnumerable<EmployeeStaffingPreferences>> GetEmployeeStaffingPreferences(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Enumerable.Empty<EmployeeStaffingPreferences>();
            }
            var employeeStaffingPreferences = await _employeeStaffingPreferenceRepository.GetEmployeeStaffingPreferences(employeeCode);
            return employeeStaffingPreferences;
        }
        public async Task<IEnumerable<EmployeeStaffingPreferences>> UpsertEmployeeStaffingPreferences(IEnumerable<EmployeeStaffingPreferences> employeeStaffingPreferences)
        {
            var employeeStaffingPreferenceDataTable = CreateEmployeeStaffingPreferenceTable(employeeStaffingPreferences.ToList());
            var upsertedEmployeeStaffingPreferences = await _employeeStaffingPreferenceRepository
                .UpsertEmployeeStaffingPreferences(employeeStaffingPreferenceDataTable);

            return upsertedEmployeeStaffingPreferences;

        }

        public async Task DeleteEmployeeStaffingPreferenceByType(string employeeCode, string preferenceTypeCode)
        {
            if (string.IsNullOrEmpty(employeeCode) || string.IsNullOrEmpty(preferenceTypeCode))
                throw new ArgumentException("employeeCode or preferenceTypeCode cannot be null or empty");

            await _employeeStaffingPreferenceRepository.DeleteEmployeeStaffingPreferenceByType(employeeCode, preferenceTypeCode);

            return;

        }


        private DataTable CreateEmployeeStaffingPreferenceTable(IList<EmployeeStaffingPreferences> employeeStaffingPreferences)
        {
            var employeeStaffingPreferenceDataTable = new DataTable();
            employeeStaffingPreferenceDataTable.Columns.Add("employeeCode", typeof(string));
            employeeStaffingPreferenceDataTable.Columns.Add("preferenceTypeCode", typeof(string));
            employeeStaffingPreferenceDataTable.Columns.Add("staffingPreference", typeof(string));
            employeeStaffingPreferenceDataTable.Columns.Add("priority", typeof(Int16));
            employeeStaffingPreferenceDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            employeeStaffingPreferenceDataTable.Columns.Add("interest", typeof(bool));
            employeeStaffingPreferenceDataTable.Columns.Add("noInterest", typeof(bool));

            foreach (var employeeStaffingPreference in employeeStaffingPreferences)
            {
                var row = employeeStaffingPreferenceDataTable.NewRow();
                row["employeeCode"] = (object)employeeStaffingPreference.EmployeeCode ?? DBNull.Value;
                row["preferenceTypeCode"] = (object)employeeStaffingPreference.PreferenceTypeCode ?? DBNull.Value;
                row["staffingPreference"] = (object)employeeStaffingPreference.StaffingPreference ?? DBNull.Value;
                row["priority"] = (object)employeeStaffingPreference.Priority ?? DBNull.Value;
                row["lastUpdatedBy"] = employeeStaffingPreference.LastUpdatedBy;
                row["interest"] = (object)employeeStaffingPreference.Interest ?? DBNull.Value;
                row["noInterest"] = (object)employeeStaffingPreference.NoInterest ?? DBNull.Value;
                employeeStaffingPreferenceDataTable.Rows.Add(row);
            }

            return employeeStaffingPreferenceDataTable;
        }
    }
}
