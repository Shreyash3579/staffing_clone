using Iris.API.Contracts.Services;
using Iris.API.Core.Helpers;
using Iris.API.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Iris.API.Core.Services
{
    public class IrisApiClient : IirisApiClient
    {
        private readonly HttpClient _apiClient;
        private readonly string columnsToSelect = "education,workhistory";
        public IrisApiClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:IrisApiBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }
        public async Task<EmployeeWorkAndSchoolHistory> GetEmployeeWorkAndSchoolHistory(string employeeCode)
        {
            var responseMessage = await _apiClient.GetAsync($"users/{employeeCode}/qualifications");

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"Cannot collect Employee work and school history. IRIS API responded with status code: {responseMessage.StatusCode}.");
            }

            var employeeWorkAndSchoolHistory = JsonConvert.DeserializeObject<EmployeeWorkAndSchoolHistory>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? new EmployeeWorkAndSchoolHistory();
            return employeeWorkAndSchoolHistory;
        }

        public async Task<EmployeesQualifications> GetWorkSchoolHistoryForAll(int pageNumber, int pageCount, bool includeAlumni)
        {
            var responseMessage = await _apiClient.GetAsync($"integration/qualifications?pageNumber={pageNumber}&pageCount={pageCount}&includeAlumni={includeAlumni}&select={columnsToSelect}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"Cannot collect Employee work and school history from page: {pageNumber} with page count: {pageCount}. IRIS API responded with status code: {responseMessage.StatusCode}.");
            }

            var employeeWorkAndSchoolHistory = JsonConvert.DeserializeObject<EmployeesQualifications>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? new EmployeesQualifications();
            return employeeWorkAndSchoolHistory;
        }

        public async Task<EmployeesQualifications> GetWorkSchoolHistoryByModifiedDate(DateTime lastModifiedAfter, bool includeAlumni)
        {
            var responseMessage = await _apiClient.GetAsync($"integration/qualifications?modifiedDate={lastModifiedAfter}&includeAlumni={includeAlumni}&select={columnsToSelect}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"Cannot collect Employee work and school history modified after {lastModifiedAfter}. IRIS API responded with status code: {responseMessage.StatusCode}.");
            }

            var employeeWorkAndSchoolHistory = JsonConvert.DeserializeObject<EmployeesQualifications>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? new EmployeesQualifications();
            return employeeWorkAndSchoolHistory;
        }
    }
}
