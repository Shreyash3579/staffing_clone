using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class CaseOppChangesRepository : ICaseOppChangesRepository
    {
        private readonly IBaseRepository<CaseOppChanges> _baseRepository;
       
        public CaseOppChangesRepository(IBaseRepository<CaseOppChanges> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<CaseOppChanges>> GetPipelineChangesByPipelineIds(string pipelineIds)
        {
            var pipelineChangesData = await
               _baseRepository.GetAllAsync(new { pipelineIds },
                   StoredProcedureMap.GetPipelineChangesByPipelineIds);

            return pipelineChangesData;
        }

        public async Task<CaseOppChanges> UpsertPipelineChanges(CaseOppChanges updatedData)
        {
            var updatedPipelineData = await
                _baseRepository.UpdateAsync(new
                {
                    updatedData.PipelineId,
                    updatedData.StartDate,
                    updatedData.EndDate,
                    updatedData.ProbabilityPercent,
                    updatedData.Notes,
                    updatedData.CaseServedByRingfence,
                    updatedData.StaffingOfficeCode,
                    updatedData.LastUpdatedBy
                }, StoredProcedureMap.UpsertPipelineChanges);

            return updatedPipelineData;
        }

        public async Task<IEnumerable<CaseOppChanges>> GetPipelineChangesByDateRange(DateTime startDate, DateTime? endDate)
        {
            var pipelineChangesData = await
               _baseRepository.GetAllAsync(new { startDate, endDate },
                   StoredProcedureMap.GetPipelineChangesByDateRange);

            return pipelineChangesData;
        }

        public async Task<IEnumerable<CaseOppChanges>> GetCaseChangesByOldCaseCodes(string oldCaseCodes)
        {
            var caseChangesData = await
               _baseRepository.GetAllAsync(new { oldCaseCodes },
                   StoredProcedureMap.GetCaseChangesByOldCaseCodes);

            return caseChangesData;
        }

        public async Task<IEnumerable<CaseOppCortexTeamSize>> GetCaseTeamSizeByOldCaseCodes(string oldCaseCodes)
        {
            var caseDataWithTeamSize = await Task.Run(() => _baseRepository.Context.Connection.Query<CaseOppCortexTeamSize>(
                StoredProcedureMap.GetCaseTeamSizeByOldCaseCodes,
                new { oldCaseCodes },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return caseDataWithTeamSize;
        }

        public async Task<CaseOppChanges> UpsertCaseChanges(CaseOppChanges updatedData)
        {
            var updatedCaseData = await
                _baseRepository.UpdateAsync(new
                {
                    updatedData.PegOpportunityId,
                    updatedData.OldCaseCode,
                    updatedData.StartDate,
                    updatedData.EndDate,
                    updatedData.Notes,
                    updatedData.CaseServedByRingfence,
                    updatedData.StaffingOfficeCode,
                    updatedData.LastUpdatedBy
                }, StoredProcedureMap.UpsertCaseChanges);

            return updatedCaseData;
        }

        public async Task<IEnumerable<CaseOppChanges>> GetCaseChangesByDateRange(DateTime startDate, DateTime? endDate)
        {
            var caseChangesData = await
               _baseRepository.GetAllAsync(new { startDate, endDate },
                   StoredProcedureMap.GetCaseChangesByDateRange);

            return caseChangesData;
        }

        public async Task<IEnumerable<CaseOppChanges>> GetCaseOppChangesByOfficesAndDateRange(string officeCodes, DateTime? startDate = null, DateTime? endDate = null)
        {
            var caseOppChangesData = await
               _baseRepository.GetAllAsync(new { officeCodes, startDate, endDate },
                   StoredProcedureMap.GetCaseOppChangesByOfficesAndDateRange);

            return caseOppChangesData;
        }

    }
}
