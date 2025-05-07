using Newtonsoft.Json;
using Staffing.Authentication.Contracts.Services;
using Staffing.Authentication.Core.Helpers;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Staffing.Authentication.Core.Services
{
    public class PegC2CApiClient : IPegC2CApiClient
    {
        private readonly HttpClient _apiClient;

        public PegC2CApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:PegC2CApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:PegC2CApi"));
        }

        public async Task<bool> GetSecurityUserAccess(string employeeCode)
        {
            bool isUserAuthorizedToAccessPEGOpportunityDetails = false;
            try
            {
                var responseMessage = await _apiClient.GetAsync($"api/staffing/validateStaffinguser?employeeCode={employeeCode}");
                isUserAuthorizedToAccessPEGOpportunityDetails =
                    JsonConvert.DeserializeObject<bool>(responseMessage.Content?.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                //TODO: log exceptions here
            }


            return isUserAuthorizedToAccessPEGOpportunityDetails;
        }

    }
}
