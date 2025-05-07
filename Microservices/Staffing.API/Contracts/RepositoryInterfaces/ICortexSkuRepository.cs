using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ICortexSkuRepository
    {
        Task<IEnumerable<CortexSkuMapping>> GetCortexSkuMappings();
        Task<IEnumerable<CaseOppCortexTeamSize>> UpsertPlaceholderCreatedForCortexSKUs(CaseOppCortexTeamSize caseOppCortexTeamSize);
        Task<IEnumerable<CaseOppCortexTeamSize>> GetOppCortexPlaceholderInfoByPipelineIds(string pipelineIds);
        Task<IEnumerable<CaseOppCortexTeamSize>> UpsertPricingSKU(CaseOppCortexTeamSize caseOppTeamSize);
        Task<IEnumerable<PricingSkuViewModel>> UpsertPricingSkuDataLog(DataTable pricingTeamSizeDataTable);
    }
}
