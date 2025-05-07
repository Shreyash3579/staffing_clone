using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Core.Helpers;
using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Core.Repository
{
    public class CaseTypeRepository : ICaseTypeRepository
    {
        private readonly IBaseRepository<CaseType> _baseRepository;

        public CaseTypeRepository(IBaseRepository<CaseType> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<CaseType>> GetCaseTypeList()
        {
            var caseTypes = await
                 _baseRepository.GetAllAsync(StoredProcedureMap.GetCaseTypeList);

            return caseTypes;
        }
    }
}
