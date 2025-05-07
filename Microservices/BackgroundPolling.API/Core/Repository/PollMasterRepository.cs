using BackgroundPolling.API.Contracts.Helpers;
using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class PollMasterRepository : IPollMasterRepository
    {
        private readonly IBaseRepository<string> _baseRepository;

        public PollMasterRepository(IBaseRepository<string> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<DateTime> GetLastPolledTimeStampFromStaffingDB(string processName)
        {
            var lastPolledTimeStamp = await Task.Run(() => _baseRepository.Context.Connection.Query<DateTime>(
                StoredProcedureMap.GetLastPolledTimeStamp,
                new { processName },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return lastPolledTimeStamp.FirstOrDefault();
        }
        public async Task UpsertPollMasterOnStaffingDB(string processName, DateTime timeStamp)
        {
            await Task.Run(() => _baseRepository.Context.Connection.Query<DateTime>(
                StoredProcedureMap.UpsertPollMaster,
                new { processName, timeStamp },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

        }

        public async Task<DateTime> GetLastPolledTimeStampFromAnalyticsDB(string processName)
        {
            var lastPolledTimeStamp = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query<DateTime>(
                StoredProcedureMap.GetLastPolledTimeStamp,
                new { processName },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return lastPolledTimeStamp.FirstOrDefault();
        }
        public async Task UpsertPollMasterOnAnalyticsDB(string processName, DateTime timeStamp)
        {
            await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query<DateTime>(
                StoredProcedureMap.UpsertPollMaster,
                new { processName, timeStamp },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

        }
    }
}
