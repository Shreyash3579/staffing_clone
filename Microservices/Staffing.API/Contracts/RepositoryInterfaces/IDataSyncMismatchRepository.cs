using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IDataSyncMismatchRepository
    {
        Task<IEnumerable<MismatchLog>> GetCountforSyncTablesInStaffing();
    }
}
