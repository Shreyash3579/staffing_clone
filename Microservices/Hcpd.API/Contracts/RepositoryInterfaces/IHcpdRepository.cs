using Hcpd.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hcpd.API.Contracts.RepositoryInterfaces
{
    public interface IHcpdRepository
    {
        Task<Advisor> GetAdvisorByEmployeeCode(string employeeCode);

        Task<IEnumerable<Mentee>> GetMenteesByEmployeeCode(string employeeCode);
    }
}
