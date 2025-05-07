namespace Staffing.AzureSearch.Contracts.Services
{
    public interface IEmbeddingGenerationService
    {
        Task<List<float>> GetEmbeddingsVectorFromOpenAI(string textToVectorize);
        Task<Dictionary<string, List<float>>> GetMultipleEmbeddingsVectorFromOpenAI(Dictionary<string, string> keyValuePairs);
    }
}
