using Iris.API.Contracts.Helpers;
using Iris.API.Contracts.Helpers.RepositoryInterfaces;
using Iris.API.Contracts.RepositoryInterfaces;
using Iris.API.Core.Helpers;
using Iris.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iris.API.Core.Repository
{
    public class PracticeAreaRepository : IPracticeAreaRepository
    {
        private readonly IBaseRepository<PracticeArea> _baseRepository;

        public PracticeAreaRepository(IBaseRepository<PracticeArea> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;

        }

        public async Task<IEnumerable<PracticeArea>> GetAllCapabilityPracticeArea()
        {
            var practiceAreas = await _baseRepository.GetAllAsync(StoredProcedureMap.GetAllCapabilityPracticeArea);
            return practiceAreas;
        }
        public async Task<IEnumerable<PracticeArea>> GetAllIndustryPracticeArea()
        {
            var practiceAreas = await _baseRepository.GetAllAsync(StoredProcedureMap.GetAllIndustryPracticeArea);
            return practiceAreas;
        }
    }
}
