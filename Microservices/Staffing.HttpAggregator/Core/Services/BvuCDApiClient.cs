using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class BvuCDApiClient : IBvuCDApiClient
    {
        private readonly HttpClient _apiClient;

        public BvuCDApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("BvuCDApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
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

        public async Task<IEnumerable<TrainingViewModel>> GetTrainingsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Enumerable.Empty<TrainingViewModel>();
            }

            var responseMessage =
               await _apiClient.GetAsync($"api/training?employeeCode={employeeCode}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");

            var resourcesTrainings =
                JsonConvert.DeserializeObject<IEnumerable<TrainingViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<TrainingViewModel>();

            return resourcesTrainings.ToList();

        }
    }
}
