namespace BackgroundPolling.API.Core.Helpers
{
    public static class StoredProcedureMap
    {
        //CCM Polling Controller
        public const string UpsertCaseMasterAndCaseMasterHistoryFromCCM = "[basis].[upsertCaseMasterAndCaseMasterHistoryFromCCM]";
        public const string UpsertCaseAdditionalInfo = "[basis].[upsertCaseAdditionalInfo]";
        public const string UpsertOfficeListForTableau = "[ccm].[upsertOfficeListForTableau]";
        public const string GetCurrencyRatesUpdatedRecently = "[basis].[GetCurrencyRatesUpdatedRecently]";
        public const string UpdateUSDCostForChangeInCurrencyRate = "[updateUSDCostForChangeInCurrencyRate]";

        //Basis Currency Controller
        public const string UpsertCurrencyRate = "[basis].[upsertCurrencyRate]";
        public const string UpsertCaseAttribute = "[basis].[upsertCaseAttribute]";
        // Notification Controller
        public const string InsertNotifications = "insertNotifications";
        public const string GetEmployeesRequiresBackFillBySpecificDate = "getEmployeesRequiresBackFillBySpecificDate";

        //CCM Polling Controller
        public const string GetOpportunitiesNotConvertedToCase = "getOpportunitiesNotConvertedToCase";
        public const string GetOpportunitiesPinnedByUsers = "getOpportunitiesPinnedByUsers";
        public const string UpdateSkuTermsForOpportunitiesConvertedToCase = "updateSkuTermsForOpportuntiesConvertedToCase";
        public const string UpdateUserPreferencesForOpportunitiesConvertedToCase = "updateUserPreferencesForOpportunitiesConvertedToCase";
        public const string UpdateRingfenceForOpportuntiesConvertedToCase = "updateRingfenceForOpportuntiesConvertedToCase";

        // Workday Polling Controller
        public const string GetRecordsWithoutCostForAllocations= "getRecordsWithoutCostForAllocations";
        public const string GetRecordsWithoutCostForPlaceholders = "getRecordsWithoutCostForPlaceholders";
        public const string UpdateAnalyticsDataHavingIncorrectWorkdayInfo = "updateAnalyticsDataHavingIncorrectWorkdayInfo";
        public const string UpdateAnalyticsDataHavingIncorrectCaseInfo = "updateAnalyticsDataHavingIncorrectCaseInfo";
        public const string UpdateAnalyticsPlaceholdersDataHavingIncorrectWorkdayInfo = "updateAnalyticsPlaceholdersDataHavingIncorrectWorkdayInfo";
        public const string UpdateAvailabilityDataHavingIncorrectWorkdayInfo = "updateAvailabilityDataHavingIncorrectWorkdayInfo";
        public const string UpdateAvailabilityDataForExternalCommitmentsAndRingfence = "updateAvailabilityDataForExternalCommitmentsAndRingfence";
        public const string UpdateAvailabilityDataForMissingOrIrrelevantEntries = "updateAvailabilityDataForMissingOrIrrelevantEntries";
        public const string UpdateCostInAnalyticsTableForAllocations = "updateCostInAnalyticsTableForAllocations";
        public const string UpdateCostInAnalyticsTableForPlaceholders = "updateCostInAnalyticsTableForPlaceholders";
        public const string UpdateAnalyticsDataForLoAUpdatedRecently = "updateAnalyticsDataForLoAUpdatedRecently";
        public const string UpdateAnalyticsDataForPendingTransactions = "updateAnalyticsDataForPendingTransactions";
        public const string GetECodesWithoutServiceLine = "getECodesWithoutServiceLine";
        public const string UpdateRecordsWithoutServiceLine = "updateRecordsWithoutServiceLine";
        public const string UpdateAnalyticsRecordsWithoutServiceLine = "updateAnalyticsRecordsWithoutServiceLine";
        public const string UpdateOverrideFlagForStaffingCommtimentsFromSource = "updateOverrideFlagForStaffingCommtimentsFromSource";
        public const string DeleteCommitmentDataForTermedEmployeesAfterTerminationDate = "deleteCommitmentDataForTermedEmployeesAfterTerminationDate";
        public const string DeleteStaffingDataForTermedEmployeesAfterTerminationDate = "deleteStaffingDataForTermedEmployeesAfterTerminationDate";
        public const string InsertShortTermCommitmentsForWorkdayLOAsAndTransitions = "insertShortTermCommitmentsForWorkdayLOAsAndTransitions";
        public const string InsertOfficeFlatListForTableau = "[workday].[insertOfficeFlatListForTableau]";
        public const string InsertServiceLineListForTableau = "[workday].[insertServiceLineListForTableau]";
        public const string InsertPDGradeListForTableau = "[workday].[insertPdGradeListForTableau]";
        public const string UpsertWorkdayEmployeeListForTableau = "[workday].[upsertWorkdayEmployeeListForTableau]";
        public const string UpsertWorkdayEmployeeStaffingTransaction = "[workday].upsertWorkdayEmployeeStaffingTransaction";
        public const string UpsertWorkdayEmployeeLoATransaction = "[workday].upsertWorkdayEmployeeLoATransaction";
        public const string InsertTimeOffsForTableau = "[workday].[insertTimeOffsForTableau]";
        public const string DeleteAvailabilityDataForRescindedEmployees = "deleteAvailabilityDataForRescindedEmployees";
        public const string GetAllActiveStaffableAsRoles = "getAllActiveStaffableAsRoles";
        public const string UpdateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations = "updateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations";
        public const string UpsertWorkdayEmployeesCertifications = "[workday].upsertWorkdayEmployeesCertifications";
        public const string UpsertWorkdayEmployeesLanguages = "[workday].upsertWorkdayEmployeesLanguages";

