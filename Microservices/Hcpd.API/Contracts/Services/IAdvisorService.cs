using Hcpd.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hcpd.API.Contracts.Services
{
    public interface IAdvisorService
    {
        Task<Advisor> GetAdvisorByEmployeeCode(string employeeCode);

        Task<IEnumerable<Mentee>> GetMenteesByEmployeeCode(string employeeCode);
    }
}
