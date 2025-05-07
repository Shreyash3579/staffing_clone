using Pipeline.API.Contracts.RepositoryInterfaces;
using Pipeline.API.Contracts.Services;
using Pipeline.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pipeline.API.Core.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _lookupRepository;

        public LookupService(ILookupRepository lookupRepository)
        {
            _lookupRepository = lookupRepository;
        }

        public async Task<IEnumerable<OpportunityStatusType>> GetOpportunityStatusTypeList()
        {
            return await _lookupRepository.GetOpportunityStatusTypeList();
        }
    }
}