        // Finance Data Polling Controller
        public const string GetOfficeList = "getOfficeList";
        public const string GetLastUpdatedBillRateDate = "[ccm].[getLastUpdatedBillRateDate]";
        public const string GetLastUpdatedRevenueTransactionDate = "[ccm].[getLastUpdatedRevenueTransactionDate]";
        public const string UpsertBillRates = "[ccm].[upsertBillRates]";
        public const string UpsertRevenueTransactions = "[ccm].[upsertRevenueTransactions]";
        public const string DeleteRevenueTransactionsById = "[ccm].[deleteRevenueTransactionsById]";
        public const string UpdateCostForUpdatedBillRate = "updateCostForUpdatedBillRate";

        // Case Roll
        //public const string UpdateAllocationsOnCCMChange = "updateAllocationsOnCCMChange";
        //public const string UpdateCaseRollOnCCMChange = "updateCaseRollOnCCMChange";

        // Staffing
        public const string GetPrePostCaseData = "getPrePostCaseData";
        public const string GetAllAllocationsByCases = "getAllAllocationsByCases";
        public const string DeleteSecurityUsersWithExpiredEndDate = "deleteSecurityUsersWithExpiredEndDate";
        public const string DeleteAnalyticsLog = "deleteAnalyticsLog";

        // PollMaster
        public const string GetLastPolledTimeStamp = "getLastPolledTimeStamp";
        public const string UpsertPollMaster = "upsertPollMaster";

        // Vacation Polling
        public const string UpsertVacations = "[vacation].[upsertVacationRequestMaster]";

        // Training Polling
        public const string upsertTrainings = "[bvu].[upsertTrainings]";

        // Holiday Polling
        public const string InsertOfficeHolidays = "[basis].[insertOfficeHolidays]";

        //IRIS Polling
        public const string UpsertWorkAndSchoolHistory = "[iris].[upsertWorkAndSchoolHistory]";

        // Polaris Polling
        public const string UpsertSecurityUsers = "[polaris].[upsertSecurityUsers]";

        // AD Polling
        public const string UpsertSecurityUsersDataFromPolaris = "upsertSecurityUsersDataFromPolaris";

        //Basis Polling Controller
        public const string UpsertPracticeAffiliation = "[basis].[upsertPracticeAffiliation]";
        public const string InsertMonthlySnapshotForPracticeAffiliations = "[insertMonthlySnapshotForPracticeAffiliations]";
        public const string InsertPracticeAreaLookUpData = "[basis].[insertPracticeAreaLookUpData]";
        // EmailUtility Controller
        public const string GetAuditLogsForSelectedUserAndDate = "getAuditLogsForSelectedUserAndDate";
        public const string GetCasesServedByRingfenceByOfficeAndCaseType = "getCasesServedByRingfenceByOfficeAndCaseType";

        //Sharepoint Polling Controller
        public const string UpsertSMAPMissions = "upsertSMAPMissions";
        public const string UpsertStaffingPreferencesFromSharepoint = "upsertStaffingPreferencesFromSharepoint";
        

        //Pipeline Polling Controller
        public const string UpsertOpportunitiesFlatData =  "[pipeline].[upsertOpportunities]";
        public const string UpsertOpportunitiesFlatDataInPipeline = "[staffing].[upsertOpportunities]";

        //AnalyticsAudit Polling Controller
        public const string GetAnalyticsRecordsNotSyncedWithCAD = "getAnalyticsRecordsNotSyncedWithCAD";

        //AzureSearch Polling Controller
        public const string UpsertEmployeeConsildatedDataForSearch = "[azureSearch].[upsertEmployeeConsolidatedData]";
    }
}
