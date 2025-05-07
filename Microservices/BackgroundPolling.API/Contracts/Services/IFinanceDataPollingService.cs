using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IFinanceDataPollingService
    {
        Task UpsertBillRates();
        Task<IEnumerable<RevOffice>> SaveOfficeListForTableau();
        Task UpsertRevenueTransactions();
        Task DeleteRevenueTransactionsById(DateTime? lastUpdated);
    }
}
