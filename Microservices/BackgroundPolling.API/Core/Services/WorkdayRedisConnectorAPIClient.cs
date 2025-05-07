using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.Models.Workday;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class WorkdayRedisConnectorAPIClient : IWorkdayRedisConnectorAPIClient
    {
        private readonly HttpClient _apiClient;

        public WorkdayRedisConnectorAPIClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:WorkdayRedisConnectorApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(30);
        }

        public async Task<IEnumerable<LOATransaction>> GetEmployeesLOATransactionsByModifiedDate(DateTime date)
        {
            var responseMessage = await _apiClient.GetAsync($"api/employeeLOA/getEmployeesLOATransactionsByModifiedDate?fromDate={date}");
            var lOAs = JsonConvert.DeserializeObject<IEnumerable<LOATransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<LOATransaction>();
            return lOAs.ToList();
        }

        public async Task<IEnumerable<LOATransaction>> GetEmployeesLOATransactionsByEfectiveDate(DateTime date)
        {
            var responseMessage = await _apiClient.GetAsync($"api/employeeLOA/getEmployeesLOATransactionsByEffectiveDate?fromDate={date}");
            var lOAs = JsonConvert.DeserializeObject<IEnumerable<LOATransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<LOATransaction>();
            return lOAs.ToList();
        }

        public async Task<IEnumerable<LOATransaction>> GetEmployeesLOATransactions(string employeeCodes)
        {
            var payload = new
            {
                employeeCodes
            };
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/employeeLOA/getEmployeesAllLOATransactions",payload);
            var lOAs = JsonConvert.DeserializeObject<IEnumerable<LOATransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<LOATransaction>();
            return lOAs.ToList();
        }

        public async Task<IEnumerable<LOATransaction>> GetEmployeesLOATransactionsPendingFromRedis()
        {
            var responseMessage = await _apiClient.GetAsync($"api/EmployeeLOA/getPendingLOATransactions");
            var loaTransactions = JsonConvert.DeserializeObject<IEnumerable<LOATransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<LOATransaction>();

            return loaTransactions;
        }

        public async Task<IEnumerable<EmployeeTransaction>> GetEmployeesStaffingTransactionsPendingFromRedis()
        {
            var responseMessage = await _apiClient.GetAsync($"api/EmployeeTransaction/getPendingTransactions");
            var transactions = JsonConvert.DeserializeObject<IEnumerable<EmployeeTransaction>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? new List<EmployeeTransaction>();

            return transactions;
        }
    }
}
