using CaseIntake.API.Contracts.Services;
using CaseIntake.API.Core.Helpers;
using CaseIntake.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CaseIntake.API.Core.Services
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
        public async Task<List<ResourceModel>> GetActiveEmployees()
        {
            var responseMessage = await _apiClient.GetAsync("api/Resources/employees");
            var resources = JsonConvert.DeserializeObject<IEnumerable<ResourceModel>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<ResourceModel>();

            return resources.Where(o => o.ActiveStatus == "Active" || o.ActiveStatus == "On Leave").ToList();
        }

        
        public async Task<List<ResourceModel>> GetEmployeesIncludingTerminated()
        {
            var resources = await _cache.GetOrCreateAsync<IList<ResourceModel>>("AllEmployees", async cacheEntry =>
            {
                cacheEntry.AbsoluteExpiration = DateTime.Now.AddHours(6);
                cacheEntry.Priority = CacheItemPriority.Normal;
                var responseMessage = await _apiClient.GetAsync("api/Resources/employeesIncludingTerminated");
                var allEmployees = JsonConvert.DeserializeObject<IEnumerable<ResourceModel>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result) ?? new List<ResourceModel>();
                return allEmployees.ToList();
            });

            return resources.ToList();
        }

        public async Task<ResourceModel> GetEmployeeByEmployeeCode(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return new ResourceModel();
            }

            var responseMessage =
               await _apiClient.GetAsync($"api/resources/employee?employeeCode={employeeCode}");

            var resource = JsonConvert.DeserializeObject<ResourceModel>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new ResourceModel();

            return resource;
        }

        #endregion


    }
}
