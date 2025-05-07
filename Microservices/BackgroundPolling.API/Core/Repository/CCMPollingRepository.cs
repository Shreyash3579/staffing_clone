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
    public class CCMPollingRepository : ICCMPollingRepository
    {
        private readonly IBaseRepository<string> _baseRepository;
        public CCMPollingRepository(IBaseRepository<string> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IList<Guid>> GetOpportunitiesNotConvertedToCase()
        {
            var opportunities = await Task.Run(() => _baseRepository.Context.Connection.Query<Guid>(
                StoredProcedureMap.GetOpportunitiesNotConvertedToCase,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());
            return opportunities;
        }

        public async Task<IList<Guid>> GetOpportunitiesPinnedByUsers()
        {
            var opportunities = await Task.Run(() => _baseRepository.Context.Connection.Query<string>(
                StoredProcedureMap.GetOpportunitiesPinnedByUsers,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());
            return opportunities.Where(x => !string.IsNullOrEmpty(x)).Select(Guid.Parse).ToList();
        }

        public async Task UpdateSkuTermsForOpportunitiesConvertedToCase(DataTable opportunityCaseMapDataTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
                StoredProcedureMap.UpdateSkuTermsForOpportunitiesConvertedToCase,
                new
                {
                    opportunitiesConvertedToCase =
                        opportunityCaseMapDataTable.AsTableValuedParameter(
                            "[dbo].[OpportunityConvertedToCaseTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpdateUserPreferencesForOpportunitiesConvertedToCase(DataTable opportunityCaseMapDataTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
                StoredProcedureMap.UpdateUserPreferencesForOpportunitiesConvertedToCase,
                new
                {
                    opportunitiesConvertedToCase =
                        opportunityCaseMapDataTable.AsTableValuedParameter(
                            "[dbo].[OpportunityConvertedToCaseTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpdateRingfenceForOpportunitiesConvertedToCase(DataTable opportunityCaseMapDataTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
                StoredProcedureMap.UpdateRingfenceForOpportuntiesConvertedToCase,
                new
                {
                    opportunitiesConvertedToCase =
                        opportunityCaseMapDataTable.AsTableValuedParameter(
                            "[dbo].[OpportunityConvertedToCaseTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpsertCaseMasterAndCaseMasterHistory(DataTable caseMasterDataTable, DataTable caseMasterHistoryDataTable, DateTime? lastPolledDateTime)
        {
            await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query<DateTime>(
                StoredProcedureMap.UpsertCaseMasterAndCaseMasterHistoryFromCCM,
                new
                {
                    caseMasterRecords =
                        caseMasterDataTable.AsTableValuedParameter(
                            "[basis].[caseMasterTableType]"),
                    caseMasterHistoryRecords =
                        caseMasterHistoryDataTable.AsTableValuedParameter(
                            "[basis].[caseMasterHistoryTableType]"),
                    lastPolledDateTime
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod));            
        }

        public async Task UpsertCaseAdditionalInfo(DataTable caseAdditionalInfoDataTable, bool isFullLoad = false)
        {
            await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query<DateTime>(
                StoredProcedureMap.UpsertCaseAdditionalInfo, 
                new
                {
                    CaseAdditionalInfo =
                            caseAdditionalInfoDataTable.AsTableValuedParameter(
                                "[basis].[caseAdditionalInfoTableType]"),
                    isFullLoad
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod));
        }
        public async Task UpsertCaseAdditionalInfoInBasis(DataTable caseAdditionalInfoDataTable, bool isFullLoad = false)
        {
            await Task.Run(() => _baseRepository.Context.BasisConnection.Query<DateTime>(
                StoredProcedureMap.UpsertCaseAdditionalInfo,
                new
                {
                    CaseAdditionalInfo =
                            caseAdditionalInfoDataTable.AsTableValuedParameter(
                                "[basis].[caseAdditionalInfoTableType]"),
                    isFullLoad
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod));
        }
        public async Task UpsertCurrencyRates(DataTable currencyRateDataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                    StoredProcedureMap.UpsertCurrencyRate,
                    new
                    {
                        currencyRate =
                            currencyRateDataTable.AsTableValuedParameter(
                                "[basis].[currencyRateTableType]")
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }
        public async Task UpsertCaseAttributes(DataTable caseAttributeDataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                    StoredProcedureMap.UpsertCaseAttribute,
                    new
                    {
                        caseAttribute =
                            caseAttributeDataTable.AsTableValuedParameter(
                                "[basis].[caseAttributeTableType]")
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }


        public async Task<IEnumerable<CurrencyRate>> GetCurrencyRatesChangedRecently(DateTime lastUpdated)
        {
            var currencyRates = await _baseRepository.Context.AnalyticsConnection.QueryAsync<CurrencyRate>(
                  StoredProcedureMap.GetCurrencyRatesUpdatedRecently,
                  new { lastUpdated },
                  commandType: CommandType.StoredProcedure,
                  commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return currencyRates;
        }
        public async Task UpdateUSDCostForChangeInCurrencyRate(DataTable currencyRateDataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                    StoredProcedureMap.UpdateUSDCostForChangeInCurrencyRate,
                    new
                    {
                        currencyRate =
                            currencyRateDataTable.AsTableValuedParameter(
                                "[basis].[currencyRateTableType]")
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }
    }
}
