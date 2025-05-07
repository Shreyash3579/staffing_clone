using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class BasisApiClient : IBasisApiClient
    {
        private readonly HttpClient _apiClient;

        public BasisApiClient(HttpClient apiClient)
        {
            _apiClient = apiClient;
            var endpointAddress = ConfigurationUtility.GetValue("BasisApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<IEnumerable<EmployeePracticeArea>> GetPracticeAreaAffiliationsByEmployeeCodes(string employeeCodes, string practiceAreaCodes, string affiliationRoleCodes)
        {
            var payload = new { listEmployeeCodes = employeeCodes, practiceAreaCodes, affiliationRoleCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/practiceArea/getAffiliationsByEmployeeCodesAndPracticeAreaCodes", payload);
            var employeePracticeAreas = JsonConvert.DeserializeObject<IEnumerable<EmployeePracticeArea>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<EmployeePracticeArea>();
            return employeePracticeAreas;
        }

        public async Task<IEnumerable<EmployeePracticeAreaViewModel>> GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Enumerable.Empty<EmployeePracticeAreaViewModel>();
            }

            var payload = new { employeeCode};

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/practiceArea/getAffiliationsByEmployeeCodesAndPracticeAreaCodes", payload);

            var employeeAffiliations =
                JsonConvert.DeserializeObject<IEnumerable<EmployeePracticeAreaViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<EmployeePracticeAreaViewModel>();

            return employeeAffiliations.ToList();
        }

        public async Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<HolidayViewModel>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/holiday/officeholidaysWithinDateRangeByEmployees", payload);

            var resourcesofficeHolidays =
                JsonConvert.DeserializeObject<IEnumerable<HolidayViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<HolidayViewModel>();

            return resourcesofficeHolidays.ToList();

        }

        public async Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Enumerable.Empty<HolidayViewModel>();
            }

            var responseMessage =
               await _apiClient.GetAsync($"api/holiday?employeeCode={employeeCode}&effectiveFromDate={effectiveFromDate}&effectiveToDate={effectiveToDate}");

            var resourcesofficeHolidays =
                JsonConvert.DeserializeObject<IEnumerable<HolidayViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<HolidayViewModel>();

            return resourcesofficeHolidays.ToList();

        }
    }
}
