using CCM.API.Contracts.Services;
using CCM.API.Core.Helpers;
using CCM.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CCM.API.Core.Services
{
    public class FinApiClient : IFinApiClient
    {
        private readonly HttpClient _apiClient;
        public FinApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:FinApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:FinApi"));
        }

        public async Task<IEnumerable<RevOffice>> GetOffices(string hierarchyType, string status)
        {
            var responseMessage = await _apiClient.GetAsync($"api/v1/Office/allofficeshierarchy?hierarchyType={hierarchyType}&status={status}");
            var offices = JsonConvert.DeserializeObject<IEnumerable<RevOffice>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<RevOffice>();

            return offices;
        }

        public async Task<IEnumerable<BillRate>> GetBillRateByOffices(string officeCodes)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/BillingRate/billingRatesWithHistorical?officeCodes={officeCodes}");
            var billRates =
                JsonConvert.DeserializeObject<IEnumerable<BillRate>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<BillRate>();

            return billRates;
        }

        public async Task<IEnumerable<BillRate>> GetBillRates()
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/BillingRate/billingRatesWithHistorical?isWorldWide={true}");
            var billRates =
                JsonConvert.DeserializeObject<IEnumerable<BillRate>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<BillRate>();

            return billRates;
        }

        public async Task<IEnumerable<BillRateType>> GetBillRateType()
        {
            var responseMessage =
                await _apiClient.GetAsync("api/BillingRate/type");
            var billRateTypes =
                JsonConvert.DeserializeObject<IEnumerable<BillRateType>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<BillRateType>();

            return billRateTypes;
        }
    }
}
