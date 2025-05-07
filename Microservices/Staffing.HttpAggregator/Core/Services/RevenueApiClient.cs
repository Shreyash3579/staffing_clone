using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class RevenueApiClient : IRevenueApiClient
    {
        private readonly HttpClient _apiClient;
        public RevenueApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("RevenueApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:RevenueApi"));
        }

        public async Task<IList<Revenue>> GetRevenueByClientCodeAndCaseCode(int? clientCode, int? caseCode, DateTime startDate, DateTime endDate)
        {
            HttpResponseMessage responseMessage;

            IList<dynamic> payload = new List<dynamic>();
            if (caseCode != null)
                payload.Add(new { clientCode, caseCode });
            else
                payload.Add(new { clientCode });

            if (startDate != null && endDate != null)
            {
                responseMessage = await _apiClient.PostAsJsonAsync($"api/v1/Revenue/getrevenuebyclientcase?startDate={startDate}&endDate={endDate}", payload);
            }
            else
            {
                responseMessage = await _apiClient.PostAsJsonAsync($"api/v1/Revenue/getrevenuebyclientcase", payload);
            }

            var revenues = JsonConvert.DeserializeObject<IEnumerable<Revenue>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<Revenue>();

            return revenues.ToList();
        }

        public async Task<IList<Revenue>> GetRevenueByServiceLine(string serviceLine, DateTime startDate, DateTime endDate)
        {
            HttpResponseMessage responseMessage;

            if (startDate != null && endDate != null)
            {
                responseMessage = await _apiClient.GetAsync($"api/v1/Revenue/getrevenuebyserviceline?serviceLineCodes={serviceLine}&startDate={startDate}&endDate={endDate}");
            }
            else
            {
                responseMessage = await _apiClient.GetAsync($"api/v1/Revenue/getrevenuebyserviceline?serviceLineCodes={serviceLine}");
            }

            var revenues = JsonConvert.DeserializeObject<IEnumerable<Revenue>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<Revenue>();

            return revenues.ToList();
        }
    }
}
