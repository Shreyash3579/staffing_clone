using Hangfire;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface IPlaceholderAnalyticsService
    {
        Task<IEnumerable<string>> CreatePlaceholderAnalyticsReport(string scheduleMasterPlaceholderIds);
        Task DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(string scheduleMasterPlaceholderIds);

        [JobDisplayName("Correct placeholder analytics data")]
        Task<IEnumerable<string>> CorrectPlaceholderAnalyticsData();
    }
}
