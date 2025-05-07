using Staffing.AzureSearch.Models;

namespace Staffing.AzureSearch.Contracts.Services
{
    public interface IQueryIntentOpenAIService
    {
        Task<OpenAIGeneratedSearchQuery> GetAzureSearchQueryFromSearchText(string searchQuery);
        Task<string> GetCompletionsDataFromSearchResults(string searchQuery, string searchResults);
        Task<string> GetCompletionsDataFromSearchResultsForNotes(string searchQuery, string searchResults);
        
    }
}
