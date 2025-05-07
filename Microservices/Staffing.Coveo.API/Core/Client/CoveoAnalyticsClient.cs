using Newtonsoft.Json;
using Staffing.Coveo.API.Contracts.Services;
using Staffing.Coveo.API.Core.Helpers;
using Staffing.Coveo.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Staffing.Coveo.API.Core.Client
{
    public class CoveoAnalyticsClient : ICoveoAnalyticsClient
    {
        private readonly HttpClient _coveoClient;
        private readonly HttpQueryBuilder _httpQueryBuilder;
        private readonly string _apiKey;
        private string _coveoAnalyticsSearchURL;
        private string _coveoAnalyticsClickURL;
        private string _coveoAnalyticsClickActionClause;
        private string _coveoAnalyticsClickLanguage;
        private int _coveoAnalyticsClickDocumentPosition;

        public CoveoAnalyticsClient(HttpClient httpClient)
        {
            _apiKey = ConfigurationUtility.GetValue("Coveo:apiKey");
            _coveoAnalyticsSearchURL = ConfigurationUtility.GetValue("Coveo:analytics:analytics:search:url");
            _coveoAnalyticsClickURL = ConfigurationUtility.GetValue("Coveo:analytics:analytics:click:url");
            _coveoAnalyticsClickActionClause = ConfigurationUtility.GetValue("Coveo:analytics:click:actionCause");
            _coveoAnalyticsClickLanguage = ConfigurationUtility.GetValue("Coveo:analytics:click:language");
            _coveoAnalyticsClickDocumentPosition = Convert.ToInt32(ConfigurationUtility.GetValue("Coveo:analytics:click:documentPosition"));
            _httpQueryBuilder = new HttpQueryBuilder();
            _coveoClient = httpClient;
            SetHttpClientHeaders();
        }

        public async Task Search(AnalyticsSearchViewModel analyticsData, string sourceTab, string userIPAddress)
        {
            if (analyticsData == null)
                return;
            analyticsData.queryPipeline = ConfigurationUtility.AppSettings.Coveo.Pipeline;
            analyticsData.originLevel1 = ConfigurationUtility.AppSettings.Coveo.SearchHub;
            analyticsData.originLevel2 = sourceTab;

            _coveoAnalyticsSearchURL = _httpQueryBuilder.GetUrlForAnalytics().ToString();

            SendRequestToCoveo(_coveoAnalyticsSearchURL, HttpMethod.Post, analyticsData, userIPAddress);
            return;
        }

        public async Task<dynamic> LogClickEventInCoveoAnalytics(AnalyticsClickViewModel analyticsData, string userIPAddress)
        {
            if (analyticsData == null)
                return null;
            analyticsData.actionCause = _coveoAnalyticsClickActionClause;
            analyticsData.documentPosition = _coveoAnalyticsClickDocumentPosition;
            analyticsData.language = _coveoAnalyticsClickLanguage;
            analyticsData.queryPipeline = ConfigurationUtility.AppSettings.Coveo.Pipeline;
            analyticsData.originLevel1 = ConfigurationUtility.AppSettings.Coveo.SearchHub;
            analyticsData.originLevel2 = analyticsData.originLevel2;

            _coveoAnalyticsClickURL = _httpQueryBuilder.GetUrlForAnalytics("click").ToString();

            var returnObject = await SendRequestToCoveo(_coveoAnalyticsClickURL, HttpMethod.Post, analyticsData, userIPAddress);
            return returnObject;
        }

        #region Private Methods
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

        private async Task<string> SendRequestToCoveo(string uploadUri, HttpMethod requestType, dynamic requestData, string userIPAddress)
        {
            var json = JsonConvert.SerializeObject(requestData);

            HttpRequestMessage req = new HttpRequestMessage(requestType, uploadUri);
            req.Content = new StringContent(json);

            var headerList = new List<(string name, string value)>();
            
            headerList.Add(("Content-Type", "application/json"));
            
            if (!string.IsNullOrEmpty(userIPAddress))
                headerList.Add(("X-Forwarded-For", userIPAddress));

            SetHttpRequestMessageHeader(req, headerList);

            var responseMessage = await _coveoClient.SendAsync(req);
            var responseBody = await responseMessage.Content.ReadAsStringAsync();

            return responseBody;
        }

        private void SetHttpRequestMessageHeader(HttpRequestMessage request, List<(string name, string value)> requestHeaders)
        {
            if(request != null && requestHeaders != null)
            {
                foreach (var header in requestHeaders)
                {
                    if (request.Content.Headers.Contains(header.name))
                        request.Content.Headers.Remove(header.name);
                    request.Content.Headers.Add(header.name, header.value);
                }
            }
        }

        #endregion
    }
}
