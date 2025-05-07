using Newtonsoft.Json;
using Staffing.AzureSearch.Models;
using Staffing.AzureSearch.Core.Helpers;
using Staffing.AzureSearch.Contracts.Services;
using System.Net.Http.Headers;

namespace Staffing.AzureSearch.Core.Services
{
    public class StaffingApiClient : IStaffingApiClient
    {
        private readonly HttpClient _apiClient;

        public StaffingApiClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("StaffingApiBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            SetHttpClientHeaders();
        }

        private void SetHttpClientHeaders()
        {
            _apiClient.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("Bearer", ConfigurationUtility.GetValue("Token:StaffingApi"));
        }

        public async Task InsertAzureSearchQueryLog(AzureSearchQueryLog searchQueryLog)
        {
            //Update Planning Card in DB
            var responseMessage =
               await _apiClient.PutAsJsonAsync($"api/azureSearchQueryLog", searchQueryLog);

            var upsertQueryLogStatus = JsonConvert.DeserializeObject<AzureSearchQueryLog>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return;
        }

        public async Task<IEnumerable<ResourceViewNote>> GetResourceNotesAfterLastPolledTime(DateTime dateTime, string noteTypeCode)
        {
            //Get resource profile notes for indexing
            noteTypeCode = "RP";
            var responseMessage =
               await _apiClient.GetAsync($"api/note/getResourceNotesByLastUpdatedDate?lastupdatedAfter={dateTime}&noteTypeCode={noteTypeCode}");

            var resourceNotes = JsonConvert.DeserializeObject<IEnumerable<ResourceViewNote>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceViewNote>();

            return resourceNotes;
        }

    }
}