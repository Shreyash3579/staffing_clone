using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using System.Linq;

namespace Staffing.API.Core.Repository
{
    public class SkuRepository : ISkuRepository
    {
        private readonly IBaseRepository<Sku> _baseRepository;

        public SkuRepository(IBaseRepository<Sku> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<SkuDemand>> GetSkuForProjects(string oldCaseCodes, string pipelineIds, string planningCardIds)
        {
            var skuTerms = await Task.Run(() => _baseRepository.Context.Connection.Query<SkuDemand>(
                StoredProcedureMap.GetSkuForProjects,
                new { oldCaseCodes, pipelineIds, planningCardIds },
               commandType: CommandType.StoredProcedure,
               commandTimeout: 180).ToList());

            return skuTerms;
        }

        public async Task<Sku> UpsertSkuForProject(Sku skuData)
        {
            var savedSku = await
                _baseRepository.UpdateAsync(new
                {
                    skuData.Id,
                    skuData.SkuTerm,
                    skuData.OldCaseCode,
                    skuData.PipelineId,
                }, StoredProcedureMap.UpsertSkuForProjects);

            return savedSku;
        }
    }
}
