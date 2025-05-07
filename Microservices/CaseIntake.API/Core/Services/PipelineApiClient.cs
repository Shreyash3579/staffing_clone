using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using CaseIntake.API.Contracts.Services;
using System.Net.Http;
using CaseIntake.API.Models;
using CaseIntake.API.Core.Helpers;


namespace CaseIntake.API.Core.Services
{
    public class PipelineApiClient : IPipelineApiClient
    {
        private readonly HttpClient _apiClient;

        public PipelineApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:PipelineApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<OpportunityDetails> GetOpportunityDetailsByPipelineId(Guid? pipelineId)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/opportunity/opportunityDetails?pipelineId={pipelineId.ToString()}");
            var opportunity =
                JsonConvert.DeserializeObject<OpportunityDetails>(responseMessage.Content?.ReadAsStringAsync()
                    .Result) ?? new OpportunityDetails();
            return opportunity;
        }


    }
}

