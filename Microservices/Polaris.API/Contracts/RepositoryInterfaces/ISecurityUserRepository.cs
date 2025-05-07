using Polaris.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polaris.API.Contracts.RepositoryInterfaces
{
    public interface ISecurityUserRepository
    {
        Task<IEnumerable<SecurityUser>> GetRevSecurityUsersWithGeography();
    }
}
