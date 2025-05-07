using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Staffing.API.Models;
using System.Linq;
using System.Data;

namespace Staffing.API.Core.Services
{
    public class PreponedCasesAllocationsAuditService: IPreponedCasesAllocationsAuditService
    {
        private readonly IPreponedCasesAllocationsAuditRepository _preponedCasesAllocationsAuditRepository;

        public PreponedCasesAllocationsAuditService(IPreponedCasesAllocationsAuditRepository preponedCasesAllocationsAuditRepository)
        {
            _preponedCasesAllocationsAuditRepository = preponedCasesAllocationsAuditRepository;
        }

        public async Task<IEnumerable<PreponedCasesAllocationsAudit>> UpsertPreponedCaseAllocationsAudit(
            IEnumerable<PreponedCasesAllocationsAudit> preponedCasesAllocationsAudit)
        {
            if (preponedCasesAllocationsAudit == null || !preponedCasesAllocationsAudit.Any())
                throw new ArgumentException();

            var preponedCasesAllocationsAuditDataTable = CreateAuditDataTable(preponedCasesAllocationsAudit);
            var result =
                await _preponedCasesAllocationsAuditRepository.UpsertPreponedCaseAllocationsAudit(preponedCasesAllocationsAuditDataTable);

            return result;
        }

        public async Task<IEnumerable<PreponedCasesAllocationsAudit>> GetPreponedCaseAllocationsAudit(string serviceLineCodes, string officeCodes,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            if (serviceLineCodes == null || !officeCodes.Any())
                throw new ArgumentException("service line code and office code is required!");

            var result =
                await _preponedCasesAllocationsAuditRepository.GetPreponedCaseAllocationsAudit(serviceLineCodes, officeCodes, startDate, endDate);

            return result;
        }

        #region private methods
        public static DataTable CreateAuditDataTable(IEnumerable<PreponedCasesAllocationsAudit> data)
        {
            DataTable table = new DataTable();

            table.Columns.Add("id", typeof(Guid));
            table.Columns.Add("caseCode", typeof(int));
            table.Columns.Add("clientCode", typeof(int));
            table.Columns.Add("oldCaseCode", typeof(string));
            table.Columns.Add("originalCaseStartDate", typeof(DateTime));
            table.Columns.Add("updatedCaseStartDate", typeof(DateTime));
            table.Columns.Add("originalCaseEndDate", typeof(DateTime));
            table.Columns.Add("updatedCaseEndDate", typeof(DateTime));
            table.Columns.Add("employeeCode", typeof(string));
            table.Columns.Add("serviceLineCode", typeof(string));
            table.Columns.Add("operatingOfficeCode", typeof(short));
            table.Columns.Add("caseLastUpdatedBy", typeof(string));
            table.Columns.Add("caseLastUpdated", typeof(DateTime));
            table.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var item in data)
            {
                DataRow row = table.NewRow();

                row["id"] = (object)item.Id ?? DBNull.Value;
                row["caseCode"] = (object)item.CaseCode ?? DBNull.Value;
                row["clientCode"] = (object)item.ClientCode ?? DBNull.Value;
                row["oldCaseCode"] = (object)item.OldCaseCode ?? DBNull.Value;
                row["originalCaseStartDate"] = (object)item.OriginalCaseStartDate ?? DBNull.Value;
                row["updatedCaseStartDate"] = (object)item.UpdatedCaseStartDate ?? DBNull.Value;
                row["originalCaseEndDate"] = (object)item.OriginalCaseEndDate ?? DBNull.Value;
                row["updatedCaseEndDate"] = (object)item.UpdatedCaseEndDate ?? DBNull.Value;
                row["employeeCode"] = (object)item.EmployeeCode ?? DBNull.Value;
                row["serviceLineCode"] = (object)item.ServiceLineCode ?? DBNull.Value;
                row["operatingOfficeCode"] = (object)item.OperatingOfficeCode ?? DBNull.Value;
                row["caseLastUpdatedBy"] = (object)item.CaseLastUpdatedBy ?? DBNull.Value;
                row["caseLastUpdated"] = (object)item.CaseLastUpdated ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)item.LastUpdatedBy ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }

        #endregion
    }
}
