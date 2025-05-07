using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class CcmApiClient : ICcmApiClient
    {
        private readonly HttpClient _apiClient;

        public CcmApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:CcmApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(5);
        }
        public async Task<IEnumerable<CaseViewModel>> GetCasesEndingBySpecificDate(int caseEndsBeforeNumberOfDays)
        {
            var responseMessage = await _apiClient.GetAsync($"api/case/casesEndingSoon?caseEndsBeforeNumberOfDays={caseEndsBeforeNumberOfDays}");
            var cases = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CaseViewModel>();
            return cases.ToList();
        }

        public async Task<IEnumerable<CaseViewModel>> GetCaseDataByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return Enumerable.Empty<CaseViewModel>();
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/case/caseDataByCaseCodes", oldCaseCodes);
            var casesDetails = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result);
            return casesDetails.ToList();
        }

        public async Task<IList<CaseOpportunityMap>> GetCasesByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return new List<CaseOpportunityMap>();
            var payload = new { pipelineIds };

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/caseOpportunityMap/casesByPipelineIds", payload);
            var caseOpportunityMapList =
                JsonConvert.DeserializeObject<IEnumerable<CaseOpportunityMap>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<CaseOpportunityMap>();

            return caseOpportunityMapList.ToList();
        }

        public async Task<IList<CaseViewModel>> GetCasesWithEndDateUpdatedInCCM(DateTime? lastPolledDateTime)
        {
            var columnName = Constants.EndDateColumn;
            var responseMessage =
                await _apiClient.GetAsync($"api/case/casesWithStartOrEndDateUpdatedInCCM?columnName={columnName}&lastPollDateTime={lastPolledDateTime}");
            var preponedCases =
                JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? new List<CaseViewModel>();

            return preponedCases.ToList();
        }

        public async Task<CaseMasterViewModel> GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(DateTime? lastPolledDateTime)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/case/caseMasterAndCaseMasterHistoryChangesSinceLastPolled?lastPolledDateTime={lastPolledDateTime}");
            var caseMasterAndCaseMasterHistoryDataChanges =
                JsonConvert.DeserializeObject<CaseMasterViewModel>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? new CaseMasterViewModel();

            return caseMasterAndCaseMasterHistoryDataChanges;
        }

        public async Task<IEnumerable<CaseAdditionalInfo>> GetCaseAdditionalInfo(DateTime? lastUpdated)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/case/caseAdditionalInfo?lastUpdated={lastUpdated}");
            var caseAdditionalInfos =
                JsonConvert.DeserializeObject<IEnumerable<CaseAdditionalInfo>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? new List<CaseAdditionalInfo>();

            return caseAdditionalInfos;
        }

        public async Task<IList<CaseAttribute>> GetCaseAttributesByLastUpdatedDate(DateTime? lastUpdatedDate)
        {
            var responseMessage = await _apiClient.GetAsync($"api/CaseAttribute/getcaseattributesbylastupdateddate?lastupdateddate={lastUpdatedDate}");
            var currencyrates = JsonConvert.DeserializeObject<IEnumerable<CaseAttribute>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CaseAttribute>();
            return currencyrates.ToList();
        }

        #region FinAPI 

        public async Task<IEnumerable<Office>> GetOfficesFlatListByRegionOrCluster(int clusterRegionCode)
        {
            var responseMessage = await _apiClient.GetAsync($"api/finAPI/officesFlatListByRegionOrCluster?clusterRegionCode={clusterRegionCode}");
            var offices = JsonConvert.DeserializeObject<IEnumerable<Office>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<Office>();

            return offices;
        }

        public async Task<IEnumerable<Office>> GetOfficeList()
        {
            var responseMessage = await _apiClient.GetAsync($"api/finAPI/officeList");
            var offices = JsonConvert.DeserializeObject<IEnumerable<Office>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<Office>();

            return offices;
        }
        public async Task<IEnumerable<RevOffice>> GetRevOfficeList()
        {
            var responseMessage = await _apiClient.GetAsync($"api/finAPI/revOfficeList");
            var offices = JsonConvert.DeserializeObject<IEnumerable<RevOffice>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<RevOffice>();

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

        public async Task<IEnumerable<BillRate>> GetBillRates()
        {
            var responseMessage = await _apiClient.GetAsync($"api/finAPI/billRates");
            var billRates =
                JsonConvert.DeserializeObject<IEnumerable<BillRate>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<BillRate>();
            return billRates;
        }
        #endregion

        #region ClientCaseAPI

        public async Task<IEnumerable<CaseViewModel>> GetModifiedCasesAfterLastPolledTime(DateTime lastPolledTime)
        {
            var responseMessage = await _apiClient.GetAsync($"api/clientCaseAPI/getModifiedCasesAfterLastPolledTime?lastPolledTime={lastPolledTime}");
            var modifiedCases =
                JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<CaseViewModel>();
            
            return modifiedCases;
        }

        #endregion
    }
}
