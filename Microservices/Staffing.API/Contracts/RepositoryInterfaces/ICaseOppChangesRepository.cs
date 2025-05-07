using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ICaseOppChangesRepository
    {
        Task<IEnumerable<CaseOppChanges>> GetPipelineChangesByPipelineIds(string pipelineIds);
        Task<CaseOppChanges> UpsertPipelineChanges(CaseOppChanges updatedData);
        Task<IEnumerable<CaseOppChanges>> GetPipelineChangesByDateRange(DateTime startDate, DateTime? endDate);
        Task<IEnumerable<CaseOppChanges>> GetCaseChangesByOldCaseCodes(string oldCaseCodes);
        Task<CaseOppChanges> UpsertCaseChanges(CaseOppChanges updatedData);
        Task<IEnumerable<CaseOppChanges>> GetCaseChangesByDateRange(DateTime startDate, DateTime? endDate);
        Task<IEnumerable<CaseOppChanges>> GetCaseOppChangesByOfficesAndDateRange(string officeCodes, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<CaseOppCortexTeamSize>> GetCaseTeamSizeByOldCaseCodes(string oldCaseCodes);
    }
}
