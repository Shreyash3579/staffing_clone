using Dapper;
using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Repository
{
    public class ResourceAvailabilityRepository : IResourceAvailabilityRepository
    {
        private readonly IBaseRepository<ResourceAvailability> _baseRepository;

        public ResourceAvailabilityRepository(IBaseRepository<ResourceAvailability> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<ResourceAvailability>> UpdateCostForResourcesAvailableInFullCapacity(
            DataTable resourcesAvailableInFullCapacityWithCost)
        {
            var allocations = await _baseRepository.Context.Connection.QueryAsync<ResourceAvailability>(
                StoredProcedureMap.UpdateCostForResourcesAvailableInFullCapacity,
                new
                {
                    resourcesOpportunityCost =
                        resourcesAvailableInFullCapacityWithCost.AsTableValuedParameter(
                            "[dbo].[analyticsResourceAvailabilityTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return allocations;
        }
    }
}
