using BackgroundPolling.API.Contracts.Helpers;
using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class PipelinePollingRepository : IPipelinePollingRepository
    {
        private readonly IBaseRepository<string> _baseRepository;
        public PipelinePollingRepository(IBaseRepository<string> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task UpsertOpportunitiesFlatData(DataTable opportunitesFlatDataTable, bool isFullLoad)
        {
            await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query<DateTime>(
                StoredProcedureMap.UpsertOpportunitiesFlatData,
                new
                {
                    opportunities =
                            opportunitesFlatDataTable.AsTableValuedParameter(
                                "[pipeline].[opportunityTableType]"),
                    isFullLoad
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod));
        }

        public async Task UpsertOpportunitiesFlatDataInPipeline(DataTable opportunitesFlatDataTable, bool isFullLoad)
        {
            await Task.Run(() => _baseRepository.Context.PipelineConnection.Query<DateTime>(
                StoredProcedureMap.UpsertOpportunitiesFlatDataInPipeline,
                new
                {
                    opportunities =
                            opportunitesFlatDataTable.AsTableValuedParameter(
                                "[staffing].[opportunityTableType]"),
                    isFullLoad
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod));
        }
    }
}
