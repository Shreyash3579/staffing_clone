using CaseIntake.API.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaseIntake.API.Core.Helpers;

namespace CaseIntake.API.Contracts.Services
{
    public class CCMApiClient : ICCMApiClient
    {
        private readonly HttpClient _apiClient;
        public CCMApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:CCMApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);

        }

        //Use this faster end point when basic cases data. Excludes taxonomies, case attributes, client priority
        //public async Task<IList<CaseData>> GetCaseDataBasicByCaseCodes(string oldCaseCodeList)
        //{
        //    if (string.IsNullOrEmpty(oldCaseCodeList))
        //        return new List<CaseData>();
        //    var responseMessage = await _apiClient.PostAsJsonAsync($"api/case/caseDataBasicByCaseCodes", oldCaseCodeList);
        //    var casesDetails = JsonConvert.DeserializeObject<IEnumerable<CaseData>>(responseMessage.Content?.ReadAsStringAsync().Result);
        //    return casesDetails.ToList();
        //}

        public async Task<CaseDetails> GetCaseDetailsByCaseCode(string oldCaseCode)
        {
            if (string.IsNullOrEmpty(oldCaseCode))
                return new CaseDetails();
            var responseMessage = await _apiClient.GetAsync($"api/case/caseDetailsByCaseCode?oldCaseCode={oldCaseCode}");
            var casesDetails = JsonConvert.DeserializeObject<CaseDetails>(responseMessage.Content?.ReadAsStringAsync().Result);
            return casesDetails;
        }

    }
}
