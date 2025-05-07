using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.Models.Workday;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class ResourceApiClient : IResourceApiClient
    {
        private readonly HttpClient _apiClient;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResourceApiClient(HttpClient apiClient, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:ResourcesApiBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(10);
            _cache = cache;
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

            _apiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:ResourcesApi"));

            if (string.IsNullOrEmpty(authorizationHeader1))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey1, authorizationApiKeyValue1);
            }
            if (string.IsNullOrEmpty(authorizationHeader2))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey2, authorizationApiKeyValue2);
            }
        }

        #region Employee
        public async Task<List<Resource>> GetEmployees()
        {
            var responseMessage = await _apiClient.GetAsync("api/Resources/employees");
            var resources = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<Resource>();

            return resources.Where(o => o.ActiveStatus == "Active" || o.ActiveStatus == "On Leave").ToList();
        }

        public async Task<List<Resource>> GetNotYetStartedEmployees()
        {
            var responseMessage = await _apiClient.GetAsync("api/Resources/employees");
            var resources = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<Resource>();

            return resources.Where(o => o.ActiveStatus == "Not Yet Started").ToList();
        }

        public async Task<List<Resource>> GetEmployeesIncludingTerminated()
        {
            var resources = await _cache.GetOrCreateAsync<IList<Resource>>("AllEmployees", async cacheEntry =>
            {
                cacheEntry.AbsoluteExpiration = DateTime.Now.AddHours(6);
                cacheEntry.Priority = CacheItemPriority.Normal;
                var responseMessage = await _apiClient.GetAsync("api/Resources/employeesIncludingTerminated");
                var allEmployees = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result) ?? new List<Resource>();
                return allEmployees.ToList();
            });

            return resources.ToList();
        }
        public async Task<List<Resource>> GetTerminatedEmployees()
        {
            var resources = await _cache.GetOrCreateAsync<IList<Resource>>("TerminatedEmployees", async cacheEntry =>
            {
                cacheEntry.AbsoluteExpiration = DateTime.Now.AddHours(6);
                cacheEntry.Priority = CacheItemPriority.Normal;
                var responseMessage = await _apiClient.GetAsync("api/Resources/terminatedEmployees");
                var allEmployees = JsonConvert.DeserializeObject<IEnumerable<Resource>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result) ?? new List<Resource>();
                return allEmployees.ToList();
            });

            return resources.ToList();
        }


        public async Task<Dictionary<string, string>> GetEmployeeIdTypeMaps()
        {
            var responseMessage = await _apiClient.GetAsync("api/resources/employeeIdTypeMap");
            var employeeIdTypeMaps = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseMessage.Content?.ReadAsStringAsync().Result);

            return employeeIdTypeMaps;
        }
        #endregion

        #region Employee Staffing Transaction
        public async Task<IEnumerable<ResourceTransaction>> GetFutureTransitions()
        {
            var responseMessage = await _apiClient.GetAsync("api/ResourcesTransactions/futureTransitions");
            var transitions = JsonConvert.DeserializeObject<IEnumerable<ResourceTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<ResourceTransaction>();

            return transitions.ToList();
        }
        public async Task<IEnumerable<ResourceTransaction>> GetFuturePromotions()
        {
            var responseMessage = await _apiClient.GetAsync("api/ResourcesTransactions/futurePromotions");
            var promotions = JsonConvert.DeserializeObject<IEnumerable<ResourceTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<ResourceTransaction>();

            return promotions.ToList();
        }
        public async Task<IEnumerable<ResourceTransaction>> GetFutureTransfers()
        {
            var responseMessage = await _apiClient.GetAsync("api/ResourcesTransactions/futureTransfers");
            var transfers = JsonConvert.DeserializeObject<IEnumerable<ResourceTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<ResourceTransaction>();

            return transfers.ToList();
        }
        public async Task<IEnumerable<EmployeeTransaction>> GetEmployeesStaffingTransactions(string listEmployeeCodes)
        {
            var payload = new { listEmployeeCodes };
            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/resourcestransactions/employeesStaffingTransactions", payload);

            var staffingTransactions = JsonConvert.DeserializeObject<IEnumerable<EmployeeTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<EmployeeTransaction>();

            return staffingTransactions;
        }
        public async Task<IEnumerable<ResourceTransaction>> GetFutureTerminations()
        {
            var responseMessage = await _apiClient.GetAsync("api/ResourcesTransactions/futureTerminations");
            var terminations = JsonConvert.DeserializeObject<IEnumerable<ResourceTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<ResourceTransaction>();

            return terminations.ToList();
        }
        #endregion

        #region Employee LOA
        public async Task<IEnumerable<ResourceLOA>> GetFutureLOAs()
        {
            var responseMessage = await _apiClient.GetAsync("api/resourcesloa/futureLOAs");
            var LOAs = JsonConvert.DeserializeObject<IEnumerable<ResourceLOA>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<ResourceLOA>();

            return LOAs.ToList();
        }
        public async Task<IEnumerable<ResourceLOA>> GetPendingLOATransactions()
        {
            var responseMessage = await _apiClient.GetAsync("api/resourcesLOA/pendingLOATransactions");
            var LOAs = JsonConvert.DeserializeObject<IEnumerable<ResourceLOA>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<ResourceLOA>();

            return LOAs.ToList();
        }
        public async Task<IEnumerable<ResourceLOA>> GetLOAsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime startDate, DateTime endDate)
        {
            var payload = new { employeeCodes, startDate, endDate };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/resourcesloa/loaByEmployees", payload);
            var resourceLoAs =
                JsonConvert.DeserializeObject<IEnumerable<ResourceLOA>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<ResourceLOA>();

            return resourceLoAs;
        }
        public async Task<IEnumerable<ResourceLOA>> GetEmployeesLoATransactions(string listEmployeeCodes)
        {
            var payload = new { listEmployeeCodes };
            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/resourcesloa/employeesAllLoAs", payload);

            var loas = JsonConvert.DeserializeObject<IEnumerable<ResourceLOA>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<ResourceLOA>();

            return loas;
        }

        public async Task<IEnumerable<EmployeeLoATransaction>> GetWDEmployeesLoATransactions(string listEmployeeCodes)
        {
            var payload = new { listEmployeeCodes };
            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/resourcesloa/employeesLoATransactions", payload);

            var LoATransactions = JsonConvert.DeserializeObject<IEnumerable<EmployeeLoATransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<EmployeeLoATransaction>();

            return LoATransactions;
        }
        #endregion

        #region Lookup

        public async Task<IEnumerable<ServiceLine>> GetServiceLines()
        {
            var responseMessage = await _apiClient.GetAsync("api/lookup/serviceLineList");
            var serviceLines = JsonConvert.DeserializeObject<IEnumerable<ServiceLine>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<ServiceLine>();

            return serviceLines.ToList();
        }
        public async Task<IEnumerable<PDGrade>> GetPDGrades()
        {
            var responseMessage = await _apiClient.GetAsync("api/lookup/pdGrades");
            var pdGrades = JsonConvert.DeserializeObject<IEnumerable<PDGrade>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<PDGrade>();

            return pdGrades.ToList();
        }
        #endregion

        #region TimeOff
        public async Task<IEnumerable<ResourceTimeOff>> GetEmployeesTimeoffs(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            var payload = new { employeeCodes, startDate, endDate };
            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/resourcesTimeoff/timeOffsByEmployees", payload);

            var employeesTimeOffs = JsonConvert.DeserializeObject<IEnumerable<ResourceTimeOff>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<ResourceTimeOff>();

            return employeesTimeOffs;
        }
        #endregion

        #region Employees Certifications
        public async Task<IEnumerable<EmployeeCertificates>> GetCertificatesByEmployeeCodes(string listEmployeeCodes)
        {
            if (string.IsNullOrEmpty(listEmployeeCodes))
                return Enumerable.Empty<EmployeeCertificates>();

            var payload = new { listEmployeeCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/resourcesCertification/certificationsByEmployees", payload);
            var employeesCertificates = JsonConvert.DeserializeObject<IEnumerable<EmployeeCertificates>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<EmployeeCertificates>();

            return employeesCertificates;
        }
        #endregion

        #region Employees Languages
        public async Task<IEnumerable<EmployeeLanguages>> GetLanguagesByEmployeeCodes(string listEmployeeCodes)
        {
            if (string.IsNullOrEmpty(listEmployeeCodes))
                return Enumerable.Empty<EmployeeLanguages>();

            var payload = new { listEmployeeCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/resourcesLanguage/employeeLanguagesByEmployeeCodes", payload);
            var employeesLanguages = JsonConvert.DeserializeObject<IEnumerable<EmployeeLanguages>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<EmployeeLanguages>();

            return employeesLanguages;
        }
        #endregion

    }
}
