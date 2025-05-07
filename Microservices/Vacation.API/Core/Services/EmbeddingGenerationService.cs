using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Vacation.API.Models;
using Vacation.API.Contracts.Services;
using Vacation.API.Core.Helpers;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Vacation.API.Core.Services
{
    public class EmbeddingGenerationService : IEmbeddingGenerationService
    {
        private readonly string azureOpenAIAPIKey;
        private readonly string azureOpenAIEmbeddingModel;
        private readonly HttpClient _apiClient;

        public EmbeddingGenerationService(HttpClient httpClient)
        {
            azureOpenAIAPIKey = ConfigService.GetOpenAIApiKey();
            azureOpenAIEmbeddingModel = ConfigService.GetOpenAIEmbeddingModel();

            _apiClient = httpClient;
            ConfigureHttpClient(_apiClient,ConfigService.GetOpenAIBaseUrl());
        }

        public async Task<List<float>> GetEmbeddingsVectorFromOpenAI(string textToVectorize)
        {
            var response = await GetEmbeddings(textToVectorize);
            
            var embeddingsData= ExtractEmbeddings(response);
            return embeddingsData;
        }

        public async Task<Dictionary<string, List<float>>> GetMultipleEmbeddingsVectorFromOpenAI([FromBody] Dictionary<string, string> keyValuePairs)
        {
            var embeddedData = new Dictionary<string, List<float>>();

            foreach (var pair in keyValuePairs)
            {
                var textToVectorize = pair.Value;
                var response = await GetEmbeddings(textToVectorize);

                try
                {
                    embeddedData.Add(pair.Key, ExtractEmbeddings(response));
                }
                catch (Exception)
                {
                    // Log or handle the exception as needed
                }
            }

            return embeddedData;
        }

        #region Private Helpers

        private void ConfigureHttpClient(HttpClient client, string azureOpenAIBaseUrl)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("api-key", azureOpenAIAPIKey);
            client.BaseAddress = new Uri(azureOpenAIBaseUrl);
        }

        private async Task<HttpResponseMessage> GetEmbeddings(string textToVectorize)
        {
            var response = await _apiClient.PostAsJsonAsync($"openai/deployments/{azureOpenAIEmbeddingModel}/embeddings?api-version=2023-05-15",
                new { input = textToVectorize });
            response.EnsureSuccessStatusCode();

            return response;
        }

        private List<float> ExtractEmbeddings(HttpResponseMessage response)
        {
            try
            {
                var responseObject = response.Content.ReadAsStringAsync().Result; // Avoid using async/await in a synchronous context
                var results = JsonConvert.DeserializeObject<EmbeddingResult>(responseObject);
                return results.Data[0].Embedding;
            }
            catch 
            { 
                return Enumerable.Empty<float>().ToList(); 
            }

        }

        #endregion
    }

}