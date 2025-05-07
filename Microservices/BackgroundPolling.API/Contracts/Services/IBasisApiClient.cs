using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IBasisApiClient
    {
        public Task<IList<CurrencyRate>> GetCurrencyRatesByEffectiveDate(DateTime? effectiveFromDate);
        public Task<IEnumerable<PracticeAffiliation>> GetAllPracticeAffiliation();
        Task<IEnumerable<CurrencyViewModel>> GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(string currencyCodes,
              string currencyRateTypeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IList<Holiday>> GetHolidays();
        Task<IEnumerable<PracticeArea>> GetAllPracticeAreas();
    }
}
