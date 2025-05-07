using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ICaseRollRepository
    {
        Task<IEnumerable<CaseRoll>> GetAllUnprocessedCasesOnCaseRoll();
        Task<IEnumerable<CaseRoll>> GetCasesOnRollByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<CaseRoll>> UpsertCaseRolls(DataTable caseRollsDataTable);
        Task DeleteCaseRollsByIds(string caseRollIdsToDelete, string lastUpdatedBy);
        Task DeleteRolledAllocationsByScheduleIds(string rolledScheduleIdsToDelete, string lastUpdatedBy);
        Task DeleteRolledAllocationsMappingFromCaseRollTracking(string lastUpdatedBy, string rolledCaseCodes);
        Task<string> GetCaseRollsRecentlyProcessedInStaffing(DateTime lastPollDateTime);
    }
}
