using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IDataSyncMismatchService
    {
        Task<IEnumerable<MismatchLog>> GetCountforSyncTablesInStaffing();
    }
}
