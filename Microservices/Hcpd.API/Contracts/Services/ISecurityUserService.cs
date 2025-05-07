using Hcpd.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hcpd.API.Contracts.Services
{
    public interface ISecurityUserService
    {
        Task<IEnumerable<SecurityUserViewModel>> GetSecurityUserDetails(string employeeCode);
    }
}
