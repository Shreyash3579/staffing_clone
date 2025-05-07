using Staffing.Analytics.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.RepositoryInterfaces
{
    public interface IResourceAvailabilityRepository
    {
        Task<IEnumerable<ResourceAvailability>> UpdateCostForResourcesAvailableInFullCapacity(DataTable resourcesAvailableInFullCapacityWithCost);
    }
}
