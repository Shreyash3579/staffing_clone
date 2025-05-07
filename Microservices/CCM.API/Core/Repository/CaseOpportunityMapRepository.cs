using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Core.Helpers;
using CCM.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCM.API.Core.Repository
{
    public class CaseOpportunityMapRepository : ICaseOpportunityMapRepository
    {
        private readonly IBaseRepository<CaseOpportunityMap> _baseRepository;

        public CaseOpportunityMapRepository(IBaseRepository<CaseOpportunityMap> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IList<CaseOpportunityMap>> GetCasesForOpportunityConversion(string pipelineIds)
        {
            var opportunitiesConvertedToCase =
                await _baseRepository.GetAllAsync(new { pipelineIds }, StoredProcedureMap.GetCasesByPipelineIds);
            return opportunitiesConvertedToCase.ToList();
        }
    }
}