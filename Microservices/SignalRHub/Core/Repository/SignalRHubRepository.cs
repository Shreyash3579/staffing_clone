using SignalRHub.Contracts.RepositoryInterfaces;
using SignalRHub.Core.Helpers;
using SignalRHub.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRHub.Core.Repository
{
    public class SignalRHubRepository : ISignalRHubRepository
    {
        private readonly IBaseRepository<UserConnectionMapping> _baseRepository;
        public SignalRHubRepository(IBaseRepository<UserConnectionMapping> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<UserConnectionMapping>> GetSignalRConnectionStringForUsers(string employeeCodes)
        {
            var connectionMapping = await _baseRepository.GetAllAsync(new { employeeCodes }, StoredProcedureMap.GetSignalRConnectionStringForUsers);
            return connectionMapping;
        }

        public async Task<UserConnectionMapping> UpsertSignalRConnectionStringForUser(string employeeCode, string connectionString)
        {
            var upsertedConnectionMapping = await _baseRepository.UpsertAsync(StoredProcedureMap.UpsertSignalRConnectionStringForUser,
                new
                {
                    employeeCode = employeeCode,
                    connectionString = connectionString,
                    lastUpdatedBy = employeeCode
                });

            return upsertedConnectionMapping;
        }
    }
}
