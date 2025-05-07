using Iris.API.Contracts.RepositoryInterfaces;
using Iris.API.Contracts.Services;
using Iris.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iris.API.Core.Services
{
    public class PracticeAreaService : IPracticeAreaService
    {
        private readonly IPracticeAreaRepository _practiceAreaRepository;

        public PracticeAreaService(IPracticeAreaRepository practiceAreaRepository)
        {
            _practiceAreaRepository = practiceAreaRepository;
        }
        public async Task<IEnumerable<PracticeArea>> GetAllCapabilityPracticeArea()
        {
            var practiceAreas = await _practiceAreaRepository.GetAllCapabilityPracticeArea();
            return practiceAreas.Distinct();
        }
        public async Task<IEnumerable<PracticeArea>> GetAllIndustryPracticeArea()
        {
            var practiceAreas = await _practiceAreaRepository.GetAllIndustryPracticeArea();
            return practiceAreas.Distinct();
        }
    }
}
