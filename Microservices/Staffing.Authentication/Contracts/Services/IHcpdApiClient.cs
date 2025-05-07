using Staffing.Authentication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Authentication.Contracts.Services
{
    public interface IHcpdApiClient
    {
        Task<IEnumerable<HcpdSecurityUser>> GetSecurityUserDetails(string employeeCode);
    }
}
