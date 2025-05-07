using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Extensions
{
    public static class AppExtensions
    {
        public delegate Task HangfireJobDelegate<TService>(TService service);

        public static IApplicationBuilder ConfigureHangfireMiddleware(this IApplicationBuilder app)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            #region Hangfire

            app.UseHangfireServer();
            
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")))
            {
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
                    PrefixPath = "/backgroundPollingApi"
                });
            }
            else
            {
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
                });
            }

            #region Cleanup Hangfire Jobs

            // Cleanup existing recurring jobs

            RecurringJob.RemoveIfExists("caseEndingNotification");
            RecurringJob.RemoveIfExists("backFillNotification");
            RecurringJob.RemoveIfExists("OpportunityCaseConversion");
            RecurringJob.RemoveIfExists("costUpdateForPendingTransaction");
            RecurringJob.RemoveIfExists("costForAnalytics");
            RecurringJob.RemoveIfExists("analyticsDataUpdateForLoAUpdatedRecently");
            RecurringJob.RemoveIfExists("analyticsDataUpdateForStaffingUpdatedInWorkday");
            RecurringJob.RemoveIfExists("upsertBillRates");
            RecurringJob.RemoveIfExists("updateCostForResourcesAvailableInFullCapacity");
            RecurringJob.RemoveIfExists("updateServiceLineInScheduleMaster");
            RecurringJob.RemoveIfExists("updateOverrideFlagForStaffingCommtimentsFromSource");
            RecurringJob.RemoveIfExists("deleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate");
            RecurringJob.RemoveIfExists("saveWorkdayLoaAndTransitionAsShortTermCommitment");
            RecurringJob.RemoveIfExists("saveOfficeFlatListForTableau");
            RecurringJob.RemoveIfExists("saveServiceLineListForTableau");
            RecurringJob.RemoveIfExists("savePDGradeListForTableau");
            RecurringJob.RemoveIfExists("saveTimeOffsForTableau");
            RecurringJob.RemoveIfExists("saveWorkdayEmployeesCertificationsToDB");
            RecurringJob.RemoveIfExists("saveWorkdayEmployeesLanguagesToDB");
            RecurringJob.RemoveIfExists("saveWorkdayEmployeeListForTableau");
            RecurringJob.RemoveIfExists("updatePrePostAllocationsFromCCM");
            RecurringJob.RemoveIfExists("updateCaseRollAllocationsFromCCM");
            RecurringJob.RemoveIfExists("updateCaseEndDateFromCCMInCasePlannigBoard");
            RecurringJob.RemoveIfExists("updateOpportunityEndDateFromPipeline");
            RecurringJob.RemoveIfExists("updateCaseRollAllocationsNotUpdatedFromCCM");
            RecurringJob.RemoveIfExists("updateAllocationsForPreponedCasesFromCCM");
            RecurringJob.RemoveIfExists("upsertCaseMasterAndCaseMasterHistoryFromCCM");
            RecurringJob.RemoveIfExists("upsertVRSVacations");
            RecurringJob.RemoveIfExists("upsertBVUTrainings");
            RecurringJob.RemoveIfExists("upsertBasisHolidays");
            RecurringJob.RemoveIfExists("updateAnalyticsDataHavingIncorrectWorkdayInfo");
            RecurringJob.RemoveIfExists("updateAnalyticsDataHavingIncorrectCaseInfo");
            RecurringJob.RemoveIfExists("updateAvailabilityDataHavingIncorrectWorkdayInfo");
            RecurringJob.RemoveIfExists("updateAvailabilityDataForExternalCommitmentsAndRingfence");
            RecurringJob.RemoveIfExists("updateAvailabilityDataForMissingOrIrrelevantEntries");
            RecurringJob.RemoveIfExists("upsertWorkdayEmployeeStaffingTransactionToDB");
            RecurringJob.RemoveIfExists("upsertWorkdayEmployeeLoATransactionToDB");
            RecurringJob.RemoveIfExists("insertDailyDataForAllAndAvalabilityDataForResourcesWithNoData");
            RecurringJob.RemoveIfExists("upsertPolarisSecurityUsers");
            RecurringJob.RemoveIfExists("upsertCaseAdditionalInfoFromCCM");
            RecurringJob.RemoveIfExists("upsertCaseAdditionalInfoFromCCMWithFullLoad");
            RecurringJob.RemoveIfExists("saveRevOfficeFlatListForTableau");
            RecurringJob.RemoveIfExists("archiveStaffableAsRoleForPromotedEmployees");
            RecurringJob.RemoveIfExists("deleteSecurityUsersWithExpiredEndDate");
            RecurringJob.RemoveIfExists("upsertSecurityUsersFromAD");
            RecurringJob.RemoveIfExists("upsertCurrencyRate");
            RecurringJob.RemoveIfExists("upsertCaseAttribute");
            RecurringJob.RemoveIfExists("upsertPracticeAffiliations");
            RecurringJob.RemoveIfExists("insertMonthlySnapshotForPracticeAffiliations");
            RecurringJob.RemoveIfExists("updateUSDCostForChangeInCurrencyRate");
            RecurringJob.RemoveIfExists("deleteAnalyticsLog");
            RecurringJob.RemoveIfExists("sendMonthlyStaffingAllocationsEmailToExperts");
            RecurringJob.RemoveIfExists("deleteAvailabilityDataForRescindedEmployees");
            RecurringJob.RemoveIfExists("SendEmailForAuditsOfAllocationByStaffingUser");
            RecurringJob.RemoveIfExists("SendEmailsForCasesServedByRingfence");
            RecurringJob.RemoveIfExists("correctAllocationsNotConvertedToPrePostAfterCaseRollProcessed");
            RecurringJob.RemoveIfExists("upsertSignedOffSMAPMissions");
            RecurringJob.RemoveIfExists("upsertOpportunitiesFlatDataFromPipeline");
            RecurringJob.RemoveIfExists("SendEmailsForCasesServedByRingfence");
            RecurringJob.RemoveIfExists("sendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign");
            RecurringJob.RemoveIfExists("upsertEmployeeConsildatedDataForSearch");
            RecurringJob.RemoveIfExists("UpsertWorkAndSchoolHistoryFromIrisFullLoad");
            RecurringJob.RemoveIfExists("UpsertWorkAndSchoolHistoryFromIrisIncrementally");
            RecurringJob.RemoveIfExists("upsertStaffingPreferences");
            RecurringJob.RemoveIfExists("insertPracticeAreaLookUpData"); 
            RecurringJob.RemoveIfExists("upsertEmployeeConsildatedDataForSearchIncrementally");
            RecurringJob.RemoveIfExists("SendDeveloperEmailForAnalyticsRecordsNotSyncedWithCAD");
            RecurringJob.RemoveIfExists("updateSecurityUserForWFPRole");

            if (Environments.Production == environment)
            {
                RecurringJob.AddOrUpdate<INotificationService>("caseEndingNotification",
                    service => service.InsertCasesEndingNotification(), Cron.Hourly);
                RecurringJob.AddOrUpdate<INotificationService>("backFillNotification",
                    service => service.InsertBackFillNotification(), Cron.Hourly);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("costUpdateForPendingTransaction",
                     service => service.UpdateAnalyitcsDataForPendingTransactions(null), Cron.Hourly);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("costForAllocationsAnalytics",
                    service => service.UpdateCostForAllocationsAnalyticsData(), "13,28,43,58 * * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("costForPlaceholdersAnalytics",
                    service => service.UpdateCostForPlaceholdersAnalyticsData(), "13,28,43,58 * * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("analyticsDataUpdateForLoAUpdatedRecently",
                    service => service.UpdateAnalyticsDataForLoAUpdatedRecently(null, null), "0 7 * * *");
                RecurringJob.AddOrUpdate<IFinanceDataPollingService>("upsertBillRates",
                    service => service.UpsertBillRates(), "0 2,14 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateCostForResourcesAvailableInFullCapacity",
                    service => service.UpdateCostForResourcesAvailableInFullCapacity(null), "30 */4 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateServiceLineInScheduleMaster",
                    service => service.UpdateServiceLineInScheduleMaster(), "*/7 * * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateOverrideFlagForStaffingCommtimentsFromSource",
                    service => service.UpdateOverrideFlagForStaffingCommtimentsFromSource(), Cron.Hourly);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("deleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate",
                    service => service.DeleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate(), "0 2,14 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveWorkdayLoaAndTransitionAsShortTermCommitment",
                    service => service.SaveWorkdayLoaAndTransitionAsShortTermCommitment(), Cron.Hourly);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveServiceLineListForTableau",
                    service => service.SaveWorkdayServiceLineListForTableau(), "0 2,14 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("savePDGradeListForTableau",
                    service => service.SaveWorkdayPDGradeListForTableau(), "0 2,14 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveWorkdayEmployeeListForTableau",
                    service => service.SaveWorkdayEmployeeDataForTableau(), Cron.Hourly);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveTimeOffsForTableau",
                    service => service.SaveWorkdayTimeOffsForTableau(), "0 */4 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveWorkdayEmployeesCertificationsToDB",
                    service => service.SaveWorkdayEmployeesCertificationsToDB(), "0 */4 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveWorkdayEmployeesLanguagesToDB",
                    service => service.SaveWorkdayEmployeesLanguagesToDB(), "0 */4 * * *");
                RecurringJob.AddOrUpdate<ICCMPollingService>("updatePrePostAllocationsFromCCM",
                    service => service.UpdatePrePostAllocationsForEndDateChangeInCCM(), Cron.Hourly);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateCaseRollAllocationsFromCCM",
                    service => service.UpdateCaseRollAllocationsFromCCM(), Cron.Hourly);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateCaseEndDateFromCCMInCasePlannigBoard",
                    service => service.UpdateCaseEndDateFromCCMInCasePlanningBoard(null), Cron.Hourly);
                RecurringJob.AddOrUpdate<IPipelinePollingService>("updateOpportunityEndDateFromPipeline",
                    service => service.UpdateOpportunityEndDateFromPipeline(), Cron.Hourly);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateCaseRollAllocationsNotUpdatedFromCCM",
                    service => service.UpdateCaseRollAllocationsNotUpdatedFromCCM(), "0 2,14 * * *");
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateAllocationsForPreponedCasesFromCCM",
                    service => service.UpdateAllocationsForPreponedCasesFromCCM(), "0 11,14 * * *");
                RecurringJob.AddOrUpdate<ICCMPollingService>("correctAllocationsNotConvertedToPrePostAfterCaseRollProcessed",
                    service => service.CorrectAllocationsNotConvertedToPrePostAfterCaseRollProcessed(), Cron.Hourly);
                RecurringJob.AddOrUpdate<ICCMPollingService>("OpportunityCaseConversion",
                    service => service.ConvertOpportunityToCase(), "*/15 * * * *"); //every 15 mins
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCaseMasterAndCaseMasterHistoryFromCCM",
                    service => service.UpsertCaseMasterAndCaseMasterHistoryFromCCM(), "*/15 * * * *"); //every 15 mins
                RecurringJob.AddOrUpdate<IVacationPollingService>("upsertVRSVacations",
                    service => service.upsertVacations(), Cron.Hourly);
                RecurringJob.AddOrUpdate<ITrainingPollingService>("upsertBVUTrainings",
                    service => service.upsertTrainings(), Cron.Hourly);
                RecurringJob.AddOrUpdate<IHolidayPollingService>("upsertBasisHolidays",
                    service => service.InsertHolidays(), Cron.Hourly);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAnalyticsDataHavingIncorrectWorkdayInfo",
                    service => service.UpdateAnalyticsDataHavingIncorrectWorkdayInfo(), "0 */4 * * *"); //running this job after upsertWorkdayEmployeeStaffingTransactionToDB so that incorrect data can be corrected after that
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAnalyticsDataHavingIncorrectCaseInfo",
                   service => service.UpdateAnalyticsDataHavingIncorrectCaseInfo(), "0 */12 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAnalyticsPlaceholderDataHavingIncorrectWorkdayInfo",
                  service => service.UpdateAnalyticsPlaceholdersDataHavingIncorrectWorkdayInfo(), "0 */4 * * *"); //running this job after upsertWorkdayEmployeeStaffingTransactionToDB so that incorrect data can be corrected after that
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAvailabilityDataHavingIncorrectWorkdayInfo",
                   service => service.UpdateAvailabilityDataHavingIncorrectWorkdayInfo(), "45 */4 * * *"); //running this job after upsertWorkdayEmployeeStaffingTransactionToDB so that incorrect data can be corrected after that
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAvailabilityDataForExternalCommitmentsAndRingfence",
                    service => service.UpdateAvailabilityDataForExternalCommitmentsAndRingfence(), "14 */4 * * *"); //running this job after upsertWorkdayEmployeeStaffingTransactionToDB so that incorrect data can be corrected after that
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAvailabilityDataForMissingOrIrrelevantEntries",
                    service => service.UpdateAvailabilityDataForMissingOrIrrelevantEntries(), "0 */4 * * *"); //running this job after upsertWorkdayEmployeeStaffingTransactionToDB so that incorrect data can be corrected after that
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("upsertWorkdayEmployeeStaffingTransactionToDB",
                    service => service.UpsertWorkdayEmployeeStaffingTransactionToDB(), "0 4,16 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("upsertWorkdayEmployeeLoATransactionToDB",
                    service => service.UpsertWorkdayEmployeeLoATransactionToDB(), "0 4,16 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("insertDailyDataForAllAndAvalabilityDataForResourcesWithNoData",
                    service => service.InsertDailyAvailabilityTillNextYearForAll(), "0 2 * * *");//once every day
                RecurringJob.AddOrUpdate<IPolarisPollingService>("upsertPolarisSecurityUsers",
                    service => service.UpsertSecurityUsers(), Cron.Hourly);
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCaseAdditionalInfoFromCCM",
                    service => service.UpsertCaseAdditionalInfoFromCCM(false, null), ConfigurationUtility.GetValue("HangfireJobSchedule:upsertCaseAdditionalInfo"), new RecurringJobOptions  { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }); // incremntal updates every 15 min after minute 10 so that tableau reports can have latest data
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCaseAdditionalInfoFromCCMWithFullLoad",
                    service => service.UpsertCaseAdditionalInfoFromCCM(true, null), ConfigurationUtility.GetValue("HangfireJobSchedule:upsertCaseAdditionalInfoFromCCMWithFullLoad"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") });  // full refresh daily after minute 10 so that tableau reports can have latest data
                RecurringJob.AddOrUpdate<IFinanceDataPollingService>("saveRevOfficeFlatListForTableau",
                    service => service.SaveOfficeListForTableau(), "0 2,14 * * *");
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("archiveStaffableAsRoleForPromotedEmployees",
                    service => service.ArchiveStaffableAsRoleForPromotedEmployees(), "0 1,13 * * *");
                RecurringJob.AddOrUpdate<IStaffingPollingService>("deleteSecurityUsersWithExpiredEndDate",
                    service => service.DeleteSecurityUsersWithExpiredEndDate(), Cron.Daily);
                RecurringJob.AddOrUpdate<IStaffingPollingService>("updateSecurityUserForWFPRole",
                  service => service.UpdateSecurityUserForWFPRole(), Cron.Daily);
                //RecurringJob.AddOrUpdate<IADSecurityUserService>("upsertSecurityUsersFromAD",
                //    service => service.UpsertADGroupMembersInDB(string.Empty, true), Cron.Hourly);
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCurrencyRate",
                    service => service.UpsertCurrencyRates(null), Cron.Hourly);
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCaseAttribute",
                    service => service.UpsertCaseAttributes(null), Cron.Hourly);
                RecurringJob.AddOrUpdate<IBasisPollingService>("upsertPracticeAffiliations",
                    service => service.UpsertPracticeAffiliations(), "0 0 * * *");
                RecurringJob.AddOrUpdate<IBasisPollingService>("insertMonthlySnapshotForPracticeAffiliations",
                    service => service.InsertMonthlySnapshotForPracticeAffiliations(), "0 2 * * *");
                RecurringJob.AddOrUpdate<IFinanceDataPollingService>("upsertRevenueTransactions",
                    service => service.UpsertRevenueTransactions(), Cron.Daily);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateUSDCostForChangeInCurrencyRate",
                    service => service.UpdateUSDCostForCurrencyRateChangedRecently(null), Cron.Daily);
                RecurringJob.AddOrUpdate<IStaffingPollingService>("deleteAnalyticsLog",
                    service => service.DeleteAnalyticsLog(), Cron.Daily);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("deleteAvailabilityDataForRescindedEmployees",
                     service => service.DeleteAvailabilityDataForRescindedEmployees(null), Cron.Daily);
                RecurringJob.AddOrUpdate<ISharepointPollingService>("upsertSignedOffSMAPMissions",
                     service => service.UpsertSignedOffSMAPMissions(), "0 0 * * *");
                //RecurringJob.AddOrUpdate<ISharepointPollingService>("upsertStaffingPreferences",
                //     service => service.UpsertStaffingPreferences(), "0 0 * * *");
                RecurringJob.AddOrUpdate<IBasisPollingService>("insertPracticeAreaLookUpData",
                     service => service.InsertPracticeAreaLookUpData(), Cron.Daily);
                RecurringJob.AddOrUpdate<IPipelinePollingService>("upsertOpportunitiesFlatDataFromPipeline",
                    service => service.UpsertOpportunitiesFlatDataFromPipeline(true, null), ConfigurationUtility.GetValue("HangfireJobSchedule:upsertOpportunitiesFlatDataFromPipeline"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }); // full refresh daily after minute 10 so that tableau reports can have latest data
                RecurringJob.AddOrUpdate<IAzureSearchPollingService>("upsertEmployeeConsildatedDataForSearchFullLoad",
                    service => service.UpsertEmployeeConsildatedDataForSearch(true), ConfigurationUtility.GetValue("HangfireJobSchedule:upsertEmployeeConsildatedDataForSearchFullLoad"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }); // full refresh every 4 hours daily
                RecurringJob.AddOrUpdate<IAzureSearchPollingService>("upsertEmployeeConsildatedDataForSearchIncrementally",
                    service => service.UpsertEmployeeConsildatedDataForSearch(false), ConfigurationUtility.GetValue("HangfireJobSchedule:upsertEmployeeConsildatedDataForSearchIncrementally"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") });  // incrmenetal refresh every 15 mins
                RecurringJob.AddOrUpdate<IIrisPollingService>("UpsertWorkAndSchoolHistoryFromIrisFullLoad",
                   service => service.UpsertWorkAndSchoolHistoryForAllActiveEmployeesFromIris(), ConfigurationUtility.GetValue("HangfireJobSchedule:UpsertWorkAndSchoolHistoryFromIrisFullLoad"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }); // full refresh once a week on Sunday
            }
            else
            {
                //non-prod environements no need to run multiple times.
                const string dailyAt12AM = "0 0 * * *";
                const string dailyAt12PM = "0 12 * * *";

                RecurringJob.AddOrUpdate<INotificationService>("caseEndingNotification",
                    service => service.InsertCasesEndingNotification(), Cron.Never);
                RecurringJob.AddOrUpdate<INotificationService>("backFillNotification",
                    service => service.InsertBackFillNotification(), Cron.Never);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("costUpdateForPendingTransaction",
                     service => service.UpdateAnalyitcsDataForPendingTransactions(null), dailyAt12AM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("costForAllocationsAnalytics",
                    service => service.UpdateCostForAllocationsAnalyticsData(), dailyAt12AM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("costForPlaceholdersAnalytics",
                    service => service.UpdateCostForPlaceholdersAnalyticsData(), dailyAt12AM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("analyticsDataUpdateForLoAUpdatedRecently",
                    service => service.UpdateAnalyticsDataForLoAUpdatedRecently(null, null), Cron.Never);
                RecurringJob.AddOrUpdate<IFinanceDataPollingService>("upsertBillRates",
                    service => service.UpsertBillRates(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateCostForResourcesAvailableInFullCapacity",
                    service => service.UpdateCostForResourcesAvailableInFullCapacity(null), dailyAt12PM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateServiceLineInScheduleMaster",
                    service => service.UpdateServiceLineInScheduleMaster(), Cron.Never);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateOverrideFlagForStaffingCommtimentsFromSource",
                    service => service.UpdateOverrideFlagForStaffingCommtimentsFromSource(), dailyAt12AM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("deleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate",
                    service => service.DeleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveWorkdayLoaAndTransitionAsShortTermCommitment",
                    service => service.SaveWorkdayLoaAndTransitionAsShortTermCommitment(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveServiceLineListForTableau",
                    service => service.SaveWorkdayServiceLineListForTableau(), dailyAt12AM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("savePDGradeListForTableau",
                    service => service.SaveWorkdayPDGradeListForTableau(), dailyAt12AM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveWorkdayEmployeeListForTableau",
                    service => service.SaveWorkdayEmployeeDataForTableau(), dailyAt12AM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveTimeOffsForTableau",
                    service => service.SaveWorkdayTimeOffsForTableau(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveWorkdayEmployeesCertificationsToDB",
                    service => service.SaveWorkdayEmployeesCertificationsToDB(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("saveWorkdayEmployeesLanguagesToDB",
                    service => service.SaveWorkdayEmployeesLanguagesToDB(), dailyAt12AM);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updatePrePostAllocationsFromCCM",
                    service => service.UpdatePrePostAllocationsForEndDateChangeInCCM(), dailyAt12PM);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateCaseRollAllocationsFromCCM",
                    service => service.UpdateCaseRollAllocationsFromCCM(), dailyAt12PM);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateCaseEndDateFromCCMInCasePlannigBoard",
                    service => service.UpdateCaseEndDateFromCCMInCasePlanningBoard(null), dailyAt12PM);
                RecurringJob.AddOrUpdate<IPipelinePollingService>("updateOpportunityEndDateFromPipeline",
                    service => service.UpdateOpportunityEndDateFromPipeline(), dailyAt12AM);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateCaseRollAllocationsNotUpdatedFromCCM",
                    service => service.UpdateCaseRollAllocationsNotUpdatedFromCCM(), dailyAt12AM);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateAllocationsForPreponedCasesFromCCM",
                    service => service.UpdateAllocationsForPreponedCasesFromCCM(), dailyAt12AM);
                RecurringJob.AddOrUpdate<ICCMPollingService>("correctAllocationsNotConvertedToPrePostAfterCaseRollProcessed",
                    service => service.CorrectAllocationsNotConvertedToPrePostAfterCaseRollProcessed(), dailyAt12AM);
                RecurringJob.AddOrUpdate<ICCMPollingService>("OpportunityCaseConversion",
                    service => service.ConvertOpportunityToCase(), dailyAt12AM); //every 15 mins
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCaseMasterAndCaseMasterHistoryFromCCM",
                    service => service.UpsertCaseMasterAndCaseMasterHistoryFromCCM(), dailyAt12PM); //every 15 mins
                RecurringJob.AddOrUpdate<IVacationPollingService>("upsertVRSVacations",
                    service => service.upsertVacations(), dailyAt12AM);
                RecurringJob.AddOrUpdate<ITrainingPollingService>("upsertBVUTrainings",
                    service => service.upsertTrainings(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IHolidayPollingService>("upsertBasisHolidays",
                    service => service.InsertHolidays(), dailyAt12AM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAnalyticsDataHavingIncorrectWorkdayInfo",
                    service => service.UpdateAnalyticsDataHavingIncorrectWorkdayInfo(), Cron.Never); //running this job after upsertWorkdayEmployeeStaffingTransactionToDB so that incorrect data can be corrected after that
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAnalyticsDataHavingIncorrectCaseInfo",
                    service => service.UpdateAnalyticsDataHavingIncorrectCaseInfo(), Cron.Never); //running this job after upsertWorkdayEmployeeStaffingTransactionToDB so that incorrect data can be corrected after that
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAnalyticsPlaceholderDataHavingIncorrectWorkdayInfo",
                  service => service.UpdateAnalyticsPlaceholdersDataHavingIncorrectWorkdayInfo(), Cron.Never); //running this job after upsertWorkdayEmployeeStaffingTransactionToDB so that incorrect data can be corrected after that
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("updateAvailabilityDataHavingIncorrectWorkdayInfo",
                   service => service.UpdateAvailabilityDataHavingIncorrectWorkdayInfo(), Cron.Never); //running this job after upsertWorkdayEmployeeStaffingTransactionToDB so that incorrect data can be corrected after that
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("upsertWorkdayEmployeeStaffingTransactionToDB",
                    service => service.UpsertWorkdayEmployeeStaffingTransactionToDB(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("upsertWorkdayEmployeeLoATransactionToDB",
                    service => service.UpsertWorkdayEmployeeLoATransactionToDB(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("insertDailyDataForAllAndAvalabilityDataForResourcesWithNoData",
                    service => service.InsertDailyAvailabilityTillNextYearForAll(), dailyAt12AM);//once every day
                RecurringJob.AddOrUpdate<IPolarisPollingService>("upsertPolarisSecurityUsers",
                    service => service.UpsertSecurityUsers(), dailyAt12AM);
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCaseAdditionalInfoFromCCM",
                    service => service.UpsertCaseAdditionalInfoFromCCM(false, null), Cron.Never); // incremntal updates every 15 min after minute 10 so that tableau reports can have latest data
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCaseAdditionalInfoFromCCMWithFullLoad",
                    service => service.UpsertCaseAdditionalInfoFromCCM(true, null), dailyAt12AM);  // full refresh daily after minute 10 so that tableau reports can have latest data
                RecurringJob.AddOrUpdate<IFinanceDataPollingService>("saveRevOfficeFlatListForTableau",
                    service => service.SaveOfficeListForTableau(), dailyAt12AM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("archiveStaffableAsRoleForPromotedEmployees",
                    service => service.ArchiveStaffableAsRoleForPromotedEmployees(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IStaffingPollingService>("deleteSecurityUsersWithExpiredEndDate",
                    service => service.DeleteSecurityUsersWithExpiredEndDate(), dailyAt12AM);
                //RecurringJob.AddOrUpdate<IADSecurityUserService>("upsertSecurityUsersFromAD",
                //    service => service.UpsertADGroupMembersInDB(string.Empty, true), Cron.Hourly);
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCurrencyRate",
                    service => service.UpsertCurrencyRates(null), dailyAt12PM);
                RecurringJob.AddOrUpdate<ICCMPollingService>("upsertCaseAttribute",
                    service => service.UpsertCaseAttributes(null), dailyAt12PM);
                RecurringJob.AddOrUpdate<IBasisPollingService>("upsertPracticeAffiliations",
                    service => service.UpsertPracticeAffiliations(), dailyAt12AM);
                RecurringJob.AddOrUpdate<IBasisPollingService>("insertMonthlySnapshotForPracticeAffiliations",
                    service => service.InsertMonthlySnapshotForPracticeAffiliations(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IFinanceDataPollingService>("upsertRevenueTransactions",
                    service => service.UpsertRevenueTransactions(), Cron.Never);
                RecurringJob.AddOrUpdate<ICCMPollingService>("updateUSDCostForChangeInCurrencyRate",
                    service => service.UpdateUSDCostForCurrencyRateChangedRecently(null), Cron.Never);
                RecurringJob.AddOrUpdate<IStaffingPollingService>("deleteAnalyticsLog",
                    service => service.DeleteAnalyticsLog(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IWorkdayPollingService>("deleteAvailabilityDataForRescindedEmployees",
                     service => service.DeleteAvailabilityDataForRescindedEmployees(null), dailyAt12AM);
                RecurringJob.AddOrUpdate<ISharepointPollingService>("upsertSignedOffSMAPMissions",
                     service => service.UpsertSignedOffSMAPMissions(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IBasisPollingService>("insertPracticeAreaLookUpData",
                     service => service.InsertPracticeAreaLookUpData(), dailyAt12PM);
                RecurringJob.AddOrUpdate<IPipelinePollingService>("upsertOpportunitiesFlatDataFromPipeline",
                    service => service.UpsertOpportunitiesFlatDataFromPipeline(true, null), dailyAt12AM); // full refresh daily after minute 10 so that tableau reports can have latest data
                RecurringJob.AddOrUpdate<IAzureSearchPollingService>("upsertEmployeeConsildatedDataForSearchFullLoad",
                    service => service.UpsertEmployeeConsildatedDataForSearch(true), dailyAt12PM); // full refresh every 4 hours daily
                //RecurringJob.AddOrUpdate<IAzureSearchPollingService>("upsertEmployeeConsildatedDataForSearchIncrementally",
                //    service => service.UpsertEmployeeConsildatedDataForSearch(false), ConfigurationUtility.GetValue("HangfireJobSchedule:upsertEmployeeConsildatedDataForSearchIncrementally"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") });  // incrmenetal refresh every 15 mins
                RecurringJob.AddOrUpdate<IIrisPollingService>("UpsertWorkAndSchoolHistoryFromIrisFullLoad",
                   service => service.UpsertWorkAndSchoolHistoryForAllActiveEmployeesFromIris(), dailyAt12PM); // full refresh once a week on Sunday
                RecurringJob.AddOrUpdate<IStaffingPollingService>("updateSecurityUserForWFPRole",
                                     service => service.UpdateSecurityUserForWFPRole(), Cron.Daily);
            }
                
            if (Environments.Production == environment)
            {
                RecurringJob.AddOrUpdate<IEmailUtilityService>("sendMonthlyStaffingAllocationsEmailToExperts",
                    service => service.SendMonthlyStaffingAllocationsEmailToExperts(null), ConfigurationUtility.GetValue("HangfireJobSchedule:sendMonthlyStaffingAllocationsEmailToExperts"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }); //"0 5 15 * *" Every 15th 12 AM EST

                var effectiveDate = (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).AddMonths(-1);
                RecurringJob.AddOrUpdate<IEmailUtilityService>("SendEmailForAuditsOfAllocationByStaffingUser",
                   service => service.SendEmailForAuditsOfAllocationByStaffingUser(ConfigurationUtility.GetValue("StaffingUserECode"), effectiveDate), "00 6 20 * *", new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }); // Every month on 20

                RecurringJob.AddOrUpdate<IEmailUtilityService>("SendEmailsForCasesServedByRingfence",
                    service => service.SendEmailsForCasesServedByRingfenceByOfficeAndCaseType(null, null), "00 17 * * 2", new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }); // Every Tuesday at 5:00 PM EST

                RecurringJob.AddOrUpdate<IAnalyticsAuditService>("SendDeveloperEmailForAnalyticsRecordsNotSyncedWithCAD",
                    service => service.GetAnalyticsRecordsNotSyncedWithCAD(), ConfigurationUtility.GetValue("HangfireJobSchedule:SendDeveloperEmailForAnalyticsRecordsNotSyncedWithCAD"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }); // Every Tuesday at 5:00 PM EST

                RecurringJob.AddOrUpdate<IEmailUtilityService>("sendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign",
                    service => service.SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(null), ConfigurationUtility.GetValue("HangfireJobSchedule:sendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }); //"0 5 15 * *" Every 15th 12 AM EST
                RecurringJob.AddOrUpdate<IEmailUtilityService>("sendMonthlyStaffingAllocationsEmailToExperts",
                    service => service.SendMonthlyStaffingAllocationsEmailToExperts(null),ConfigurationUtility.GetValue("HangfireJobSchedule:sendMonthlyStaffingAllocationsEmailToExperts"), new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")});
                
            }
            #endregion

            #endregion
            return app;
        }

        public static void ConfigureRecurringJob<TService>(string jobName, HangfireJobDelegate<TService> jobMethod, string cronExpressionConfigKey = null,     TimeZoneInfo timeZone = null)
        {
            cronExpressionConfigKey ??= Cron.Never();
            timeZone ??= TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            var cronExpression = cronExpressionConfigKey;// ConfigurationUtility.GetValue(cronExpressionConfigKey);

            RecurringJob.AddOrUpdate<TService>(jobName,
                service => jobMethod(service),
                cronExpression,
                new RecurringJobOptions
                {
                    TimeZone = timeZone
                });
        }
    }
}
