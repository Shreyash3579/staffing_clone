using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Core.Services
{
    public class CaseOpportunityMapService : ICaseOpportunityMapService
    {
        private readonly ICaseOpportunityMapRepository _caeOpportunityMapRepository;

        public CaseOpportunityMapService(ICaseOpportunityMapRepository caeOpportunityMapRepository)
        {
            _caeOpportunityMapRepository = caeOpportunityMapRepository;
        }

        public Task<IList<CaseOpportunityMap>> GetCasesForOpportunityConversion(string pipelineIds)
        {
            return _caeOpportunityMapRepository.GetCasesForOpportunityConversion(pipelineIds);
        }
    }
}