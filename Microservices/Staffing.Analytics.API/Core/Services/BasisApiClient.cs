using Newtonsoft.Json;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Services
{
    public class BasisApiClient : IBasisApiClient
    {
        private readonly HttpClient _apiClient;
        public BasisApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("BasisApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(20);
        }
        public async Task<IEnumerable<CurrencyViewModel>> GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(string currencyCodes,
            string currencyRateTypeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var responseMessage = await _apiClient.GetAsync($"api/currency/GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate?currencyCodes={currencyCodes}&currencyRateTypeCodes={currencyRateTypeCodes}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");
            var currencyRates = JsonConvert.DeserializeObject<IEnumerable<CurrencyViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<CurrencyViewModel>();

            return currencyRates.ToList();
        }
    }
}
