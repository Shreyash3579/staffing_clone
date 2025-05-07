using CCM.API.Contracts.Helpers;
using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Core.Helpers;
using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Core.Repository
{
    public class LookupRepository : ILookupRepository
    {
        private readonly IBaseRepository<CaseAttribute> _baseRepository;

        public LookupRepository(IBaseRepository<CaseAttribute> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;
        }

        public async Task<IEnumerable<CaseAttribute>> GetCaseAttributeLookupList()
        {
            var caseAttributes = await _baseRepository.GetAllAsync(null,
                StoredProcedureMap.GetCaseAttributeLookupList);

            return caseAttributes;
        }
    }
}