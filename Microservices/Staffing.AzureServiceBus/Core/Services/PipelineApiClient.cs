using Newtonsoft.Json;
using Staffing.AzureServiceBus.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Staffing.AzureServiceBus.Core.Helpers;
using Staffing.AzureServiceBus.Contracts.Services;
using System.Linq;

namespace Staffing.AzureServiceBus.Core.Services
{
    public class PipelineApiClient : IPipelineApiClient
    {
        private readonly HttpClient _apiClient;

        public PipelineApiClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("PipelineApiBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<Opportunity> GetOpportunityByCortexId(string cortexOpportunityId)
        {
            var responseMessage =
               await _apiClient.GetAsync($"api/opportunity/getOppDataFromCortex?cortexOpportunityId={cortexOpportunityId}");
            var opportunity = JsonConvert.DeserializeObject<IList<Opportunity>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result).FirstOrDefault();

            return opportunity;
        }
    }
}
