using BackgroundPolling.API.Models;
using BackgroundPolling.API.Models.Workday;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IWorkdayPollingRepository
    {
        Task<IEnumerable<ScheduleMasterDetail>> GetRecordsWithoutCostForPlaceholders(); 
        Task<IList<string>> UpdateAnalyticsDataHavingIncorrectWorkdayInfo();
        Task<IList<string>> UpdateAnalyticsDataHavingIncorrectCaseInfo();
        Task<IList<string>> UpdateAnalyticsPlaceholderDataHavingIncorrectWorkdayInfo();
        Task<IList<string>> UpdateAvailabilityDataHavingIncorrectWorkdayInfo();
        Task<IList<string>> UpdateAvailabilityDataForExternalCommitmentsAndRingfence();
        Task<IList<string>> UpdateAvailabilityDataForMissingOrIrrelevantEntries();
        Task UpdateRecordsWithoutCostForPlaceholders(DataTable scheduleMasterDetailUpdateTable);

        Task UpdateRecordsWithoutCostForAllocations(DataTable scheduleMasterDetailUpdateTable);
        Task UpdateAnalyticsDataForLoAUpdatedRecently(DataTable scheduleMasterDetailUpdateTable);
        Task UpdateAnalyitcsDataForPendingTransactions(DataTable resourceTransactionTable);
        Task<IEnumerable<string>> GetECodesWithoutServiceLine();
        Task UpdateRecordsWithoutServiceLine(DataTable scheduleMasterTable);
        Task UpdateAnalyticsRecordsWithoutServiceLine(DataTable scheduleMasterTable);
        Task UpdateOverrideFlagForStaffingCommtimentsFromSource(DataTable commitmentMasterTable);
        Task<string> DeleteCommitmentDataForTermedEmployeesAfterTerminationDate(DataTable resourceTrasactionTable);
        Task<string> DeleteStaffingDataForTermedEmployeesAfterTerminationDate(DataTable resourceTrasactionTable);
        Task DeleteAnalyticsDataForTermedEmployeesAfterTerminationDate(DataTable resourceTrasactionTable);
        Task CreateShortTermCommitmentsForWorkdayLOAsAndTransitions(DataTable shortTermCommitmentDataTable);
        Task<IEnumerable<ServiceLine>> SaveWorkdayServiceLineListForTableau(DataTable serviceLineDataTable);
        Task<IEnumerable<PDGrade>> SaveWorkdayPDGradeListForTableau(DataTable pdGradeDataTable);
        Task<IEnumerable<Resource>> SaveWorkdayEmployeeDataForTableau(DataTable resourcesDataTable);
        
        Task UpsertWorkdayEmployeeStaffingTransactionToDB(DataTable workdayEmployeeTransactions);
        Task UpsertWorkdayEmployeeLoATransactionToDB(DataTable workdayEmployeeLoATransactions);
        Task<IEnumerable<ResourceTimeOff>> SaveWorkdayTimeOffsForTableau(DataTable workdayEmployeesTimeOffs);
        Task<IEnumerable<string>> DeleteAvailabilityDataForRescindedEmployees(string employeeCodes);
        Task<IEnumerable<StaffableAs>> GetAllActiveStaffableAsRoles();

        Task UpdateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations();
        Task UpsertWorkdayEmployeesCertificatesToDB(DataTable workdayEmployeesCertifications);
        Task UpsertWorkdayEmployeesLanguagesToDB(DataTable workdayEmployeesLanguages);
        Task<IEnumerable<ScheduleMasterDetail>> GetRecordsWithoutCostForAllocations();
    }
}
