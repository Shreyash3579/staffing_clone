using Hcpd.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hcpd.API.Contracts.RepositoryInterfaces
{
    public interface ISecurityUserRepository
    {
        Task<IEnumerable<SecurityUser>> GetSecurityUserDetails(string employeeCode);
    }
}
