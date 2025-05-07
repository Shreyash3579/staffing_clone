using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Staffing.Authentication.Contracts.Services;
using Staffing.Authentication.Core.Helpers;
using Staffing.Authentication.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Staffing.Authentication.Core.Services
{
    public class ResourcesApiClient : IResourcesApiClient
    {
        private readonly HttpClient _apiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResourcesApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:ResourcesApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
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

            _apiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:ResourcesApi"));

            var authorizationHeader1 = _httpContextAccessor.HttpContext
                .Request.Headers[authorizationApiKey1];
            var authorizationHeader2 = _httpContextAccessor.HttpContext
                .Request.Headers[authorizationApiKey2];

            if (string.IsNullOrEmpty(authorizationHeader1))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey1, authorizationApiKeyValue1);
            }
            if (string.IsNullOrEmpty(authorizationHeader2))
            {
                _apiClient.DefaultRequestHeaders.Add(authorizationApiKey2, authorizationApiKeyValue2);
            }

        }

        public async Task<Employee> GetEmployee(string employeeCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/resources/employee?employeeCode={employeeCode}");
            var employee =
                JsonConvert.DeserializeObject<Employee>(responseMessage.Content?.ReadAsStringAsync().Result) ??
                new Employee();

            return employee;
        }
    }
}