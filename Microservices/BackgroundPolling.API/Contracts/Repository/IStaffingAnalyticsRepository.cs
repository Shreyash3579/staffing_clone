using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IStaffingAnalyticsRepository
    {
        public Task<IEnumerable<AuditLog>> GetAuditLogsForSelectedUserAndDate(string staffingUsers, DateTime auditLogsFromDate);
        public Task<IEnumerable<CaseViewModel>> GetCasesServedByRingfenceByOfficeAndCaseType(string officeCodes, string caseTypeCodes);
        public Task<IEnumerable<CADMismatchLog>> GetAnalyticsRecordsNotSyncedWithCAD();
        public Task<string> UpsertEmployeeConsildatedDataForSearch(bool isFullLoad);
    }
}
