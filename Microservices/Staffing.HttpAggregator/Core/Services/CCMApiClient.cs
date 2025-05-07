using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class CCMApiClient : ICCMApiClient
    {
        private readonly HttpClient _apiClient;
        public CCMApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("CCMApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);

        }

        //Use this faster end point when basic cases data. Excludes taxonomies, case attributes, client priority
        public async Task<IList<CaseData>> GetCaseDataBasicByCaseCodes(string oldCaseCodeList)
        {
            if (string.IsNullOrEmpty(oldCaseCodeList))
                return new List<CaseData>();
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/case/caseDataBasicByCaseCodes", oldCaseCodeList);
            var casesDetails = JsonConvert.DeserializeObject<IEnumerable<CaseData>>(responseMessage.Content?.ReadAsStringAsync().Result);
            return casesDetails.ToList();
        }

        public async Task<IList<CaseData>> GetCaseDataByCaseCodes(string oldCaseCodeList)
        {
            if (string.IsNullOrEmpty(oldCaseCodeList))
                return new List<CaseData>();
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/case/caseDataByCaseCodes", oldCaseCodeList);
            var casesDetails = JsonConvert.DeserializeObject<IEnumerable<CaseData>>(responseMessage.Content?.ReadAsStringAsync().Result);
            return casesDetails.ToList();
        }

        public async Task<CaseDetails> GetCaseDetailsByCaseCode(string oldCaseCode)
        {
            if (string.IsNullOrEmpty(oldCaseCode))
                return new CaseDetails();
            var responseMessage = await _apiClient.GetAsync($"api/case/caseDetailsByCaseCode?oldCaseCode={oldCaseCode}");
            var casesDetails = JsonConvert.DeserializeObject<CaseDetails>(responseMessage.Content?.ReadAsStringAsync().Result);
            return casesDetails;
        }

        //public async Task<IList<CaseData>> GetCasesByOffices(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate)
        //{
        //    var responseMessage = await _apiClient.GetAsync($"api/case/casesByOffices?officeCodes={officeCodes}&caseTypeCodes={caseTypeCodes}&startDate={startDate}&endDate={endDate}");
        //    var cases = JsonConvert.DeserializeObject<IEnumerable<CaseData>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<CaseData>();
        //    return cases.ToList();

        //}

        public async Task<IList<CaseData>> GetNewDemandCasesByOffices(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate, string clientCodes)
        {
            if (ValidateGetCasesParamterers(officeCodes, caseTypeCodes, startDate, endDate))
            {
                var responseMessage = await _apiClient.GetAsync($"api/case/newDemandCasesByOffices?officeCodes={officeCodes}&caseTypeCodes={caseTypeCodes}&startDate={startDate}&endDate={endDate}&clientCodes={clientCodes}");
                var cases = JsonConvert.DeserializeObject<IEnumerable<CaseData>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<CaseData>();
                return cases.ToList();
            }
            return new List<CaseData>();

        }

        public async Task<IList<CaseData>> GetActiveCasesExceptNewDemandsByOffices(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate, string clientCodes)
        {
            if (ValidateGetCasesParamterers(officeCodes, caseTypeCodes, startDate, endDate))
            {
                var responseMessage = await _apiClient.GetAsync($"api/case/activeCasesExceptNewDemandsByOffices?officeCodes={officeCodes}&caseTypeCodes={caseTypeCodes}&startDate={startDate}&endDate={endDate}&clientCodes={clientCodes}");
                var cases = JsonConvert.DeserializeObject<IEnumerable<CaseData>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<CaseData>();
                return cases.ToList();
            }
            return new List<CaseData>();
        }

        public async Task<IList<CaseAttribute>> GetCaseAttributeLookupList()
        {
            var responseMessage = await _apiClient.GetAsync($"api/lookup/caseAttributes");
            var caseAttributes = JsonConvert.DeserializeObject<IEnumerable<CaseAttribute>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<CaseAttribute>();
            return caseAttributes.ToList();
        }

        public async Task<IEnumerable<CaseData>> GetCasesForTypeahead(string searchString, string officeCodes)
        {
            var url = $"api/case/typeaheadCases?searchString={searchString}";
            if (!string.IsNullOrEmpty(officeCodes))
            {
                url += $"&$filter=managingOfficeCode in ({officeCodes}) or billingOfficeCode in ({officeCodes})";
            }
            var responseMessage = await _apiClient.GetAsync(url);
            var cases = JsonConvert.DeserializeObject<IEnumerable<CaseData>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<CaseData>();
            return cases.ToList();
        }

        public async Task<IList<CaseData>> GetCasesWithTaxonomiesByCaseCodes(string oldCaseCodeList)
        {
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/case/casesWithTaxonomiesByCaseCodes", oldCaseCodeList);
            var cases = JsonConvert.DeserializeObject<IEnumerable<CaseData>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<CaseData>();
            return cases.ToList();
        }

        public async Task<IEnumerable<CaseData>> GetCasesActiveAfterSpecifiedDate(DateTime? date)
        {
            var responseMessage = await _apiClient.GetAsync($"api/case/casesActiveAfterSpecifiedDate?date={date}");
            var cases = JsonConvert.DeserializeObject<IEnumerable<CaseData>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<CaseData>();
            return cases.ToList();
        }

        public async Task<IEnumerable<Office>> GetOfficeList()
        {
            var responseMessage = await _apiClient.GetAsync($"api/finAPI/officeList");
            var offices = JsonConvert.DeserializeObject<IEnumerable<Office>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<Office>();

            return offices;
        }

        public async Task<IEnumerable<Office>> GetOfficesFlatListByRegionOrCluster(int clusterRegionCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/finAPI/officesFlatListByRegionOrCluster?clusterRegionCode={clusterRegionCode}");
            var offices = JsonConvert.DeserializeObject<IEnumerable<Office>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<Office>();

            return offices;
        }

        public async Task<OfficeHierarchy> GetOfficeHierarchyByOffices(string officeCodes)
        {
            var responseMessage = await _apiClient.GetAsync($"api/finAPI/officeHierarchyByOffices?officeCodes={officeCodes}");
            var officeHierarchyByOffices = JsonConvert.DeserializeObject<OfficeHierarchy>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new OfficeHierarchy();

            return officeHierarchyByOffices;
        }

        #region Private Methods
        public bool ValidateGetCasesParamterers(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate || string.IsNullOrEmpty(officeCodes) || string.IsNullOrEmpty(caseTypeCodes))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}
