using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.RepositoryInterfaces
{
    public interface IAnalyticsRepository
    {
        //Task UpsertAvailabilityData(string employeeCodes);
        Task UpsertAvailabilityDataBetweenDateRange(DataTable availabilityDateRangeForEmployeesTable);
        Task UpdateAvailabilityDataForResourcesWithNoAvailabilityRecords(DataTable resourceAvailabilityTable);
        //TODO: Delete after 2019 data population or keep for future data population between date ranges
        Task UpdateAvailabilityDataForResourcesWithNoAvailabilityRecordsBetweenDateRange(DataTable resourceAvailabilityTable);
        Task UpdateCostAndAvailabilityDataByScheduleId(string scheduleIds);
        Task<AnalyticsViewModel> GetResourcesAllocationAndAvailabilityByDateRange(DateTime? startDate, DateTime? endDate, DateTime? lastUpdatedFrom, DateTime? lastUpdatedTo, string action, string sourceTable, short pageNumber, int pageSize);
        Task UpdateAnalyticsDataForCommitments(DataTable resourcesCommitments);

        Task<IEnumerable<Holiday>> GetHolidayWithinDateRangeByEmployees(string employeeCodes, DateTime? fromDate, DateTime? toDate);

        Task UpsertCapacityAnalysisDaily(bool? fullLoad, DateTime? loadAfterLastUpdated);
        Task UpdateCapacityAnalysisDailyForChangeInCaseAttribute(DateTime? updateAfterTimeStamp);
        Task UpsertCapacityAnalysisMonthly(bool? fullLoad);

        Task<IEnumerable<CommitmentViewModel>> GetExternalCommitments(DateTime? lastUpdated);

        Task<IEnumerable<Guid>> GetScheduleIdsIncorrectlyProcessedInAnalytics();
    }
}
