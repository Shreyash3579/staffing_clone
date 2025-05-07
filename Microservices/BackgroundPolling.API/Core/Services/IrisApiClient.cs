using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class IrisApiClient : IIrisApiClient
    {
        private readonly HttpClient _apiClient;
        public IrisApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:IrisApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task<EmployeesQualifications> GetWorkSchoolHistoryForAll(int pageNumber, int pageCount, bool includeAlumni)
        {
            var responseMessage = await _apiClient.GetAsync($"api/EmployeeWorkAndSchoolHistory/workSchoolHistoryForAll?pageNumber={pageNumber}&pageCount={pageCount}&includeAlumni={includeAlumni}");
            var paginatedQualificationsForEmployees = JsonConvert.DeserializeObject<EmployeesQualifications>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new EmployeesQualifications();
            return paginatedQualificationsForEmployees;

        }
        public async Task<EmployeesQualifications> GetWorkSchoolHistoryByModifiedDate(DateTime lastModifiedAfter, bool includeAlumni)
        {
            var responseMessage = await _apiClient.GetAsync($"api/EmployeeWorkAndSchoolHistory/workSchoolHistoryByModifiedDate?lastModifiedAfter={lastModifiedAfter}&includeAlumni={includeAlumni}");
            var qualificationsForEmployeesAfterLAstModifiedDate = JsonConvert.DeserializeObject<EmployeesQualifications>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new EmployeesQualifications();
            return qualificationsForEmployeesAfterLAstModifiedDate;
        }
    }
}
