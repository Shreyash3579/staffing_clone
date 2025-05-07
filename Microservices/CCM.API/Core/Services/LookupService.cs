using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Contracts.Services;
using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Core.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _lookupRepository;

        public LookupService(ILookupRepository lookupRepository)
        {
            _lookupRepository = lookupRepository;
        }

        public async Task<IEnumerable<CaseAttribute>> GetCaseAttributeLookupList()
        {
            return await _lookupRepository.GetCaseAttributeLookupList();
        }
    }
}