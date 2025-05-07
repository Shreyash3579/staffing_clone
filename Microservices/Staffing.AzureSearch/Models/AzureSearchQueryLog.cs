namespace Staffing.AzureSearch.Models
{
    public class AzureSearchQueryLog
    {
        public string EmployeeCode { get; set; }
        public string SearchString { get; set; }
        public string SearchTriggeredFrom { get; set; }
        public string OpenAIGeneratedSearchQuery { get; set; }
        public int SearchResultsCount { get; set; }
        public bool IsErrorInOpenAiGeneratedSearchQuery { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
