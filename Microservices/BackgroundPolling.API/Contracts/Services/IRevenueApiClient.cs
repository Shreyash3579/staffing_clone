using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IRevenueApiClient
    {
        Task<IEnumerable<RevenueTransaction>> GetRevenueTransactions(DateTime editedDateAfterDate);
        Task<IEnumerable<RevenueTransaction>> GetDeletedRevenueTransactions(DateTime deletedAfterDate);
    }
}
