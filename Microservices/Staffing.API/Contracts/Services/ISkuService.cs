using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface ISkuService
    {
        Task<IEnumerable<SkuDemand>> GetSkuForProjects(string oldCaseCodes, string pipelineIds, string planningCardIds);
        Task<Sku> UpsertSkuForProject(Sku skus);
    }
}
