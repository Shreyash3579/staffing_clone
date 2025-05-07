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
    public class EmployeeStaffingInfoService : IEmployeeStaffingInfoService
    {
        private readonly IEmployeeStaffingInfoRepository _employeeStaffingInfoRepository;

        public EmployeeStaffingInfoService(IEmployeeStaffingInfoRepository employeeStaffingInfoRepository)
        {
            _employeeStaffingInfoRepository = employeeStaffingInfoRepository;
        }

        public async Task<IEnumerable<StaffingResponsible>> GetResourceStaffingResponsibleByEmployeeCodes(string employeeCodes)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                throw new ArgumentException("EmployeeCodes cannot be null or empty");

            var staffingResponsibleData =
                await _employeeStaffingInfoRepository.GetResourceStaffingResponsibleByEmployeeCodes(employeeCodes);

            return staffingResponsibleData ?? Enumerable.Empty<StaffingResponsible>();
        }

        public async Task<IEnumerable<StaffingResponsible>> UpsertResourceStaffingResponsible(IEnumerable<StaffingResponsible> employeeStaffingResponsibleData)
        {
            if (!employeeStaffingResponsibleData.Any())
                throw new ArgumentException("No data to upsert");

            var employeeStaffingResponsibleDataTable = CreateEmployeeStaffingResponsibleDataTable(employeeStaffingResponsibleData.ToList());
            return await _employeeStaffingInfoRepository.UpsertEmployeeStaffingResponsible(employeeStaffingResponsibleDataTable);
        }
        #region Private Helpers
        private DataTable CreateEmployeeStaffingResponsibleDataTable(IList<StaffingResponsible> employeeStaffableAsData)
        {
            var employeeStaffableAsDataTable = new DataTable();
            employeeStaffableAsDataTable.Columns.Add("id", typeof(Guid));
            employeeStaffableAsDataTable.Columns.Add("employeeCode", typeof(string));
            employeeStaffableAsDataTable.Columns.Add("responsibleForStaffingCodes", typeof(string));
            employeeStaffableAsDataTable.Columns.Add("pdLeadCodes", typeof(string));
            employeeStaffableAsDataTable.Columns.Add("notifyUponStaffingCodes", typeof(string));
            employeeStaffableAsDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var staffingResponsible in employeeStaffableAsData)
            {
                var row = employeeStaffableAsDataTable.NewRow();
                row["id"] = (object)staffingResponsible.Id ?? DBNull.Value;
                row["employeeCode"] = staffingResponsible.EmployeeCode;
                row["responsibleForStaffingCodes"] = (object)staffingResponsible.ResponsibleForStaffingCodes ?? DBNull.Value;
                row["pdLeadCodes"] = (object)staffingResponsible.pdLeadCodes ?? DBNull.Value;
                row["notifyUponStaffingCodes"] = (object)staffingResponsible.notifyUponStaffingCodes ?? DBNull.Value;
                row["lastUpdatedBy"] = staffingResponsible.LastUpdatedBy;
                employeeStaffableAsDataTable.Rows.Add(row);
            }

            return employeeStaffableAsDataTable;
        }
        #endregion

    }
}
