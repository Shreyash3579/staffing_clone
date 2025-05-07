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
    public class VacationApiClient : IVacationApiClient
    {
        private readonly HttpClient _apiClient;

        public VacationApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("VacationApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }
        public async Task<IEnumerable<VacationRequestViewModel>> GetVacationsWithinDateRangeByEmployeeCodes(string employeeCodes,
            DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<VacationRequestViewModel>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/vacationrequest/vacationsWithinDateRangeByEmployees", payload);

            var resourcesVacations =
                JsonConvert.DeserializeObject<IEnumerable<VacationRequestViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<VacationRequestViewModel>();

            return resourcesVacations.ToList();

        }

        public async Task<IEnumerable<VacationRequestViewModel>> GetVacationRequestsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Enumerable.Empty<VacationRequestViewModel>();
            }

            var responseMessage =
               await _apiClient.GetAsync($"api/vacationrequest?employeeCode={employeeCode}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");

            var resourcesVacations =
                JsonConvert.DeserializeObject<IEnumerable<VacationRequestViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<VacationRequestViewModel>();

            return resourcesVacations.ToList();

        }
    }
}
