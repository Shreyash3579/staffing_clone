using Dapper;
using Hcpd.API.Contracts.RepositoryInterfaces;
using Hcpd.API.Core.Helpers;
using Hcpd.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Hcpd.API.Core.Repository
{
    public class HcpdRepository : IHcpdRepository
    {
        private readonly IBaseRepository<Advisor> _baseRepository;

        public HcpdRepository(IBaseRepository<Advisor> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<Advisor> GetAdvisorByEmployeeCode(string employeeCode)
        {
            var result = await
                _baseRepository.GetByDynamicAsync(new { employeeCode },
                    StoredProcedureMap.GetAdvisorByEmployee);

            return result;
        }

        public async Task<IEnumerable<Mentee>> GetMenteesByEmployeeCode(string employeeCode)
        {
            var result = await
                _baseRepository.Context.Connection
                .QueryAsync<Mentee>(
                    StoredProcedureMap.GetMenteesByEmployee,
                    new { employeeCode },
                    commandType: CommandType.StoredProcedure
                );

            return result;
        }
    }
}
