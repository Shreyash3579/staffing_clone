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
    public class StaffableAsService : IStaffableAsService
    {
        private readonly IStaffableAsRepository _staffableAsRepository;

        public StaffableAsService(IStaffableAsRepository staffableAsRepository)
        {
            _staffableAsRepository = staffableAsRepository;
        }

        public async Task<IEnumerable<StaffableAs>> GetResourceActiveStaffableAsByEmployeeCodes(string employeeCodes)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                throw new ArgumentException("EmployeeCode(s) cannot be null or empty");

            var staffableAsData =  
                await _staffableAsRepository.GetResourceActiveStaffableAsByEmployeeCodes(employeeCodes);

            return staffableAsData ?? Enumerable.Empty<StaffableAs>();
        }

        public async Task<IEnumerable<StaffableAs>> UpsertResourceStaffableAs(IEnumerable<StaffableAs> employeeStaffableAsData)
        {
            if (!employeeStaffableAsData.Any())
                throw new ArgumentException("No data to upsert");

            var employeeStaffableAsDataTable = CreateEmployeeStaffableAsTable(employeeStaffableAsData.ToList());
            return
                await _staffableAsRepository.UpsertResourceStaffableAs(employeeStaffableAsDataTable);
        }

        public async Task<string> DeleteResourceStaffableAsById(string idToDelete, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(idToDelete))
                throw new ArgumentException("EmployeeCode cannot be null or empty");

            await _staffableAsRepository.DeleteResourceStaffableAsById(idToDelete, lastUpdatedBy);

            return idToDelete;
        }

        #region Private Helpers
        private DataTable CreateEmployeeStaffableAsTable(IList<StaffableAs> employeeStaffableAsData)
        {
            var employeeStaffableAsDataTable = new DataTable();
            employeeStaffableAsDataTable.Columns.Add("id", typeof(Guid));
            employeeStaffableAsDataTable.Columns.Add("employeeCode", typeof(string));
            employeeStaffableAsDataTable.Columns.Add("levelGrade", typeof(string));
            employeeStaffableAsDataTable.Columns.Add("staffableAsTypeCode", typeof(short));
            employeeStaffableAsDataTable.Columns.Add("effectiveDate", typeof(DateTime));
            employeeStaffableAsDataTable.Columns.Add("isActive", typeof(bool));
            employeeStaffableAsDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var staffableAs in employeeStaffableAsData)
            {
                var row = employeeStaffableAsDataTable.NewRow();
                row["id"] = (object)staffableAs.Id ?? DBNull.Value;
                row["employeeCode"] = staffableAs.EmployeeCode;
                row["levelGrade"] = staffableAs.LevelGrade;
                row["effectiveDate"] = staffableAs.EffectiveDate.Date;
                row["staffableAsTypeCode"] = staffableAs.StaffableAsTypeCode;
                row["isActive"] = staffableAs.isActive;
                row["lastUpdatedBy"] = staffableAs.LastUpdatedBy;
                employeeStaffableAsDataTable.Rows.Add(row);
            }

            return employeeStaffableAsDataTable;
        }
        #endregion

    }
}