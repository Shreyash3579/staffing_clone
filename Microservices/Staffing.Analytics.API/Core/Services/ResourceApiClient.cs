using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using Staffing.Analytics.API.Models.Workday;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Services
{
    public class ResourceApiClient : IResourceApiClient
    {
        private readonly HttpClient _apiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResourceApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            var endpointAddress = ConfigurationUtility.GetValue("ResourceApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:ResourcesApi"));
            _apiClient.Timeout = TimeSpan.FromMinutes(20);
            _httpContextAccessor = httpContextAccessor;
            if (endpointAddress.Contains("aip"))
            {
                SetHttpClientHeaders();
            }
        }

        private void SetHttpClientHeaders()
        {
            var authorizationApiKey1 = ConfigurationUtility.GetValue("Authorization:ApiKey1");
            var authorizationApiKeyValue1 = ConfigurationUtility.GetValue("Authorization:ApiKeyValue1");
            var authorizationApiKey2 = ConfigurationUtility.GetValue("Authorization:ApiKey2");
            var authorizationApiKeyValue2 = ConfigurationUtility.GetValue("Authorization:ApiKeyValue2");

            var authorizationHeader1 = _httpContextAccessor.HttpContext
                ?.Request.Headers[authorizationApiKey1];
            var authorizationHeader2 = _httpContextAccessor.HttpContext
                ?.Request.Headers[authorizationApiKey2];

            if (string.IsNullOrEmpty(authorizationHeader1))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey1, authorizationApiKeyValue1);
            }
            if (string.IsNullOrEmpty(authorizationHeader2))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey2, authorizationApiKeyValue2);
            }
        }

        public async Task<IEnumerable<ResourceTransaction>> GetEmployeePendingPromotions(string employeeCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/Resources/promotionsByEmployee?employeeCode={employeeCode}");
            var resourceTransactions = JsonConvert.DeserializeObject<IEnumerable<ResourceTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<ResourceTransaction>();

            return resourceTransactions;
        }

        public async Task<IEnumerable<ResourceTransaction>> GetEmployeePendingTransfers(string employeeCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/Resources/transfersByEmployee?employeeCode={employeeCode}");
            var resourceTransactions = JsonConvert.DeserializeObject<IEnumerable<ResourceTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<ResourceTransaction>();

            return resourceTransactions;
        }

        public async Task<IEnumerable<ResourceTransaction>> GetFutureLOAsByEmployeeCode(string employeeCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/resourcesloa/LOAByEmployee?employeeCode={employeeCode}");
            var resourceTransactions = JsonConvert.DeserializeObject<IEnumerable<ResourceTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<ResourceTransaction>();

            return resourceTransactions;
        }

        public async Task<IEnumerable<ResourceLoA>> GetLOAsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime startDate, DateTime endDate)
        {
            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/resourcesloa/LOAByEmployees", payload);
            var resourceLoAs =
                JsonConvert.DeserializeObject<IEnumerable<ResourceLoA>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceLoA>();

            return resourceLoAs;
        }

        public async Task<IEnumerable<ResourceTransaction>> GetFutureTransitionByEmployeeCode(string employeeCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/ResourcesTransactions/transitionByEmployee?employeeCode={employeeCode}");
            var resourceTransactions = JsonConvert.DeserializeObject<ResourceTransaction>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new ResourceTransaction();

            return Enumerable.Repeat(resourceTransactions, 1);
        }

        public async Task<IEnumerable<ResourceTransaction>> GetPendingTransactionsByEmployeeCodes(string employeeCodes)
        {
            var responseMessage = await _apiClient.GetAsync($"api/ResourcesTransactions/employeesPendingTransactions?employeeCodes={employeeCodes}");
            var resourceTransactions = JsonConvert.DeserializeObject<IEnumerable<ResourceTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<ResourceTransaction>();

            return resourceTransactions;
        }

        public async Task<IEnumerable<Resource>> GetEmployees()
        {
            var responseMessage = await _apiClient.GetAsync("api/Resources/employees");
            var resources = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<Resource>();

            return resources.ToList();
        }

        public async Task<IEnumerable<Resource>> GetEmployeesIncludingTerminated()
        {
            var responseMessage = await _apiClient.GetAsync("api/Resources/employeesIncludingTerminated");
            var resources = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<Resource>();

            return resources.ToList();
        }

        public async Task<IEnumerable<Models.Workday.EmployeeTransaction>> GetEmployeesStaffingTransactions(string listEmployeeCodes)
        {
            var payload = new { listEmployeeCodes };
            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/resourcestransactions/employeesStaffingTransactions", payload);

            var staffingTransactions = JsonConvert.DeserializeObject<IEnumerable<Models.Workday.EmployeeTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<Models.Workday.EmployeeTransaction>();

            return staffingTransactions;
        }

        public async Task<IEnumerable<ResourceLoA>> GetEmployeesLoATransactions(string listEmployeeCodes)
        {
            var payload = new { listEmployeeCodes };
            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/resourcesloa/employeesAllLoAs", payload);

            var loas = JsonConvert.DeserializeObject<IEnumerable<ResourceLoA>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<ResourceLoA>();

            return loas;
        }

        public async Task<IEnumerable<ServiceLine>> GetServiceLineList()
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/lookup/serviceLineList");

            var serviceLines = JsonConvert.DeserializeObject<IEnumerable<ServiceLine>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<ServiceLine>();

            return serviceLines;
        }

        public async Task<IEnumerable<ResourceTimeOff>> GetEmployeesTimeoffs(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<ResourceTimeOff>();

            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/resourcesTimeOff/timeOffsByEmployees", payload);

            var resourcesTimeOffs =
                JsonConvert.DeserializeObject<IEnumerable<ResourceTimeOff>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceTimeOff>();

            return resourcesTimeOffs.ToList();
        }

        public async Task<IEnumerable<JobProfile>> GetJobProfileList()
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/lookup/jobProfiles");

            var jobProfiles = JsonConvert.DeserializeObject<IEnumerable<JobProfile>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? new List<JobProfile>();

            return jobProfiles;
        }
    }
}
