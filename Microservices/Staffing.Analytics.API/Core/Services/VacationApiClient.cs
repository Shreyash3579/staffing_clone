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
    }
}
