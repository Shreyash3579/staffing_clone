using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IFinanceDataPollingRepository
    {
        Task UpsertBillRates(DataTable billRates);
        Task UpsertRevenueTransactions(DataTable revenueTransactions);
        Task DeleteRevenueTransactionsById(string ids);
        Task<IEnumerable<Office>> GetOfficeList();
        Task<DateTime> GetLastUpdatedBillRateDate();
        Task<DateTime> GetLastUpdatedRevenueTransactionDate();
        Task UpdateCostForUpdatedBillRate(DataTable monthlyCalculatedBillRatesDto);
        Task<IEnumerable<RevOffice>> SaveOfficeListForTableau(DataTable officeListDataTable);
    }
}
