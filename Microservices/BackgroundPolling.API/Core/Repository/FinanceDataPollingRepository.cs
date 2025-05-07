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
    public class FinanceDataPollingRepository : IFinanceDataPollingRepository
    {
        private readonly IBaseRepository<Office> _baseRepository;

        public FinanceDataPollingRepository(IBaseRepository<Office> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<Office>> GetOfficeList()
        {
            var records = await Task.Run(() => _baseRepository.Context.Connection.Query<Office>(
                StoredProcedureMap.GetOfficeList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return records;
        }

        public async Task UpsertBillRates(DataTable billRates)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpsertBillRates,
                new
                {
                    billRates =
                        billRates.AsTableValuedParameter(
                            "[ccm].[billRatesTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpsertRevenueTransactions(DataTable revenueTransactions)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpsertRevenueTransactions,
                new
                {
                    revenueTransactions =
                        revenueTransactions.AsTableValuedParameter(
                            "[ccm].[revenueTransactionTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 1500);
        }

        public async Task DeleteRevenueTransactionsById(string ids)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.DeleteRevenueTransactionsById,
                new
                {
                    ids
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task<DateTime> GetLastUpdatedBillRateDate()
        {
            var date = await _baseRepository.Context.AnalyticsConnection.QueryAsync<DateTime>(
                StoredProcedureMap.GetLastUpdatedBillRateDate,
                null,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return date.FirstOrDefault();
        }

        public async Task<DateTime> GetLastUpdatedRevenueTransactionDate()
        {
            var date = await _baseRepository.Context.AnalyticsConnection.QueryAsync<DateTime>(
                StoredProcedureMap.GetLastUpdatedRevenueTransactionDate,
                null,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return date.FirstOrDefault();
        }

        public async Task UpdateCostForUpdatedBillRate(DataTable monthlyCalculatedBillRatesDto)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpdateCostForUpdatedBillRate,
                new
                {
                    updatedBillRate =
                        monthlyCalculatedBillRatesDto.AsTableValuedParameter(
                            "[dbo].[analyticsBillRateTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task<IEnumerable<RevOffice>> SaveOfficeListForTableau(DataTable officeDataTable)
        {
            var savedOffices = await _baseRepository.Context.AnalyticsConnection.QueryAsync<RevOffice>(
               StoredProcedureMap.UpsertOfficeListForTableau,
               new
               {
                   offices =
                       officeDataTable.AsTableValuedParameter(
                           "[ccm].[officeTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return savedOffices;
        }
    }
}
