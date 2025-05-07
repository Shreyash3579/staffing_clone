using Hcpd.API.Contracts.RepositoryInterfaces;
using Hcpd.API.Contracts.Services;
using Hcpd.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hcpd.API.Core.Services
{
    public class AdvisorService : IAdvisorService
    {
        private readonly IHcpdRepository _hcpdRepository;

        public AdvisorService(IHcpdRepository hcpdRepository)
        {
            _hcpdRepository = hcpdRepository;
        }

        public async Task<Advisor> GetAdvisorByEmployeeCode(string employeeCode)
        {
            var advisor = await _hcpdRepository.GetAdvisorByEmployeeCode(employeeCode);
            return advisor;
        }

        public async Task<IEnumerable<Mentee>> GetMenteesByEmployeeCode(string employeeCode)
        {
            var mentees = await _hcpdRepository.GetMenteesByEmployeeCode(employeeCode);
            return mentees;
        }
    }
}
