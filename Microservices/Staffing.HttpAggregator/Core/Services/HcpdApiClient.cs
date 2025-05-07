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
    public class HcpdApiClient : IHcpdApiClient
    {
        private readonly HttpClient _apiClient;

        public HcpdApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("HcpdApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(20);
        }

        public async Task<Advisor> GetAdvisorByEmployeeCode(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return new Advisor();

            var responseMessage = await _apiClient.GetAsync($"api/Advisor/advisorByEmployeeCode?employeeCode={employeeCode}");
            var advisor = JsonConvert.DeserializeObject<Advisor>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new Advisor();
            return advisor;
        }

        public async Task<IEnumerable<Mentee>> GetMenteesByEmployeeCode(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Enumerable.Empty<Mentee>();
            }

            var responseMessage = await _apiClient.GetAsync($"api/Advisor/menteesByEmployeeCode?employeeCode={employeeCode}");
            var mentees = JsonConvert.DeserializeObject<IEnumerable<Mentee>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<Mentee>();
            return mentees;
        }
    }
}
