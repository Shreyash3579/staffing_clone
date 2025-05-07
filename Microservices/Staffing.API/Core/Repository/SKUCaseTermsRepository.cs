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
    public class SKUCaseTermsRepository : ISKUCaseTermsRepository
    {
        private readonly IBaseRepository<SKUCaseTerms> _baseRepository;

        public SKUCaseTermsRepository(IBaseRepository<SKUCaseTerms> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<SKUTerm>> GetSKUTermList()
        {
            var skuTermList = await Task.Run(() => _baseRepository.Context.Connection.Query<SKUTerm>(
                StoredProcedureMap.GetSKUTermList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return skuTermList;
        }

        public async Task<IEnumerable<SKUCaseTerms>> GetSKUTermsForOpportunity(Guid pipelineId)
        {
            var skuCaseTerms = await
                _baseRepository.GetAllAsync(new { pipelineId },
                    StoredProcedureMap.GetSKUTermsForOpportunity);

            return skuCaseTerms;
        }

        public async Task<IEnumerable<SKUCaseTerms>> GetSKUTermsForCase(string oldCaseCode)
        {
            var skuCaseTerms = await
                _baseRepository.GetAllAsync(new { oldCaseCode },
                    StoredProcedureMap.GetSKUTermsForCase);

            return skuCaseTerms;
        }

        public async Task<SKUCaseTerms> InsertSKUCaseTerms(SKUCaseTerms skuCaseTerms)
        {
            var savedCaseTerms = await
                _baseRepository.InsertAsync(new
                {
                    skuCaseTerms.OldCaseCode,
                    skuCaseTerms.PipelineId,
                    skuCaseTerms.SKUTermsCodes,
                    skuCaseTerms.EffectiveDate,
                    skuCaseTerms.LastUpdatedBy
                }, StoredProcedureMap.InsertSKUCaseTerms);

            return savedCaseTerms;
        }

        public async Task<SKUCaseTerms> UpdateSKUCaseTerms(SKUCaseTerms skuCaseTerms)
        {
            var updatedCaseTerms = await
                _baseRepository.UpdateAsync(new
                {
                    skuCaseTerms.Id,
                    skuCaseTerms.OldCaseCode,
                    skuCaseTerms.PipelineId,
                    skuCaseTerms.SKUTermsCodes,
                    skuCaseTerms.EffectiveDate,
                    skuCaseTerms.LastUpdatedBy
                }, StoredProcedureMap.UpdateSKUCaseTerms);

            return updatedCaseTerms;
        }

        public async Task DeleteSKUCaseTermsById(Guid Id)
        {
            await _baseRepository.DeleteAsync(new { Id }, StoredProcedureMap.DeleteSKUCaseTerms);
        }

        public async Task<IEnumerable<SKUCaseTerms>> GetSKUTermsForCaseOrOpportunityForDuration(string oldCaseCodes, string pipelineIds, DateTime startDate, DateTime endDate)
        {
            var skuCaseTerms = await
                _baseRepository.GetAllAsync(new { startDate, endDate, oldCaseCodes, pipelineIds },
                    StoredProcedureMap.GetSKUTermsForOldCaseCodeOrPipelineIdForDuration);

            return skuCaseTerms ?? Enumerable.Empty<SKUCaseTerms>();
        }


    }
}
