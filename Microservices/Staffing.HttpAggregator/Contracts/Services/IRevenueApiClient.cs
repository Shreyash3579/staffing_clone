using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IRevenueApiClient
    {
        Task<IList<Revenue>> GetRevenueByClientCodeAndCaseCode(int? clientCode, int? caseCode, DateTime startDate, DateTime endDate);
        Task<IList<Revenue>> GetRevenueByServiceLine(string serviceLine, DateTime startDate, DateTime endDate);
    }
}
