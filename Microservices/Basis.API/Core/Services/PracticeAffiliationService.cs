using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Contracts.Services;
using Basis.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basis.API.Core.Services
{
    public class PracticeAffiliationService: IPracticeAffiliationService
    {
        private readonly IPracticeAffiliationRepository _practiceAffiliationRepository;

        public PracticeAffiliationService(IPracticeAffiliationRepository practiceAffiliationRepository)
        {
            _practiceAffiliationRepository = practiceAffiliationRepository;
        }

        public async Task<IEnumerable<PracticeAffiliation>> GetAllPracticeAffiliation() 
        {
            return await _practiceAffiliationRepository.GetAllPracticeAffiliation();
        }
    }
}
