using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class BasisApiClient :IBasisApiClient
    {
        private readonly HttpClient _apiClient;
        public BasisApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:BasisApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task<IList<CurrencyRate>> GetCurrencyRatesByEffectiveDate(DateTime? effectiveFromDate)
        {
            var responseMessage = await _apiClient.GetAsync($"api/Currency/getcurrencyratesbyeffectivefromdate?effectiveFromDate={effectiveFromDate}");
            var currencyrates = JsonConvert.DeserializeObject<IEnumerable<CurrencyRate>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CurrencyRate>();
            return currencyrates.ToList();
        }

        public async Task<IEnumerable<PracticeAffiliation>> GetAllPracticeAffiliation()
        {
            var responseMessage = await _apiClient.GetAsync($"api/PracticeAffiliation/getAllPracticeAffiliation");
            var affiliations = JsonConvert.DeserializeObject<IEnumerable<PracticeAffiliation>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<PracticeAffiliation>();
            return affiliations.ToList();
        }
        public async Task<IEnumerable<CurrencyViewModel>> GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(string currencyCodes,
            string currencyRateTypeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var responseMessage = await _apiClient.GetAsync($"api/currency/GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate?currencyCodes={currencyCodes}&currencyRateTypeCodes={currencyRateTypeCodes}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");
            var currencyRates = JsonConvert.DeserializeObject<IEnumerable<CurrencyViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<CurrencyViewModel>();

            return currencyRates.ToList();
        }

        public async Task<IList<Holiday>> GetHolidays()
        {
            var responseMessage = await _apiClient.GetAsync($"api/holiday/holidays");
            var holidays = JsonConvert.DeserializeObject<IEnumerable<Holiday>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<Holiday>();
            return holidays.ToList();
        }

        public async Task<IEnumerable<PracticeArea>> GetAllPracticeAreas()
        {
            var responseMessage = await _apiClient.GetAsync($"api/practiceArea/getAllPracticeArea");
            var practiceAreas = JsonConvert.DeserializeObject<IEnumerable<PracticeArea>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<PracticeArea>();
            return practiceAreas.ToList();
        }
    }
}
