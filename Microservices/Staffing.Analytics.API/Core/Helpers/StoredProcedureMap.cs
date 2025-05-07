namespace Staffing.Analytics.API.Core.Helpers
{
    public class StoredProcedureMap
    {
        // Anlaytics Controller
        public const string UpsertResourceAvailability = "upsertResourceAvailability";
        public const string UpsertResourceAvailabilityBetweenDateRange = "upsertResourceAvailabilityBetweenDateRange";
        public const string UpdateAvailabilityForResourcesWithNoAvailabilityRecords = "updateAvailabilityForResourcesWithNoAvailabilityRecords";
        public const string UpdateAvailabilityForResourcesWithNoAvailabilityRecordsBetweenDateRange = "updateAvailabilityForResourcesWithNoAvailabilityRecordsBetweendateRange";
        public const string UpdateCostAndAvailabilityDataByScheduleId = "updateCostAndAvailabilityDataByScheduleId";

        public const string GetResourcesWithNoAvailabilityRecords = "getResourcesWithNoAvailabilityRecords";
        public const string GetResourcesWithNoAvailabilityRecordsBetweenDateRange = "getResourcesWithNoAvailabilityRecordsBetweenDateRange";  //Using for 2019 data population. Can be re-used in future for any data populationbetween date ranges
        public const string GetECodesWithPartialAvailabilityOnDate = "getECodesWithPartialAvailabilityOnDate";
        public const string GetResourcesFullAvailabilityDateRange = "getResourcesFullAvailabiltyDateRange";
        public const string UpsertAnalyticsReportData = "upsertScheduleMasterDetail";
        public const string UpsertScheduleMasterPlaceholderDetail = "upsertScheduleMasterPlaceholderDetail";
        public const string DeleteAnalyticsDataByScheduleId = "deleteAnalyticsDataByScheduleId";
        public const string DeleteAnalyticsDataByScheduleIds = "deleteAnalyticsDataByScheduleIds";
        public const string DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds = "deletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds";        
        public const string UpdateCostForResourcesAvailableInFullCapacity = "UpdateCostForResourcesAvailableInFullCapacity";

        public const string InsertAvailabilityTillNextYear = "insertAvailabilityTillNextYear";
        public const string GetResourcesAllocationAndAvailability = "getResourcesAllocationAndAvailablity";
        public const string updateAnalyticsDataForCommitments = "updateAnalyticsDataForCommitments";

        public const string GetHolidayWithinDateRangeByEmployees = "[basis].[getHolidayWithinDateRangeByEmployees]";

        public const string UpsertCapacityAnalysisDaily = "upsertCapacityAnalysisDaily";
        public const string UpsertCapacityAnalysisMonthly = "upsertCapacityAnalysisMonthly";
        public const string UpdateCapacityAnalysisDailyForChangeInCaseAttribute = "updateCapacityAnalysisDailyForChangeInCaseAttribute";

        public const string GetExternalCommitmentsMinStartMaxEndDate = "getExternalCommitmentsMinStartMaxEndDate";
        public const string GetScheduleIdsIncorrectlyProcessedForAnalytics = "getScheduleIdsIncorrectlyProcessedForAnalytics";
        public const string GetPlaceholderScheduleIdsIncorrectlyProcessedForAnalytics = "getPlaceholderScheduleIdsIncorrectlyProcessedForAnalytics";

        //Case Planning Metrics Controller
        public const string GetAvailabilityMetricsByFilterValues = "getAvailabilityMetricsByFilterValues";
        public const string GetAvailabilityMetricsForPlaygroundById = "getAvailabilityMetricsForPlaygroundById";
        public const string UpsertCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations = "upsertCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations";
        public const string DeleteCasePlanningBoardMetricsPlaygroundById = "deleteCasePlanningBoardMetricsPlaygroundById";
        public const string GetCasePlanningBoardPlaygroundFiltersByPlaygroundId = "getCasePlanningBoardPlaygroundFiltersByPlaygroundId";

        // PollMaster
        public const string GetLastPolledTimeStamp = "getLastPolledTimeStamp";
        public const string UpsertPollMaster = "upsertPollMaster";

        // CasePlanningPlayground
        public const string CreateCasePlanningBoardMetricsPlayground = "createCasePlanningBoardMetricsPlayground";

    }
}
