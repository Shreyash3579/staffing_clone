using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IStaffingAnalyticsApiClient
    {
        Task CreateAnalyticsReport(string scheduleIds);
        Task UpdateAnlayticsDataForUpsertedCommitment(string commitmentIds);
        Task UpdateAnlayticsDataForDeletedCommitment(string commitmentIds);
        void CreatePlaceholderAnalyticsReport(string placeholderScheduleIds);
        Task<IEnumerable<ResourceAvailability>> UpdateCostForResourcesAvailableInFullCapacity(string employeeCodes);        
        Task<List<ResourceAvailabilityViewModel>> UpdateAvailabilityDataForResourcesWithNoAvailabilityRecords(string employeeCodes);
        Task<string> UpdateCostAndAvailabilityDataByScheduleId(string scheduleIds);
        Task<Guid> DeleteAnalyticsDataForDeletedAllocationByScheduleId(Guid deletedAllocationId);
        Task DeleteAnalyticsDataForDeletedAllocationByScheduleIds(string deletedAllocationIds);
        void DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(string deletedPlaceholderAllocationIds);
    }
}
