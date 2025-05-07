using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.Models.Workday;
using BackgroundPolling.API.ViewModels;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IWorkdayPollingService
    {

        [SkipWhenPreviousJobIsRunning]
        Task<IEnumerable<AnalyticsResourceTransactionViewModel>> UpdateAnalyitcsDataForPendingTransactions(DateTime? lastModifiedDateOnOrAfter);

        [SkipWhenPreviousJobIsRunning]
        Task UpdateCostForAllocationsAnalyticsData();

        [SkipWhenPreviousJobIsRunning]
        Task UpdateCostForPlaceholdersAnalyticsData();

        [SkipWhenPreviousJobIsRunning]
        Task<IEnumerable<LOATransaction>> UpdateAnalyticsDataForLoAUpdatedRecently(DateTime? lastModifiedDateOnOrAfter, DateTime? fromEffectiveDate);

        [SkipWhenPreviousJobIsRunning]
        Task InsertDailyAvailabilityTillNextYearForAll();

        [SkipWhenPreviousJobIsRunning]
        Task<List<ScheduleMaster>> UpdateServiceLineInScheduleMaster();

        [SkipWhenPreviousJobIsRunning]
        void UpdateCostForResourcesAvailableInFullCapacity(string employeeCodes);

        [SkipWhenPreviousJobIsRunning]
        Task<string> DeleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate();

        [DisableConcurrentExecution(60)]
        Task<IEnumerable<Commitment>> SaveWorkdayLoaAndTransitionAsShortTermCommitment();

        [SkipWhenPreviousJobIsRunning]
        Task<IEnumerable<ServiceLine>> SaveWorkdayServiceLineListForTableau();

        [SkipWhenPreviousJobIsRunning]
        Task<IEnumerable<PDGrade>> SaveWorkdayPDGradeListForTableau();

        [SkipWhenPreviousJobIsRunning]
        Task<string> SaveWorkdayTimeOffsForTableau();

        [SkipWhenPreviousJobIsRunning]
        Task<IEnumerable<ResourceLOA>> UpdateOverrideFlagForStaffingCommtimentsFromSource();

        [SkipWhenPreviousJobIsRunning]
        Task UpdateAnalyticsDataHavingIncorrectWorkdayInfo();

        [SkipWhenPreviousJobIsRunning]
        Task UpdateAnalyticsDataHavingIncorrectCaseInfo();

        [SkipWhenPreviousJobIsRunning]
        Task UpdateAnalyticsPlaceholdersDataHavingIncorrectWorkdayInfo();

        [SkipWhenPreviousJobIsRunning]
        Task UpdateAvailabilityDataHavingIncorrectWorkdayInfo();

        [SkipWhenPreviousJobIsRunning]
        Task UpdateAvailabilityDataForExternalCommitmentsAndRingfence();

        [SkipWhenPreviousJobIsRunning]
        Task UpdateAvailabilityDataForMissingOrIrrelevantEntries();

        [SkipWhenPreviousJobIsRunning]
        Task<string> UpsertWorkdayEmployeeStaffingTransactionToDB();

        [SkipWhenPreviousJobIsRunning]
        Task<string> UpsertWorkdayEmployeeLoATransactionToDB();

        [SkipWhenPreviousJobIsRunning]
        Task<string> SaveWorkdayEmployeeDataForTableau();
        [SkipWhenPreviousJobIsRunning]
        Task<string> DeleteAvailabilityDataForRescindedEmployees(string employeeCodes);
        [SkipWhenPreviousJobIsRunning]
        Task<string> ArchiveStaffableAsRoleForPromotedEmployees();

        [SkipWhenPreviousJobIsRunning]
        Task UpdateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations();

        [SkipWhenPreviousJobIsRunning]
        Task<string> SaveWorkdayEmployeesCertificationsToDB();

        [SkipWhenPreviousJobIsRunning]
        Task<string> SaveWorkdayEmployeesLanguagesToDB();
    }
}