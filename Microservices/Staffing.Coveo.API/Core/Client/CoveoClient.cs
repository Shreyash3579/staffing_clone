using Newtonsoft.Json;
using Staffing.Coveo.API.Contracts.Services;
using Staffing.Coveo.API.Core.Helpers;
using Staffing.Coveo.API.Models;
using Staffing.Coveo.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Staffing.Coveo.API.Core.Client
{
    public class CoveoClient : ICoveoClient
    {
        private readonly HttpClient _coveoClient;
        private readonly HttpQueryBuilder _httpQueryBuilder;
        private readonly string _apiKey;
        private string _coveoSearchURL;
        private string _coveoAnalyticsSearchActionClause;
        private string _coveoAnalyticsSearchLanguage;
        private bool _coveoAnalyticsSearchAnonymous;
        private string _coveoCreateFileContainerURL;
        private string _coveoOrganizationId;
        private string _coveoPushBatchToSourceBaseUrl;
        private string _coveoAllocationSourceId;

        public CoveoClient(HttpClient httpClient)
        {
            _apiKey = ConfigurationUtility.GetValue("Coveo:apiKey");
            _coveoSearchURL = ConfigurationUtility.GetValue("Coveo:searchURL");
            _coveoAnalyticsSearchActionClause = ConfigurationUtility.GetValue("Coveo:analytics:search:actionCause");
            _coveoAnalyticsSearchLanguage = ConfigurationUtility.GetValue("Coveo:analytics:search:language");
            _coveoAnalyticsSearchAnonymous = Convert.ToBoolean(ConfigurationUtility.GetValue("Coveo:analytics:search:anonymous"));
            _coveoCreateFileContainerURL = ConfigurationUtility.GetValue("Coveo:createFileContainerURL");
            _coveoOrganizationId = ConfigurationUtility.GetValue("Coveo:organizationId");
            _coveoPushBatchToSourceBaseUrl = ConfigurationUtility.GetValue("Coveo:pushBatchToSourceBaseUrl");
            _coveoAllocationSourceId = ConfigurationUtility.GetValue("Coveo:allocationSource:sourceId");
            _httpQueryBuilder = new HttpQueryBuilder();
            _coveoClient = httpClient;
            SetHttpClientHeaders();
        }

        /// <summary>
        /// This will search only on QA source based on all the fields or first name,last name or employee code
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="userDisplayName"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<(ResourcesViewModel, AnalyticsSearchViewModel)> SearchByResource(string searchTerm, string userDisplayName, string username, bool? test = false)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return (new ResourcesViewModel(), new AnalyticsSearchViewModel());
            _coveoSearchURL = _httpQueryBuilder.GetUrlWithQueryAndAdvancedQuery(searchTerm, ConfigurationUtility.AppSettings.Coveo.ResourceSource, test).ToString();
            
            HttpResponseMessage response = await _coveoClient.GetAsync(_coveoSearchURL);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<ResourceResponse>(responseBody);

            dynamic resourcesData = MapHttpResponse.MapCommonSourceResponse(JsonConvert.DeserializeObject<ResourceResponse>(responseBody));

            var analyticsSearchData = GetAnalyticsSearchData(searchTerm, deserializedResponse, null, null, userDisplayName, username);

            return (resourcesData, analyticsSearchData);
        }

        /// <summary>
        /// This will search on SM source based on all fields or old case code
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public async Task<dynamic> SearchByAllocation(string searchTerm)
        {
            _coveoSearchURL = _httpQueryBuilder.GetUrlWithQueryAndAdvancedQuery(searchTerm, ConfigurationUtility.AppSettings.Coveo.AllocationSource).ToString();
            HttpResponseMessage response = await _coveoClient.GetAsync(_coveoSearchURL);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var allocationData = JsonConvert.DeserializeObject(responseBody);
            return allocationData;
        }

        /// <summary>
        /// This will search on cases source based on case name or old case code
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="userDisplayName"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<Case>, AnalyticsSearchViewModel)> SearchByCase(string searchTerm, string userDisplayName, string username)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return (Enumerable.Empty<Case>(), new AnalyticsSearchViewModel());
            _coveoSearchURL = _httpQueryBuilder.GetUrlWithQueryAndAdvancedQuery(searchTerm, ConfigurationUtility.AppSettings.Coveo.CaseSource).ToString();
            
            HttpResponseMessage response = await _coveoClient.GetAsync(_coveoSearchURL);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<CaseResponse>(responseBody);

            var caseData = MapHttpResponse.MapCaseResponse(JsonConvert.DeserializeObject<CaseResponse>(responseBody));

            var analyticsSearchData = GetAnalyticsSearchData(searchTerm, null, deserializedResponse, null, userDisplayName, username);

            return (caseData, analyticsSearchData);
        }

        /// <summary>
        /// This will search on opportunity source based on opportunity name
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="userDisplayName"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<Opportunity>, AnalyticsSearchViewModel)> SearchByOpportunity(string searchTerm, string userDisplayName, string username)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return (Enumerable.Empty<Opportunity>(), new AnalyticsSearchViewModel());
            _coveoSearchURL = _httpQueryBuilder.GetUrlWithQueryAndAdvancedQuery(searchTerm, ConfigurationUtility.AppSettings.Coveo.OpportunitySource).ToString();
            HttpResponseMessage response = await _coveoClient.GetAsync(_coveoSearchURL);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<OpportunityResponse>(responseBody);

            var opportunityData = MapHttpResponse.MapOpportunityResponse(JsonConvert.DeserializeObject<OpportunityResponse>(responseBody));

            var analyticsSearchData = GetAnalyticsSearchData(searchTerm, null, null, deserializedResponse, userDisplayName, username);

            return (opportunityData, analyticsSearchData);
        }

        public async Task<IEnumerable<Allocation>> UpsertOrDeleteAllocationIndexes(IEnumerable<ResourceAllocation> allocations)
        {
            var isOperationSuccessful = false;
            var fileContainer = await CreateFileContainerForPushAPI(_coveoCreateFileContainerURL);

            var upsertedAllocations = CreateObjectForContainer(allocations);

            if (await BatchUploadedToFileContainer(fileContainer.UploadUri, upsertedAllocations))
            {
                isOperationSuccessful = await PushFileContainerToSource(upsertedAllocations, fileContainer.FileId);
            }

            return isOperationSuccessful ? ConvertResourceAllocationModelToAllocationModel(allocations) : Enumerable.Empty<Allocation>();
        }

        #region Private Methods

        private IEnumerable<Allocation> ConvertResourceAllocationModelToAllocationModel(IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var allocations = resourceAllocations.Select(item => new Allocation
            {
                Id = item.Id,
                ClientCode = item.ClientCode,
                CaseCode = item.CaseCode,
                OldCaseCode = item.OldCaseCode,
                EmployeeCode = item.EmployeeCode,
                ServiceLineCode = item.ServiceLineCode,
                ServiceLineName = item.ServiceLineName,
                OperatingOfficeCode = item.OperatingOfficeCode,
                CurrentlevelGrade = item.CurrentLevelGrade,
                AllocationPercent = item.Allocation,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                PipelineId = item.PipelineId,
                InvestmentCode = item.InvestmentCode,
                InvestmentName = item.InvestmentName,
                CaseRoleCode = item.CaseRoleCode,
                CaseRoleName = item.CaseRoleName,
                Notes = item.Notes,
                LastUpdated = item.LastUpdated,
                LastUpdatedBy = item.LastUpdatedBy
            });

            return allocations;
        }

        private IEnumerable<UpsertedAllocationModel> ConvertResourceAllocationModelToUpsertedAllocationModel(IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var allocations = resourceAllocations.Select(item => new UpsertedAllocationModel
            {
                id = item.Id,
                documentId = "https://staffingqa.bain.com/staffing/home?id=" + item.Id,
                data = item.Notes,
                clientCode = item.ClientCode,
                caseCode = item.CaseCode,
                oldCaseCode = item.OldCaseCode,
                employeeCode = item.EmployeeCode,
                serviceLineCode = item.ServiceLineCode,
                serviceLineName = item.ServiceLineName,
                operatingOfficeCode = item.OperatingOfficeCode,
                currentlevelGrade = item.CurrentLevelGrade,
                allocationPercent = item.Allocation,
                startDate = item.StartDate,
                endDate = item.EndDate,
                pipelineId = item.PipelineId,
                investmentCode = item.InvestmentCode,
                investmentName = item.InvestmentName,
                caseRoleCode = item.CaseRoleCode,
                caseRoleName = item.CaseRoleName,
                notes = item.Notes,
                lastUpdated = item.LastUpdated,
                lastUpdatedBy = item.LastUpdatedBy
            });

            return allocations;
        }

        private IEnumerable<DeletedRecordModel> ConvertResourceAllocationModelToDeletedAllocationModel(IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var allocations = resourceAllocations.Select(item => new DeletedRecordModel
            {
                documentId = "https://staffingqa.bain.com/staffing/home?id=" + item.Id,
                deleteChildren = true
            });

            return allocations;
        }

        private async Task<bool> BatchUploadedToFileContainer(string uploadUri, CoveoFileContainerTransactions upsertedAllocations)
        {
            var json = JsonConvert.SerializeObject(upsertedAllocations);

            var requiredHeaders = new List<(string header, string value)>();
            requiredHeaders.Add(("x-amz-server-side-encryption", "AES256"));

            SetHttpClientHeaders(true, requiredHeaders);

            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, uploadUri);
            req.Content = new StringContent(json);
            req.Content.Headers.Remove("Content-Type");
            req.Content.Headers.Add("Content-Type", "application/octet-stream");

            var responseMessage = await _coveoClient.SendAsync(req);
            return responseMessage.IsSuccessStatusCode;
        }

        private async Task<CoveoFileContainerDetails> CreateFileContainerForPushAPI(string _coveoCreateFileContainerURL)
        {
            var responseMessage = await _coveoClient.PostAsJsonAsync(_coveoCreateFileContainerURL, new { });

            var fileContainerURL = JsonConvert.DeserializeObject<CoveoFileContainerDetails>(responseMessage.Content
                ?.ReadAsStringAsync().Result) ?? new CoveoFileContainerDetails();

            return fileContainerURL;
        }

        private CoveoFileContainerTransactions CreateObjectForContainer(IEnumerable<ResourceAllocation> allocations)
        {
            var containerObject = new CoveoFileContainerTransactions()
            {
                addOrUpdate = ConvertResourceAllocationModelToUpsertedAllocationModel(allocations.Where(a => a.Action != "Delete")),
                delete = ConvertResourceAllocationModelToDeletedAllocationModel(allocations.Where(a => a.Action == "Delete"))
            };
            return containerObject;
        }

        private async Task<bool> PushFileContainerToSource(CoveoFileContainerTransactions allocations, Guid FileId)
        {
            SetHttpClientHeaders();
            var responseMessage = await _coveoClient.PutAsJsonAsync($"{_coveoPushBatchToSourceBaseUrl}/{_coveoOrganizationId}/sources/" +
                $"{_coveoAllocationSourceId}/documents/batch?fileId={FileId}"
                , new { });

            return responseMessage.IsSuccessStatusCode;
        }

        private void SetHttpClientHeaders(bool noAuth = false, List<(string name, string value)> newHeaders = null)
        {
            _coveoClient.DefaultRequestHeaders.Remove("Authorization");

            if (!noAuth)
                _coveoClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            if (newHeaders != null)
            {
                foreach (var header in newHeaders)
                {
                    if (_coveoClient.DefaultRequestHeaders.Contains(header.name))
                        _coveoClient.DefaultRequestHeaders.Remove(header.name);
                    _coveoClient.DefaultRequestHeaders.Add(header.name, header.value);
                }
            }
        }

        private AnalyticsSearchViewModel GetAnalyticsSearchData(string searchTerm, ResourceResponse resourceResponse = null,
            CaseResponse caseResponse = null, OpportunityResponse opportunityResponse = null, string userDisplayName = null, string username = null)
        {
            var analyticsSearchData = new AnalyticsSearchViewModel()
            {
                anonymous = string.IsNullOrEmpty(userDisplayName) && string.IsNullOrEmpty(username),
                actionCause = _coveoAnalyticsSearchActionClause,
                language = _coveoAnalyticsSearchLanguage,
                queryText = searchTerm,
                responseTime = resourceResponse != null
                    ? resourceResponse.Duration
                    : caseResponse !=null 
                        ? caseResponse.Duration
                        : opportunityResponse != null
                            ? opportunityResponse.Duration
                            : int.MinValue,
                searchQueryUid = resourceResponse != null
                    ? resourceResponse.SearchUid
                    : caseResponse != null
                        ? caseResponse.SearchUid
                        : opportunityResponse != null
                            ? opportunityResponse.SearchUid
                            : Guid.Empty,
                userDisplayName = userDisplayName,
                username = username,
                numberOfResults = resourceResponse != null
                    ? resourceResponse.TotalCount
                    : caseResponse != null
                        ? caseResponse.TotalCount
                        : opportunityResponse != null
                            ? opportunityResponse.TotalCount
                            : int.MinValue
            };
            return analyticsSearchData;
        }
        #endregion
    }
}
