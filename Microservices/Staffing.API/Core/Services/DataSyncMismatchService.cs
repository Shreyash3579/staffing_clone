using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class DataSyncMismatchService : IDataSyncMismatchService
    {
        private readonly IDataSyncMismatchRepository _dataSyncMismatchRepository;

        public DataSyncMismatchService(IDataSyncMismatchRepository dataSyncMismatchRepository)
        {
            _dataSyncMismatchRepository = dataSyncMismatchRepository;
        }
        public async Task<IEnumerable<MismatchLog>> GetCountforSyncTablesInStaffing()
        {
            return await _dataSyncMismatchRepository.GetCountforSyncTablesInStaffing();
        }
    }
}
