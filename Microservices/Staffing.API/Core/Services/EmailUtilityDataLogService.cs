using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class EmailUtilityDataLogService : IEmailUtilityDataLogService
    {
        private readonly IEmailUtilityDataLogRepository _emailUtilityDataLogRepository;

        public EmailUtilityDataLogService(IEmailUtilityDataLogRepository emailUtilityDataLogRepository)
        {
            _emailUtilityDataLogRepository = emailUtilityDataLogRepository;
        }

        public async Task<IEnumerable<EmailUtilityData>> GetEmailUtilityDataLogsByDateAndEmailType(DateTime dateOfEmail, string emailType)
        {
            if (dateOfEmail == null || dateOfEmail == DateTime.MinValue) 
                throw new ArgumentException("Date is Mandatory");

            if (string.IsNullOrEmpty(emailType))
                throw new ArgumentException("Email Type is Mandatory");

            return await _emailUtilityDataLogRepository.GetEmailUtilityDataLogsByDateAndEmailType(dateOfEmail, emailType);
        }

        public async Task<IEnumerable<EmailUtilityData>> UpsertEmailUtilityDataLog(
            IEnumerable<EmailUtilityData> emailUtilityDatas)
        {
            if (emailUtilityDatas == null) throw new ArgumentException("emailUtility data cannot be null or empty");

            var emailUtilityDataTable = CreateEmailutilityDataTable(emailUtilityDatas);
            var allocatedResources =
                await _emailUtilityDataLogRepository.UpsertEmailUtilityDataLog(emailUtilityDataTable);

            return allocatedResources;
        }

        #region Helper Methods
        private static DataTable CreateEmailutilityDataTable(IEnumerable<EmailUtilityData> emailUtilityDatas)
        {
            var emailUtilityDataTable = new DataTable();
            emailUtilityDataTable.Columns.Add("employeeCode", typeof(string));
            emailUtilityDataTable.Columns.Add("employeeName", typeof(string));
            emailUtilityDataTable.Columns.Add("currentLevelGrade", typeof(string));
            emailUtilityDataTable.Columns.Add("positionGroupName", typeof(string));
            emailUtilityDataTable.Columns.Add("serviceLineName", typeof(string));
            emailUtilityDataTable.Columns.Add("officeName", typeof(string));
            emailUtilityDataTable.Columns.Add("status", typeof(string));
            emailUtilityDataTable.Columns.Add("retryCount", typeof(int));
            emailUtilityDataTable.Columns.Add("exception", typeof(string));
            emailUtilityDataTable.Columns.Add("date", typeof(DateTime));
            emailUtilityDataTable.Columns.Add("emailType", typeof(string));
            emailUtilityDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var emailUtilityData in emailUtilityDatas)
            {
                var row = emailUtilityDataTable.NewRow();
                row["employeeCode"] = emailUtilityData.EmployeeCode;
                row["employeeName"] = (object)emailUtilityData.EmployeeName ?? DBNull.Value;
                row["currentLevelGrade"] = emailUtilityData.CurrentLevelGrade;
                row["positionGroupName"] = (object)emailUtilityData.PositionGroupName ?? DBNull.Value;
                row["serviceLineName"] = (object)emailUtilityData.ServiceLineName ?? DBNull.Value;
                row["officeName"] = (object)emailUtilityData.OfficeName ?? DBNull.Value;
                row["status"] = emailUtilityData.Status;
                row["retryCount"] = (object)emailUtilityData.RetryCount ?? DBNull.Value;
                row["exception"] = (object)emailUtilityData.Exception ?? DBNull.Value;
                row["date"] = emailUtilityData.Date;
                row["emailType"] = emailUtilityData.EmailType;
                row["lastUpdatedBy"] = emailUtilityData.LastUpdatedBy;
                emailUtilityDataTable.Rows.Add(row);
            }

            return emailUtilityDataTable;
        }
        #endregion

    }
}