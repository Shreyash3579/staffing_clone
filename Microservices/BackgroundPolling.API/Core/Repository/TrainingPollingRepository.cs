using BackgroundPolling.API.Contracts.Helpers;
using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class TrainingPollingRepository : ITrainingPollingRepository
    {
        public IBaseRepository<Training> _baseRepository;

        public TrainingPollingRepository(IBaseRepository<Training> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task UpsertTrainings(DataTable trainingDataTable)
        {
             await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.upsertTrainings,
                new
                {
                    trainings =
                        trainingDataTable.AsTableValuedParameter(
                            "[bvu].[trainingMasterTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }
    }
}
