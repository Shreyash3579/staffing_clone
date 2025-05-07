using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class ResourceHistoryRepository : IResourceHistoryRepository
    {
        private readonly IBaseRepository<ResourceAllocation> _baseRepository;

        public ResourceHistoryRepository(IBaseRepository<ResourceAllocation> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetHistoricalStaffingAllocationsForEmployee(string employeeCode)
        {
            var resourceHistoryData = await _baseRepository.GetAllAsync(new { employeeCode }, StoredProcedureMap.GetHistoricalStaffingAllocationsForEmployee);
            return resourceHistoryData;
        }
    }
}
