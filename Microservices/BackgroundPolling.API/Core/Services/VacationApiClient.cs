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
    public class VacationApiClient : IVacationApiClient
    {
        private readonly HttpClient _apiClient;

        public VacationApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:VacationApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }
        public async Task<IList<Vacation>> GetVacationsSinceLastPolled(DateTime? lastPolledDateTime)
        {
            var responseMessage = await _apiClient.GetAsync($"api/vacationRequest/vacations?lastPolledDateTime={lastPolledDateTime}");
            var vacations = JsonConvert.DeserializeObject<IEnumerable<Vacation>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<Vacation>();
            return vacations.ToList();
        }
    }
}
