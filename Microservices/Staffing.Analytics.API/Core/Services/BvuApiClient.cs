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
    public class BvuApiClient : IBvuApiClient
    {
        private readonly HttpClient _apiClient;
        public BvuApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("BvuApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(20);
        }
        public async Task<IEnumerable<TrainingViewModel>> GetTrainingsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<TrainingViewModel>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/training/trainingsWithinDateRangeByEmployees", payload);

            var resourcesTrainings =
                JsonConvert.DeserializeObject<IEnumerable<TrainingViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<TrainingViewModel>();

            return resourcesTrainings.ToList();

        }
    }
}
