using Basis.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Contracts.RepositoryInterfaces
{
    public interface ICurrencyRepository
    {
        Task<IEnumerable<CurrencyRates>> GetCurrencyRatesByEffectiveDate(DateTime? effectiveFromDate);
        Task<IEnumerable<CurrencyRates>> GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(string currencyCodes, 
            string currencyRateTypeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate);
    }
}
