using Newtonsoft.Json;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Services
{
    public class CCMApiClient : ICCMApiClient
    {
        private readonly HttpClient _apiClient;
        public CCMApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("CCMApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(20);
        }
        public async Task<IEnumerable<CaseViewModel>> GetCaseDetailsByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return new List<CaseViewModel>();

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/case/caseDetailsByCaseCodes", oldCaseCodes);
            var casesDetails = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CaseViewModel>();
            return casesDetails.ToList();

        }
        #region FinAPI
        public async Task<IEnumerable<Office>> GetOfficeList()
        {
            var responseMessage = await _apiClient.GetAsync($"api/finAPI/officeList");
            var offices = JsonConvert.DeserializeObject<IEnumerable<Office>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<Office>();

            return offices;
        }
        public async Task<IEnumerable<BillRate>> GetBillRateByOffices(string officeCodes)
        {
            var responseMessage = await _apiClient.GetAsync($"api/finAPI/billRateByOffices?officeCodes={officeCodes}");
            var billRates =
                JsonConvert.DeserializeObject<IEnumerable<BillRate>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<BillRate>();

            return billRates;
        }
        #endregion
    }
}
