namespace Staffing.AzureSearch.Core.Helpers
{
    public class ConfigService
    {
        public static string GetOpenAIApiKey() => ConfigurationUtility.GetValue("AzureOpenAI:APIKey");
        public static string GetOpenAIBaseUrl() => ConfigurationUtility.GetValue("AzureOpenAI:BaseUrl");
        public static string GetOpenAICompletionModel() => ConfigurationUtility.GetValue("AzureOpenAI:CompletionModel");
        public static string GetOpenAIEmbeddingModel() => ConfigurationUtility.GetValue("AzureOpenAI:EmbeddingModel");

        //-------------------------------------------
        public static string GetSearchBaseUrl() => ConfigurationUtility.GetValue("AzureSearch:BaseUrl");
        public static string GetSearchQueryApiKey() => ConfigurationUtility.GetValue("AzureSearch:QueryApiKey");
        public static string GetSearchAdminApiKey() => ConfigurationUtility.GetValue("AzureSearch:AdminApiKey");
        public static string GetSearchIndexName() => ConfigurationUtility.GetValue("AzureSearch:IndexName");
        public static string GetSearchResultsCount() => ConfigurationUtility.GetValue("AzureSearch:ResultsCount");

    }
}
