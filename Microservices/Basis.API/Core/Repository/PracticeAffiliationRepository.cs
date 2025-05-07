using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Core.Helpers;
using Basis.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Core.Repository
{
    public class PracticeAffiliationRepository: IPracticeAffiliationRepository
    {
        private readonly IBaseRepository<PracticeAffiliation> _baseRepository;

        public PracticeAffiliationRepository(IBaseRepository<PracticeAffiliation> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<PracticeAffiliation>> GetAllPracticeAffiliation() 
        {
            return await _baseRepository.GetAllAsync(null, StoredProcedureMap.GetAllPracticeAffiliation);
        }
    }
}
