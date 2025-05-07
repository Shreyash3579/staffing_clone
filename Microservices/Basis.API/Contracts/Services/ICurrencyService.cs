using Basis.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Contracts.Services
{
   public interface ICurrencyService
    {
        Task<IEnumerable<CurrencyViewModel>> GetCurrencyRatesByEffectiveDate(DateTime? effectiveFromDate);
        Task<IEnumerable<CurrencyViewModel>> GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(string currencyCodes,
            string currencyRateTypeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate);
    }
}
