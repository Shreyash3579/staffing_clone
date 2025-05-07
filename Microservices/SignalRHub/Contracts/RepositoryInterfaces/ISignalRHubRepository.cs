using SignalRHub.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalRHub.Contracts.RepositoryInterfaces
{
    public interface ISignalRHubRepository
    {
        Task<IEnumerable<UserConnectionMapping>> GetSignalRConnectionStringForUsers(string employeeCode);
        Task<UserConnectionMapping> UpsertSignalRConnectionStringForUser(string employeeCode, string connectionString);
    }
}
