using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class StaffingAnalyticsRepository : IStaffingAnalyticsRepository
    {
        private readonly IBaseRepository<AuditLog> _baseRepository;

        public StaffingAnalyticsRepository(IBaseRepository<AuditLog> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<AuditLog>> GetAuditLogsForSelectedUserAndDate(string staffingUsers, DateTime auditLogsFromDate)
        {
            var auditLogs = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query<AuditLog>(
                StoredProcedureMap.GetAuditLogsForSelectedUserAndDate,
                new { staffingUsers, auditLogsFromDate },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return auditLogs;
        }
        public async Task<IEnumerable<CaseViewModel>> GetCasesServedByRingfenceByOfficeAndCaseType(string officeCodes, string caseTypeCodes)
        {
            var cases = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query<CaseViewModel>(
                StoredProcedureMap.GetCasesServedByRingfenceByOfficeAndCaseType,
                new { officeCodes, caseTypeCodes },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return cases;
        }

        public async Task<IEnumerable<CADMismatchLog>> GetAnalyticsRecordsNotSyncedWithCAD()
        {
            var mismatchedTimestamps = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query< CADMismatchLog>(
               StoredProcedureMap.GetAnalyticsRecordsNotSyncedWithCAD,
               commandType: CommandType.StoredProcedure,
               commandTimeout: 180));

            return mismatchedTimestamps;
        }

        public async Task<string> UpsertEmployeeConsildatedDataForSearch(bool isFullLoad)
        {
            var procRunMessage = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.QueryFirst<string>(
               StoredProcedureMap.UpsertEmployeeConsildatedDataForSearch,
               new { isFullLoad },
               commandType: CommandType.StoredProcedure,
               commandTimeout: 180));

            return procRunMessage;
        }
    }
}
