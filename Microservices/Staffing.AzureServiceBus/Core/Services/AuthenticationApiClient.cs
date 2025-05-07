using Staffing.AzureServiceBus.Contracts.Services;
using Staffing.AzureServiceBus.Core.Helpers;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Staffing.AzureServiceBus.Core.Services
{
    public class AuthenticationApiClient : IAuthenticationApiClient
    {
        private readonly HttpClient _apiClient;

        public AuthenticationApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AuthenticationApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }
        public async Task<string> GetToken(string appName, string appSecret)
        {
            var appSecretHtmlEncoded = HttpUtility.UrlEncode(appSecret);
            var responseMessage = await _apiClient.GetAsync($"api/securityUser/authenticate?appName={appName}&appSecret={appSecretHtmlEncoded}");
            var token = responseMessage.Content?.ReadAsStringAsync().Result;
            return token;
        }
    }
}
