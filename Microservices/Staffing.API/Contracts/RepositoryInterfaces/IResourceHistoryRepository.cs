using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IResourceHistoryRepository
    {
        Task<IEnumerable<ResourceAllocation>> GetHistoricalStaffingAllocationsForEmployee(string employeeCode);
    }
}
