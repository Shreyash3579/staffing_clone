using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class StaffingPollingRepository : IStaffingPollingRepository
    {
        private readonly IBaseRepository<string> _baseRepository;

        public StaffingPollingRepository(IBaseRepository<string> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<string> DeleteSecurityUsersWithExpiredEndDate()
        {
            var deletedSecurityUsers = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<PolarisSecurityUser>(
                StoredProcedureMap.DeleteSecurityUsersWithExpiredEndDate,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod));
            return string.Join(',', deletedSecurityUsers.ToList().Select(x => x.EmployeeCode));
        }

        public async Task DeleteAnalyticsLog()
        {
            var deletedSecurityUsers = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.DeleteAnalyticsLog,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod));
        }

        public async Task UpsertSMAPMissions(DataTable dataTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
               StoredProcedureMap.UpsertSMAPMissions,
               new
               {
                   smapMissions =
                       dataTable.AsTableValuedParameter(
                           "[dbo].[smapMissionTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpsertStaffingPreferencesFromSharepoint(DataTable dataTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
               StoredProcedureMap.UpsertStaffingPreferencesFromSharepoint,
               new
               {
                   employeeStaffingPreferences =
                       dataTable.AsTableValuedParameter(
                           "[dbo].[employeeStaffingPreferenceFromSharepointType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpsertStaffingPreferencesFromSharepointToAnalyticsDB(DataTable dataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
               StoredProcedureMap.UpsertStaffingPreferencesFromSharepoint,
               new
               {
                   employeeStaffingPreferences =
                       dataTable.AsTableValuedParameter(
                           "[dbo].[employeeStaffingPreferenceFromSharepointType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }
    }
}
