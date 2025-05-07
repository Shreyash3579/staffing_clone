using Staffing.HttpAggregator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IHcpdApiClient
    {
        Task<Advisor> GetAdvisorByEmployeeCode(string employeeCode);

        Task<IEnumerable<Mentee>> GetMenteesByEmployeeCode(string employeeCode);
    }
}
