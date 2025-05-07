using Hangfire;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface IAnalyticsService
    {
        [JobDisplayName("Upsert analytics data")]
        Task<IEnumerable<string>> CreateAnalyticsReport(string scheduleIds);

        [JobDisplayName("Correct analytics data")]
        Task<IEnumerable<string>> CorrectAnalyticsData();

        [JobDisplayName("Upsert capacity daily analysis data")]

        [SkipWhenPreviousJobIsRunning]
        Task UpsertCapacityAnalysisDaily(bool? fullLoad, DateTime? loadAfterLastUpdated);

        [SkipConcurrentExecution(timeoutInSeconds: 2 * 60)]
        Task UpdateCapacityAnalysisDailyForChangeInCaseAttribute(DateTime? updateAfterTimeStamp);   

        [JobDisplayName("Upsert capacity monthly analysis data")]
        [SkipWhenPreviousJobIsRunning]
        Task UpsertCapacityAnalysisMonthly(bool? fullLoad);

        //[JobDisplayName("Upsert availability data")]
        //Task UpsertAvailabilityData(string employeeCodes);

        [JobDisplayName("Upsert availability data between date range")]
        Task UpsertAvailabilityDataBetweenDateRange(IEnumerable<AvailabilityDateRange> employeeCodes);

        [JobDisplayName("Update cost for resources available in full capacity")]
        Task<IEnumerable<ResourceAvailability>> UpdateCostForResourcesAvailableInFullCapacity(string employeeCodes);

        [JobDisplayName("delete analytics data for allocation deleted from BOSS")]
        Task<Guid> DeleteAnalyticsDataForDeletedAllocationByScheduleId(Guid deletedAllocationId);

        [JobDisplayName("delete analytics data for allocations deleted from BOSS")]
        Task DeleteAnalyticsDataForDeletedAllocationByScheduleIds(string deletedAllocationIds);

        [JobDisplayName("Update analytics data for commitments upserted in BOSS")]
        Task UpdateAnlayticsDataForUpsertedCommitment(string commitmentIds);
        
        [JobDisplayName("Update analytics data for commitments upserted in external system")]
        
        [SkipWhenPreviousJobIsRunning]
        Task UpdateAnlayticsDataForUpsertedExternalCommitment(DateTime? updatedAfter);

        [JobDisplayName("Update analytics data for commitments deleted in BOSS")]
        Task UpdateAnlayticsDataForDeletedCommitment(string commitmentIds);
        
        [JobDisplayName("Insert daily availabiliy record for all active employees and insert availability data for employees having no records")]
        Task<List<ResourceAvailabilityViewModel>> InsertDailyAvailabilityTillNextYearForAll(string employeeCodes);
        
        [JobDisplayName("Update employee availability having no records")]
        Task<List<ResourceAvailabilityViewModel>> UpdateAvailabilityDataForResourcesWithNoAvailabilityRecords(string employeeCodes);

        [JobDisplayName("Update data in Schedulemasterdetail & Resource availability table by scheduleId")]
        Task<string> UpdateCostAndAvailabilityDataByScheduleId(string scheduleIds);

        //used in Placeholder and Resource Allocation detail
        Task<List<ResourceAllocation>> UpdateCommitmentsInAllocations(List<ResourceAllocation> allocations);
        //TODO: delete after 2019 data population or keep for future
        Task<List<ResourceAvailabilityViewModel>> InsertAvailabilityDataForForResourcesWithNoDataBetweenDateRange(string employeeCodes);
        Task<AnalyticsViewModel> GetResourcesAllocationAndAvailabilityByDateRange(DateTime? startDate, DateTime? endDate, DateTime? lastUpdatedFrom, DateTime? lastUpdatedTo, string action, string sourceTable, short pageNumber, int pageSize);

        Task SplitAllocationsForPendingTransactionOrLoA(IEnumerable<ResourceAllocation> resourceAllocations, List<ResourceAllocation> allocations, string employeeCodes);

        Task SplitAllocationsForHistoricalTransactionOrLevelGradeChange(IEnumerable<ResourceAllocation> resourceAllocations, List<ResourceAllocation> allocations, string employeeCodes);

        Task<IEnumerable<ResourceAllocation>> GetResourcesAllocationsWithBillRate(IEnumerable<ResourceAllocation> resourcesAllocations);
        
        IEnumerable<AvailabilityDateRange> GetAvailabilityDateRangeToUpsertFromPreviousAndNewAllocationDates(IEnumerable<ResourceAllocation> previousAllocationDateRange, IEnumerable<ResourceAllocation> newAllocationsData);
    }

}
