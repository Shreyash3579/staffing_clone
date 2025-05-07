using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Contracts.RepositoryInterfaces
{
    public interface ICaseOpportunityMapService
    {
        Task<IList<CaseOpportunityMap>> GetCasesForOpportunityConversion(string pipelineIds);
    }
}
