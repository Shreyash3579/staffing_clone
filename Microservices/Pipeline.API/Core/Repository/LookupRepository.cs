using Pipeline.API.Contracts.Helpers;
using Pipeline.API.Contracts.RepositoryInterfaces;
using Pipeline.API.Core.Helpers;
using Pipeline.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pipeline.API.Core.Repository
{
    public class LookupRepository : ILookupRepository
    {
        private readonly IBaseRepository<OpportunityStatusType> _baseRepository;

        public LookupRepository(IBaseRepository<OpportunityStatusType> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;

        }
        public async Task<IEnumerable<OpportunityStatusType>> GetOpportunityStatusTypeList()
        {
            var investmentCategories =
                await _baseRepository.GetAllAsync(null,
                    StoredProcedureMap.GetOpportunityStatusTypeList);

            return investmentCategories;
        }

    }
}
