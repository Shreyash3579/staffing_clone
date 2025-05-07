using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class CaseRollService : ICaseRollService
    {
        private readonly ICaseRollRepository _caseRollRepository;

        public CaseRollService(ICaseRollRepository caseRollRepository)
        {
            _caseRollRepository = caseRollRepository;
        }

        public async Task<IEnumerable<CaseRoll>> GetAllUnprocessedCasesOnCaseRoll()
        {
            return await _caseRollRepository.GetAllUnprocessedCasesOnCaseRoll();
        }

        public async Task<IEnumerable<CaseRoll>> GetCasesOnRollByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
            {
                throw new ArgumentException("oldCaseCodes cannot be null or empty");
            }
            return await _caseRollRepository.GetCasesOnRollByCaseCodes(oldCaseCodes);
        }

        public async Task<IEnumerable<CaseRoll>> UpsertCaseRolls(IEnumerable<CaseRoll> upsertedCaseRolls)
        {
            var caseRollDataTable = CreateCaseRollDataTable(upsertedCaseRolls);

            return await _caseRollRepository.UpsertCaseRolls(caseRollDataTable);
        }

        public async Task<string> DeleteCaseRollsByIds(string caseRollIdsToDelete, string lastUpdatedBy)
        {
            await _caseRollRepository.DeleteCaseRollsByIds(caseRollIdsToDelete, lastUpdatedBy);
            return caseRollIdsToDelete;
        }

        public async Task<string> DeleteRolledAllocationsByScheduleIds(string rolledScheduleIds, string lastUpdatedBy)
        {
            await _caseRollRepository.DeleteRolledAllocationsByScheduleIds(rolledScheduleIds, lastUpdatedBy);
            return rolledScheduleIds;
        }

        public async Task DeleteRolledAllocationsMappingFromCaseRollTracking(string lastUpdatedBy, string rolledCaseCodes)
        {
            await _caseRollRepository.DeleteRolledAllocationsMappingFromCaseRollTracking(lastUpdatedBy, rolledCaseCodes);
        }
        public async Task<string> GetCaseRollsRecentlyProcessedInStaffing(DateTime lastPollDateTime)
        {
            return await _caseRollRepository.GetCaseRollsRecentlyProcessedInStaffing(lastPollDateTime);
        }

        #region private Helper methods
        private static DataTable CreateCaseRollDataTable(IEnumerable<CaseRoll> caseRolls)
        {
            var caseRollDataTable = new DataTable();
            caseRollDataTable.Columns.Add("id", typeof(Guid));
            caseRollDataTable.Columns.Add("rolledFromOldCaseCode", typeof(string));
            caseRollDataTable.Columns.Add("currentCaseEndDate", typeof(DateTime));
            caseRollDataTable.Columns.Add("expectedCaseEndDate", typeof(DateTime));
            caseRollDataTable.Columns.Add("rolledToOldCaseCode", typeof(string));
            caseRollDataTable.Columns.Add("rolledToPlanningCardId", typeof(Guid));
            caseRollDataTable.Columns.Add("isProcessedFromCCM", typeof(bool));
            caseRollDataTable.Columns.Add("rolledScheduleIds", typeof(string));
            caseRollDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var caseRoll in caseRolls)
            {
                var row = caseRollDataTable.NewRow();
                row["id"] = (object)caseRoll.Id ?? DBNull.Value;
                row["rolledFromOldCaseCode"] = caseRoll.RolledFromOldCaseCode;
                row["currentCaseEndDate"] = (object)caseRoll.CurrentCaseEndDate ?? DBNull.Value;
                row["expectedCaseEndDate"] = (object)caseRoll.ExpectedCaseEndDate ?? DBNull.Value;
                row["rolledToOldCaseCode"] = (object)caseRoll.RolledToOldCaseCode ?? DBNull.Value;
                row["rolledToPlanningCardId"] = (object)caseRoll.RolledToPlanningCardId ?? DBNull.Value;
                row["rolledScheduleIds"] = caseRoll.RolledScheduleIds;
                row["isProcessedFromCCM"] = caseRoll.IsProcessedFromCCM;
                row["lastUpdatedBy"] = caseRoll.LastUpdatedBy;
                caseRollDataTable.Rows.Add(row);
            }

            return caseRollDataTable;
        }
        #endregion
    }
}
