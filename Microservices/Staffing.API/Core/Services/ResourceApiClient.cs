using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Net.Http.Headers;

namespace Staffing.API.Core.Services
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
   
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
             SetHttpClientHeaders();
        }

        private void SetHttpClientHeaders()
        {
            var authorizationApiKey1 = ConfigurationUtility.GetValue("Authorization:ApiKey1");
            var authorizationApiKeyValue1 = ConfigurationUtility.GetValue("Authorization:ApiKeyValue1");
            var authorizationApiKey2 = ConfigurationUtility.GetValue("Authorization:ApiKey2");
            var authorizationApiKeyValue2 = ConfigurationUtility.GetValue("Authorization:ApiKeyValue2");

            var authorizationHeader1 = _httpContextAccessor.HttpContext
                .Request.Headers[authorizationApiKey1];
            var authorizationHeader2 = _httpContextAccessor.HttpContext
                .Request.Headers[authorizationApiKey2];

            _apiClient.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Authorization:BearerToken"));

            if (string.IsNullOrEmpty(authorizationHeader1))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey1, authorizationApiKeyValue1);
            }
            if (string.IsNullOrEmpty(authorizationHeader2))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey2, authorizationApiKeyValue2);
            }
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

        public async Task<List<ResourceModel>> GetActiveEmployees()
        {
            var resources = await _cache.GetOrCreateAsync<IList<ResourceModel>>("ActiveEmployees", async cacheEntry =>
            {
                cacheEntry.AbsoluteExpiration = DateTime.Now.AddHours(6);
                cacheEntry.Priority = CacheItemPriority.Normal;
                var responseMessage = await _apiClient.GetAsync("api/Resources/employees");
                var allEmployees = JsonConvert.DeserializeObject<IEnumerable<ResourceModel>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result) ?? new List<ResourceModel>();
                return allEmployees.ToList();
            });

            return resources.ToList();
        }

    }
}