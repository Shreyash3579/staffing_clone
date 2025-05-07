using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class RevenueApiClient : IRevenueApiClient
    {
        private readonly HttpClient _apiClient;
        public RevenueApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:RevenueApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:RevenueApi"));
        }

        public async Task<IEnumerable<RevenueTransaction>> GetDeletedRevenueTransactions(DateTime deletedAfterDate)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/v1/Revenue/deletedrevenuetransactions?deletedAfterDate={deletedAfterDate}");
            var deletedRevenueTransactions =
                JsonConvert.DeserializeObject<IEnumerable<RevenueTransaction>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<RevenueTransaction>();

            return deletedRevenueTransactions;
        }
        public async Task<IEnumerable<RevenueTransaction>> GetRevenueTransactions(DateTime editedDateAfterDate) 
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/v2/Revenue/revenuetransactions?editedAddedAfterDate={editedDateAfterDate}");
            var revenueTransactions =
                JsonConvert.DeserializeObject<IEnumerable<RevenueTransaction>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<RevenueTransaction>();

            return revenueTransactions;
        }
    }
}
