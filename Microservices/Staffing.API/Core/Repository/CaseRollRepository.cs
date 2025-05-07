using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class CaseRollRepository : ICaseRollRepository
    {
        private readonly IBaseRepository<CaseRoll> _baseRepository;

        public CaseRollRepository(IBaseRepository<CaseRoll> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<CaseRoll>> GetAllUnprocessedCasesOnCaseRoll()
        {
            var unprocessdCasesOnRoll = await _baseRepository.GetAllAsync(StoredProcedureMap.GetAllUnprocessedCasesOnCaseRoll);

            return unprocessdCasesOnRoll;
        }

        public async Task<IEnumerable<CaseRoll>> GetCasesOnRollByCaseCodes(string oldCaseCodes)
        {
            var casesOnRoll = await _baseRepository.GetAllAsync(new { oldCaseCodes }, StoredProcedureMap.GetCasesOnRollByCaseCodes);

            return casesOnRoll;
        }

        public async Task<IEnumerable<CaseRoll>> UpsertCaseRolls(DataTable caseRollsDataTable)
        {
            var upsertedCaseRolls = await _baseRepository.Context.Connection.QueryAsync<CaseRoll>(
                StoredProcedureMap.UpsertCaseRolls,
                new
                {
                    caseRolls =
                        caseRollsDataTable.AsTableValuedParameter(
                            "[dbo].[CaseRollTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedCaseRolls;
        }

        public async Task DeleteCaseRollsByIds(string caseRollIdsToDelete, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { caseRollIdsToDelete, lastUpdatedBy }, StoredProcedureMap.DeleteCaseRollsByIds);

        }

        public async Task DeleteRolledAllocationsByScheduleIds(string rolledScheduleIdsToDelete, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { @rolledScheduleIdsToDelete, lastUpdatedBy }, StoredProcedureMap.DeleteRolledAllocationsByScheduleIds);
        }

        public async Task DeleteRolledAllocationsMappingFromCaseRollTracking(string lastUpdatedBy, string rolledCaseCodes)
        {
           await _baseRepository.DeleteAsync(new { lastUpdatedBy, rolledCaseCodes }, StoredProcedureMap.DeleteRolledAllocationsMappingFromCaseRollTracking);

        }

        public async Task<string> GetCaseRollsRecentlyProcessedInStaffing(DateTime lastPollDateTime)
        {
            var caseRollsRecentlyProcessedInStaffing = await _baseRepository.Context.Connection.QueryFirstOrDefaultAsync<string>(
                StoredProcedureMap.GetCaseRollsRecentlyProcessedInStaffing,
                new
                {
                    lastPollDateTime = lastPollDateTime
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return caseRollsRecentlyProcessedInStaffing;
        }
    }
}
