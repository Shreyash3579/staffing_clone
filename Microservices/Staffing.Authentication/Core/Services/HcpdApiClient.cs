using Newtonsoft.Json;
using Staffing.Authentication.Contracts.Services;
using Staffing.Authentication.Core.Helpers;
using Staffing.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.Authentication.Core.Services
{
    public class HcpdApiClient: IHcpdApiClient
    {
        private readonly HttpClient _apiClient;

        public HcpdApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:HcpdApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<IEnumerable<HcpdSecurityUser>> GetSecurityUserDetails(string employeeCode)
        {


            var responseMessage = await _apiClient.GetAsync($"api/securityUser/securityUserDetails?employeeCode={employeeCode}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                // Log the error message or handle it as needed
                Console.WriteLine($"Error: {responseMessage.StatusCode} - {await responseMessage.Content.ReadAsStringAsync()}");
                return Enumerable.Empty<HcpdSecurityUser>();
            }

            var content = await responseMessage.Content.ReadAsStringAsync();
            var securityUserDetails = JsonConvert.DeserializeObject<IEnumerable<HcpdSecurityUser>>(content) ?? Enumerable.Empty<HcpdSecurityUser>();

            return securityUserDetails;
        }
    }
}
