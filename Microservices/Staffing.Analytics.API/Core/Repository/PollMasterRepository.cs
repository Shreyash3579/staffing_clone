using Dapper;
using Staffing.Analytics.API.Contracts.Helpers;
using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Core.Helpers;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Repository
{
    public class PollMasterRepository : IPollMasterRepository
    {
        private readonly IBaseRepository<string> _baseRepository;

        public PollMasterRepository(IBaseRepository<string> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;
        }
        public async Task<DateTime> GetLastPolledTimeStamp(string processName)
        {
            var lastPolledTimeStamp = await Task.Run(() => _baseRepository.Context.Connection.Query<DateTime>(
                StoredProcedureMap.GetLastPolledTimeStamp,
                new { processName },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            // SQL accepts min date as 01-Jan-1753
            return lastPolledTimeStamp.Any() ? lastPolledTimeStamp.First() : new DateTime(1753,1,1);
        }
        public async Task UpsertPollMaster(string processName, DateTime timeStamp)
        {
            await Task.Run(() => _baseRepository.Context.Connection.Query<DateTime>(
                StoredProcedureMap.UpsertPollMaster,
                new { processName, timeStamp },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

        }
    }
}
