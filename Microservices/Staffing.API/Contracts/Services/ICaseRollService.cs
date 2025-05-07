using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface ICaseRollService
    {
        Task<IEnumerable<CaseRoll>> GetAllUnprocessedCasesOnCaseRoll();
        Task<IEnumerable<CaseRoll>> GetCasesOnRollByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<CaseRoll>> UpsertCaseRolls(IEnumerable<CaseRoll> upsertedCaseRolls);
        Task<string> DeleteCaseRollsByIds(string caseRollIdsToDelete, string lastUpdatedBy);
        Task<string> DeleteRolledAllocationsByScheduleIds(string rolledScheduleIds, string lastUpdatedBy);
        Task DeleteRolledAllocationsMappingFromCaseRollTracking(string lastUpdatedBy, string rolledCaseCodes);
        Task<string> GetCaseRollsRecentlyProcessedInStaffing(DateTime lastPollDateTime);

    }
}
