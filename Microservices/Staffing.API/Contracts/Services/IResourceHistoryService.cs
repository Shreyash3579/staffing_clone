using Staffing.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IResourceHistoryService
    {
        Task<IEnumerable<ResourceAllocationViewModel>> GetHistoricalStaffingAllocationsForEmployee(string employeeCode);
    }
}