using Staffing.Analytics.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.RepositoryInterfaces
{
    public interface IPlaceholderAllocationRepository
    {
        Task<IEnumerable<ResourceAllocation>> UpsertPlaceholderAnalyticsReportData(DataTable placeholderAllocations);
        Task<IEnumerable<string>> DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(string scheduleMasterPlaceholderIds);
        Task<PlaceholderScheduleIdsViewModel> GetPlaceholderScheduleIdsIncorrectlyProcessedInAnalytics();
    }
}
