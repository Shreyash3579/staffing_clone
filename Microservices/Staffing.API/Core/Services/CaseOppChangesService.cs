using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class CaseOppChangesService : ICaseOppChangesService
    {
        private readonly ICaseOppChangesRepository _caseOppChangesRepository;

        public CaseOppChangesService(ICaseOppChangesRepository caseChangesRepository)
        {
            _caseOppChangesRepository = caseChangesRepository;
        }

        public async Task<IEnumerable<CaseOppChanges>> GetPipelineChangesByPipelineIds(string pipelineIds)
        {
            return
                await _caseOppChangesRepository.GetPipelineChangesByPipelineIds(pipelineIds);
        }

        public async Task<CaseOppChanges> UpsertPipelineChanges(CaseOppChanges updatedData)
        {
            if (string.IsNullOrEmpty(updatedData.PipelineId.ToString()) || updatedData.PipelineId == Guid.Empty)
            {
                throw new ArgumentException("Pipeline Id cannot be null or empty");
            }
            return
                await _caseOppChangesRepository.UpsertPipelineChanges(updatedData);
        }

        public async Task<IEnumerable<CaseOppChanges>> GetPipelineChangesByDateRange(DateTime startDate, DateTime? endDate)
        {
            if (startDate == DateTime.MinValue)
            {
                throw new ArgumentException("Start Date cannot be null or empty");
            }
            var pipelineChangesByDateRange = await _caseOppChangesRepository.GetPipelineChangesByDateRange(startDate, endDate);

            return pipelineChangesByDateRange;
        }

        public async Task<IEnumerable<CaseOppChanges>> GetCaseChangesByOldCaseCodes(string oldCaseCodes)
        {
            return
                await _caseOppChangesRepository.GetCaseChangesByOldCaseCodes(oldCaseCodes);
        }
        public async Task<IEnumerable<CaseOppCortexTeamSize>> GetCaseTeamSizeByOldCaseCodes(string oldCaseCodes)
        {
            return
                await _caseOppChangesRepository.GetCaseTeamSizeByOldCaseCodes(oldCaseCodes);
        }

        public async Task<CaseOppChanges> UpsertCaseChanges(CaseOppChanges updatedData)
        {
            return string.IsNullOrEmpty(updatedData.OldCaseCode)
                ? throw new ArgumentException("Old Case Code cannot be null or empty")
                : await _caseOppChangesRepository.UpsertCaseChanges(updatedData);
        }

        public async Task<IEnumerable<CaseOppChanges>> GetCaseChangesByDateRange(DateTime startDate, DateTime? endDate)
        {
            if (startDate == DateTime.MinValue)
            {
                throw new ArgumentException("Start Date cannot be null or empty");
            }
            var caseChangesByDateRange = await _caseOppChangesRepository.GetCaseChangesByDateRange(startDate, endDate);

            return caseChangesByDateRange;
        }

        public async Task<IEnumerable<CaseOppChanges>> GetCaseOppChangesByOfficesAndDateRange(string officeCodes, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(officeCodes))
            {
                throw new ArgumentException("Office Codes cannot be null or empty");
            }
            var caseOppChanges = await _caseOppChangesRepository.GetCaseOppChangesByOfficesAndDateRange(officeCodes, startDate, endDate);

            return caseOppChanges;
        }
    }
}
