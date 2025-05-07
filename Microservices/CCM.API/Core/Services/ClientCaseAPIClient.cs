using CCM.API.Contracts.Services;
using CCM.API.Core.Helpers;
using CCM.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CCM.API.Core.Services
{
    public class ClientCaseAPIClient : IClientCaseAPIClient
    {
        private readonly HttpClient _apiClient;
        public ClientCaseAPIClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:ClientCaseAPIBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:ClientCaseAPI"));
        }

        public async Task<IEnumerable<CaseDataFromClientCaseAPI>> GetModifiedCasesAfterLastPolledTime(DateTime? lastPolledTime)
        {
            if (lastPolledTime == null)
            {
                lastPolledTime = DateTime.Now.Date;
            }

            var responseMessage = await _apiClient.GetAsync($"api/v2/CaseDetails/getAllCasesModifiedAfter?modifiedAfter={lastPolledTime}");
            var cases = JsonConvert.DeserializeObject<IEnumerable<CaseDataFromClientCaseAPI>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<CaseDataFromClientCaseAPI>();

            return cases;
        }

        public async Task<IEnumerable<ClientDataFromClientCaseAPI>> GetClientsForTypeahead(string searchtext)
        {
            var responseMessage = await _apiClient.GetAsync($"api/v11/CaseDetails/clientsbysearchstring?text={searchtext}");
            var clients = JsonConvert.DeserializeObject<IEnumerable<ClientDataFromClientCaseAPI>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ClientDataFromClientCaseAPI>();

            return clients;
        }
    }
}
