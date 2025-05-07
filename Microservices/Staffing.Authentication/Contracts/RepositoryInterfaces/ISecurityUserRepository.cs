using Staffing.Authentication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Authentication.Contracts.RepositoryInterfaces
{
    public interface ISecurityUserRepository
    {
        Task<IEnumerable<SecurityUser>> Authenticate(string employeeCode);
    }
}
