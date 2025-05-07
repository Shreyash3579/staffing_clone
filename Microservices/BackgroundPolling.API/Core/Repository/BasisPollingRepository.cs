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
    public class BasisPollingRepository : IBasisPollingRepository
    {
        private readonly IBaseRepository<string> _baseRepository;
        public BasisPollingRepository(IBaseRepository<string> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task UpsertPracticeAffiliations(DataTable dataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                   StoredProcedureMap.UpsertPracticeAffiliation,
                   new
                   {
                       practiceAffiliations =
                           dataTable.AsTableValuedParameter(
                               "[basis].[practiceAffiliationTableType]")
                   },
                   commandType: CommandType.StoredProcedure,
                   commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }
        public async Task InsertMonthlySnapshotForPracticeAffiliations()
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                   StoredProcedureMap.InsertMonthlySnapshotForPracticeAffiliations,                   
                   commandType: CommandType.StoredProcedure,
                   commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task InsertPracticeAreaLookUpData(DataTable dataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                   StoredProcedureMap.InsertPracticeAreaLookUpData,
                   new
                   {
                       practiceAreas =
                           dataTable.AsTableValuedParameter(
                               "[basis].[practiceAreaTableType]")
                   },
                   commandType: CommandType.StoredProcedure,
                   commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

    }
}
