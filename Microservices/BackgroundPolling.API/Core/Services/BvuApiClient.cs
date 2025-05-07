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
    public class BvuApiClient: IBvuApiClient
    {
        private readonly HttpClient _apiClient;

        public BvuApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:BvuApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }
        public async Task<IList<Training>> GetTrainings(DateTime? lastPolledDateTime)
        {
            var responseMessage = await _apiClient.GetAsync($"api/training/trainings?lastPolledDateTime={lastPolledDateTime}");
            var trainings = JsonConvert.DeserializeObject<IEnumerable<Training>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<Training>();
            return trainings.ToList();
        }
    }
}
