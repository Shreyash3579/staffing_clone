using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface IBasisApiClient
    {
        Task<IEnumerable<CurrencyViewModel>> GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(string currencyCodes,
               string currencyRateTypeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate);
    }
}
