using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class DataSyncMismatchRepository : IDataSyncMismatchRepository
    {
        private readonly IBaseRepository<MismatchLog> _baseRepository;

        public DataSyncMismatchRepository(IBaseRepository<MismatchLog> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;

        }

        public async Task<IEnumerable<MismatchLog>> GetCountforSyncTablesInStaffing()
        {
            var mismatchLogs = await Task.Run(() => _baseRepository.Context.Connection.Query<MismatchLog>(
                StoredProcedureMap.GetCountforSyncTablesInStaffing,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return mismatchLogs;
        }
    }
}
