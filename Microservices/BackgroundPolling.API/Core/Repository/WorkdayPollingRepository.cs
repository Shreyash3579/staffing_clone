using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class WorkdayPollingRepository : IWorkdayPollingRepository
    {
        private readonly IBaseRepository<ResourceTransition> _baseRepository;

        public WorkdayPollingRepository(IBaseRepository<ResourceTransition> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<ScheduleMasterDetail>> GetRecordsWithoutCostForAllocations()
        {
            var records = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query<ScheduleMasterDetail>(
                StoredProcedureMap.GetRecordsWithoutCostForAllocations,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return records;
        }

        public async Task<IEnumerable<ScheduleMasterDetail>> GetRecordsWithoutCostForPlaceholders()
        {
            var records = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query<ScheduleMasterDetail>(
                StoredProcedureMap.GetRecordsWithoutCostForPlaceholders,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return records;
        }


        public async Task<IList<string>> UpdateAnalyticsDataHavingIncorrectWorkdayInfo()
        {
            var recordsCorrectedForEmployees = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.QueryMultipleAsync(
                StoredProcedureMap.UpdateAnalyticsDataHavingIncorrectWorkdayInfo,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 3000));
                
            return recordsCorrectedForEmployees.Read<string>().ToList();
        }

        public async Task<IList<string>> UpdateAnalyticsDataHavingIncorrectCaseInfo()
        {
            var recordsCorrectedForEmployees = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.QueryMultipleAsync(
                StoredProcedureMap.UpdateAnalyticsDataHavingIncorrectCaseInfo,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 3000));

            return recordsCorrectedForEmployees.Read<string>().ToList();
        }

        public async Task<IList<string>> UpdateAnalyticsPlaceholderDataHavingIncorrectWorkdayInfo()
        {
            var recordsCorrectedForEmployees = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.QueryMultipleAsync(
                StoredProcedureMap.UpdateAnalyticsPlaceholdersDataHavingIncorrectWorkdayInfo,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 3000));

            return recordsCorrectedForEmployees.Read<string>().ToList();
        }


        public async Task<IList<string>> UpdateAvailabilityDataHavingIncorrectWorkdayInfo()
        {
            var recordsCorrectedForEmployees = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.QueryMultipleAsync(
                StoredProcedureMap.UpdateAvailabilityDataHavingIncorrectWorkdayInfo,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 3000));

            return recordsCorrectedForEmployees.Read<string>().ToList();
        }

        public async Task<IList<string>> UpdateAvailabilityDataForExternalCommitmentsAndRingfence()
        {
            var recordsCorrectedForEmployees = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.QueryMultipleAsync(
                StoredProcedureMap.UpdateAvailabilityDataForExternalCommitmentsAndRingfence,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 3000));

            return recordsCorrectedForEmployees.Read<string>().ToList();
        }

        public async Task<IList<string>> UpdateAvailabilityDataForMissingOrIrrelevantEntries()
        {
            var recordsCorrectedForEmployees = await Task.Run(() => _baseRepository.Context.AnalyticsConnection.QueryMultipleAsync(
                StoredProcedureMap.UpdateAvailabilityDataForMissingOrIrrelevantEntries,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 3000));

            return recordsCorrectedForEmployees.Read<string>().ToList();
        }

        public async Task UpdateRecordsWithoutCostForPlaceholders(DataTable scheduleMasterDetailUpdateTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpdateCostInAnalyticsTableForPlaceholders,
                new
                {
                    records =
                        scheduleMasterDetailUpdateTable.AsTableValuedParameter(
                            "[dbo].[analyticsRecordsTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpdateRecordsWithoutCostForAllocations(DataTable scheduleMasterDetailUpdateTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpdateCostInAnalyticsTableForAllocations,
                new
                {
                    records =
                        scheduleMasterDetailUpdateTable.AsTableValuedParameter(
                            "[dbo].[analyticsRecordsTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpdateAnalyticsDataForLoAUpdatedRecently(DataTable resourceTransactionTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpdateAnalyticsDataForLoAUpdatedRecently,
                new
                {
                    resourceTransactions =
                        resourceTransactionTable.AsTableValuedParameter(
                            "[dbo].[analyticsResourceTransactionTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpdateAnalyitcsDataForPendingTransactions(DataTable resourceTransactionTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpdateAnalyticsDataForPendingTransactions,
                new
                {
                    resourceTransactions =
                        resourceTransactionTable.AsTableValuedParameter(
                            "[dbo].[analyticsResourceTransactionTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task<IEnumerable<string>> GetECodesWithoutServiceLine()
        {
            var eCodes = await Task.Run(() => _baseRepository.Context.Connection.Query<string>(
                StoredProcedureMap.GetECodesWithoutServiceLine,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return eCodes;
        }

        public async Task UpdateRecordsWithoutServiceLine(DataTable scheduleMasterTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
                StoredProcedureMap.UpdateRecordsWithoutServiceLine,
                new
                {
                    recordsWithServiceLine =
                        scheduleMasterTable.AsTableValuedParameter(
                            "[dbo].[scheduleMasterTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpdateAnalyticsRecordsWithoutServiceLine(DataTable scheduleMasterTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpdateAnalyticsRecordsWithoutServiceLine,
                new
                {
                    recordsWithServiceLine =
                        scheduleMasterTable.AsTableValuedParameter(
                            "[dbo].[scheduleMasterTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }


        public async Task UpdateOverrideFlagForStaffingCommtimentsFromSource(DataTable commitmentMasterTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
               StoredProcedureMap.UpdateOverrideFlagForStaffingCommtimentsFromSource,
               new
               {
                   futureCommitmentsUpdatedInSource =
                       commitmentMasterTable.AsTableValuedParameter(
                           "[dbo].[commitmentMasterTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task<string> DeleteCommitmentDataForTermedEmployeesAfterTerminationDate(DataTable resourceTransactionsTable)
        {
            var affectedEmployeesList = await _baseRepository.Context.Connection.QueryAsync<string>(
               StoredProcedureMap.DeleteCommitmentDataForTermedEmployeesAfterTerminationDate,
               new
               {
                   resourceTransactions =
                       resourceTransactionsTable.AsTableValuedParameter(
                           "[dbo].[ResourceTransactionTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return string.Join(",", affectedEmployeesList);
        }

        public async Task<string> DeleteStaffingDataForTermedEmployeesAfterTerminationDate(DataTable resourceTransactionsTable)
        {
            var affectedEmployeesList = await _baseRepository.Context.Connection.QueryAsync<string>(
               StoredProcedureMap.DeleteStaffingDataForTermedEmployeesAfterTerminationDate,
               new
               {
                   resourceTransactions =
                       resourceTransactionsTable.AsTableValuedParameter(
                           "[dbo].[ResourceTransactionTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);
            
            return string.Join(",", affectedEmployeesList);
        }

        public async Task DeleteAnalyticsDataForTermedEmployeesAfterTerminationDate(DataTable resourceTransactionsTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
               StoredProcedureMap.DeleteStaffingDataForTermedEmployeesAfterTerminationDate,
               new
               {
                   resourceTransactions =
                       resourceTransactionsTable.AsTableValuedParameter(
                           "[dbo].[ResourceTransactionTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }
        

        public async Task CreateShortTermCommitmentsForWorkdayLOAsAndTransitions(DataTable shortTermCommitmentDataTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
               StoredProcedureMap.InsertShortTermCommitmentsForWorkdayLOAsAndTransitions,
               new
               {
                   shortTermCommitments =
                       shortTermCommitmentDataTable.AsTableValuedParameter(
                           "[dbo].[commitmentMasterTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task<IEnumerable<ServiceLine>> SaveWorkdayServiceLineListForTableau(DataTable serviceLineDataTable)
        {
            var savedServiceLines = await _baseRepository.Context.AnalyticsConnection.QueryAsync<ServiceLine>(
               StoredProcedureMap.InsertServiceLineListForTableau,
               new
               {
                   serviceLines =
                       serviceLineDataTable.AsTableValuedParameter(
                           "[workday].[serviceLineTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return savedServiceLines;
        }

        public async Task<IEnumerable<PDGrade>> SaveWorkdayPDGradeListForTableau(DataTable pdGradeTableType)
        {
            var savedPDGrades = await _baseRepository.Context.AnalyticsConnection.QueryAsync<PDGrade>(
               StoredProcedureMap.InsertPDGradeListForTableau,
               new
               {
                   pdGrades =
                       pdGradeTableType.AsTableValuedParameter(
                           "[workday].[pdGradeTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return savedPDGrades;
        }

        public async Task<IEnumerable<Resource>> SaveWorkdayEmployeeDataForTableau(DataTable resourcesTableType)
        {
            var savedResources = await _baseRepository.Context.AnalyticsConnection.QueryAsync<Resource>(
               StoredProcedureMap.UpsertWorkdayEmployeeListForTableau,
               new
               {
                   employees =
                       resourcesTableType.AsTableValuedParameter(
                           "[workday].[employeeTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return savedResources;
        }

        public async Task UpsertWorkdayEmployeeStaffingTransactionToDB(DataTable workdayEmployeeTransactions)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
               StoredProcedureMap.UpsertWorkdayEmployeeStaffingTransaction,
               new
               {
                   workdayEmployeeTransactions =
                       workdayEmployeeTransactions.AsTableValuedParameter(
                           "[workday].[workdayEmployeeStaffingTransactionTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

        }

        public async Task UpsertWorkdayEmployeeLoATransactionToDB(DataTable workdayEmployeeLoATransactions)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
               StoredProcedureMap.UpsertWorkdayEmployeeLoATransaction,
               new
               {
                   workdayEmployeeLoAs =
                       workdayEmployeeLoATransactions.AsTableValuedParameter(
                           "[workday].[workdayEmployeeLoATableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

        }

        public async Task<IEnumerable<ResourceTimeOff>> SaveWorkdayTimeOffsForTableau(DataTable workdayEmployeesTimeOffs)
        {
            var savedTimeOffs = await _baseRepository.Context.AnalyticsConnection.QueryAsync<ResourceTimeOff>(
               StoredProcedureMap.InsertTimeOffsForTableau,
               new
               {
                   timeOffs =
                       workdayEmployeesTimeOffs.AsTableValuedParameter(
                           "[workday].[timeOffTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: 900);

            return savedTimeOffs;

        }

        public async Task<IEnumerable<string>> DeleteAvailabilityDataForRescindedEmployees(string employeeCodes)
        {
            var eCodesOfRescindedEmployees =  await _baseRepository.Context.AnalyticsConnection.QueryAsync<string>(
                StoredProcedureMap.DeleteAvailabilityDataForRescindedEmployees,
                new
                {
                    rescindedEmployeeCodes = employeeCodes
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return eCodesOfRescindedEmployees;
        }

        public async Task<IEnumerable<StaffableAs>> GetAllActiveStaffableAsRoles()
        {
            var data = await Task.Run(() => _baseRepository.Context.Connection.Query<StaffableAs>(
                StoredProcedureMap.GetAllActiveStaffableAsRoles,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return data;
        }

        public async Task UpdateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations()
        {
           await Task.Run(() => _baseRepository.Context.AnalyticsConnection.Query(
                StoredProcedureMap.UpdateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());
        }

        public async Task UpsertWorkdayEmployeesCertificatesToDB(DataTable workdayEmployeesCertifications)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
               StoredProcedureMap.UpsertWorkdayEmployeesCertifications,
               new
               {
                   workdayEmployeesCertificates =
                       workdayEmployeesCertifications.AsTableValuedParameter(
                           "[workday].[workdayEmployeesCertificationsTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

        }

        public async Task UpsertWorkdayEmployeesLanguagesToDB(DataTable workdayEmployeesLanguages)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
               StoredProcedureMap.UpsertWorkdayEmployeesLanguages,
               new
               {
                   workdayEmployeesLanguages =
                       workdayEmployeesLanguages.AsTableValuedParameter(
                           "[workday].[workdayEmployeesLanguagesTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

        }
    }
}
