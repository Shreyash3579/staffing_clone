using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class SkuService : ISkuService
    {
        private readonly ISkuRepository _skuRepository;
        
        public SkuService(ISkuRepository skuRepository)
        {
            _skuRepository = skuRepository;
        }

        public async Task<IEnumerable<SkuDemand>> GetSkuForProjects(string oldCaseCodes, string pipelineIds, string planningCardIds)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds) && string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<SkuDemand>();

            var skuData = await _skuRepository.GetSkuForProjects(oldCaseCodes, pipelineIds, planningCardIds);
            return skuData;
        }

        public async Task<Sku> UpsertSkuForProject(Sku skus)
        {
            if (skus == null)
                return new Sku();

            var skuData = await _skuRepository.UpsertSkuForProject(skus);
            return skuData;
        }
    }
}
