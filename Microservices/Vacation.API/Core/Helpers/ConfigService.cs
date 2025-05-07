namespace Vacation.API.Core.Helpers
{
    public class ConfigService
    {
        public static string GetOpenAIApiKey() => ConfigurationUtility.GetValue("AzureOpenAI:APIKey");
        public static string GetOpenAIBaseUrl() => ConfigurationUtility.GetValue("AzureOpenAI:BaseUrl");
        public static string GetOpenAICompletionModel() => ConfigurationUtility.GetValue("AzureOpenAI:CompletionModel");
        public static string GetOpenAIEmbeddingModel() => ConfigurationUtility.GetValue("AzureOpenAI:EmbeddingModel");
    }
}
