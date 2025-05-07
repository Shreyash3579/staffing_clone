using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class PolarisApiClient : IPolarisApiClient
    {
        private readonly HttpClient _polarisApiClient;

        public PolarisApiClient(HttpClient httpClient)
        {
            var polarisEndpointAddress = ConfigurationUtility.GetValue("AppSettings:PolarisApiBaseUrl");

            _polarisApiClient = httpClient;
            _polarisApiClient.BaseAddress = new Uri(polarisEndpointAddress);
        }

        public async Task<IList<PolarisSecurityUser>> GetRevSecurityUsersWithGeography()
        {
            var responseMessage = await _polarisApiClient.GetAsync($"api/securityuser/getrevuserpersona");
            var securityUsers = JsonConvert.DeserializeObject<IEnumerable<PolarisSecurityUser>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<PolarisSecurityUser>();
            return securityUsers.ToList();
        }
    }
}
