using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class CommitmentRepository : ICommitmentRepository
    {
        private readonly IBaseRepository<Commitment> _baseRepository;

        public CommitmentRepository(IBaseRepository<Commitment> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;

        }

        public async Task<IEnumerable<CommitmentType>> GetCommitmentTypeLookupList(bool? showHidden)
        {
            var commitmentTypes = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<CommitmentType>(
                StoredProcedureMap.GetCommitmentTypeLookupList,
                new { showHidden },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return commitmentTypes;
        }

        public async Task<IEnumerable<Commitment>> UpsertResourcesCommitments(DataTable resourcesCommitmentsDataTable)
        {
            var resourcesCommitments = await Task.Run(() => _baseRepository.Context.Connection.Query<Commitment, CommitmentType, Commitment>(
            StoredProcedureMap.UpsertResourcesCommitments,
            (resourceCommitmentObject, commitmentType) =>
            {
                resourceCommitmentObject.CommitmentType = commitmentType;
                return resourceCommitmentObject;
            },
            new
            {
                resourcesCommitments =
                    resourcesCommitmentsDataTable.AsTableValuedParameter(
                        "[dbo].[commitmentMasterTableType]")
            },
            splitOn: "commitmentTypeCode",
            commandType: CommandType.StoredProcedure,
            commandTimeout: _baseRepository.Context.TimeoutPeriod).ToList());

            return resourcesCommitments;
        }

        public async Task<CommitmentType> UpsertCommitmentTypes(CommitmentType ringfenceCommitments)
        {
            var upsertedDetails = await _baseRepository.Context.Connection.QuerySingleAsync<CommitmentType>(
                  StoredProcedureMap.UpsertPracticeBasedRingfences,
                  new
                  {
                      ringfenceCommitments.CommitmentTypeCode,
                      ringfenceCommitments.CommitmentTypeName,
                      ringfenceCommitments.IsStaffingTag,
                      ringfenceCommitments.AllowsStaffingInAmericas,
                      ringfenceCommitments.AllowsStaffingInEMEA,
                      ringfenceCommitments.AllowsStaffingInAPAC,
                      ringfenceCommitments.LastUpdatedBy

                  },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedDetails;

        }

        public async Task<IEnumerable<Commitment>> GetResourceCommitmentsByIds(string commitmentIds)
        {
            var commitments = await Task.Run(() => _baseRepository.Context.Connection.Query<Commitment, CommitmentType, Commitment>(
                StoredProcedureMap.GetResourceCommitmentsByIds,
                (resourceCommitmentObject, commitmentType) =>
                {
                    resourceCommitmentObject.CommitmentType = commitmentType;
                    return resourceCommitmentObject;
                },
                new { commitmentIds },
                splitOn: "commitmentTypeCode",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return commitments;
        }

        public async Task<IEnumerable<Commitment>> GetResourceCommitmentsByDeletedIds(string commitmentIds)
        {
            var commitments = await Task.Run(() => _baseRepository.Context.Connection.Query<Commitment, CommitmentType, Commitment>(
                StoredProcedureMap.GetResourceCommitmentsByDeletedIds,
                (resourceCommitmentObject, commitmentType) =>
                {
                    resourceCommitmentObject.CommitmentType = commitmentType;
                    return resourceCommitmentObject;
                },
                new { commitmentIds },
                splitOn: "commitmentTypeCode",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return commitments;
        }

        public async Task<IEnumerable<Commitment>> GetResourceCommitments(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var commitments = await Task.Run(() => _baseRepository.Context.Connection.Query<Commitment, CommitmentType, Commitment>(
                StoredProcedureMap.GetResourceCommitments,
                (resourceCommitmentObject, commitmentType) =>
                {
                    resourceCommitmentObject.CommitmentType = commitmentType;
                    return resourceCommitmentObject;
                },
                new { employeeCode, effectiveFromDate, effectiveToDate },
                splitOn: "commitmentTypeCode",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return commitments;
        }

        public async Task DeleteResourceCommitmentById(Guid id, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { id, lastUpdatedBy }, StoredProcedureMap.DeleteResourceCommitmentById);
        }

        public async Task DeleteResourceCommitmentByIds(string ids, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { ids, lastUpdatedBy }, StoredProcedureMap.DeleteResourceCommitmentByIds);
        }

        public async Task<IEnumerable<Commitment>> GetCommitmentsWithinDateRange(DateTime startDate, DateTime endDate)
        {
            var commitments = await Task.Run(() => _baseRepository.Context.Connection.Query<Commitment, CommitmentType, Commitment>(
                StoredProcedureMap.GetCommitmentsWithinDateRange,
                (resourceCommitmentObject, commitmentType) =>
                {
                    resourceCommitmentObject.CommitmentType = commitmentType;
                    return resourceCommitmentObject;
                },
                new { startDate, endDate },
                splitOn: "commitmentTypeCode",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return commitments;
        }

        public async Task<IEnumerable<Commitment>> GetResourceCommitmentsWithinDateRangeByEmployees(string employeeCodes, DateTime? startDate,
                DateTime? endDate, string commitmentTypeCode)
        {
            var commitments = await Task.Run(() => _baseRepository.Context.Connection.Query<Commitment, CommitmentType, Commitment>(
                StoredProcedureMap.GetResourceCommitmentsWithinDateRangeByEmployees,
                (resourceCommitmentObject, commitmentType) =>
                {
                    resourceCommitmentObject.CommitmentType = commitmentType;
                    return resourceCommitmentObject;
                },
                new { employeeCodes, startDate, endDate, commitmentTypeCode },
                splitOn: "commitmentTypeCode",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return commitments;
        }

        public async Task<IEnumerable<Commitment>> GetCommitmentBySelectedValues(string commitmentTypeCodes, string employeeCodes, DateTime? startDate,
            DateTime? endDate, bool? ringfenceCommitmentsOnly)
        {
            var commitments = await Task.Run(() => _baseRepository.Context.Connection.Query<Commitment, CommitmentType, Commitment>(
                StoredProcedureMap.GetCommitmentBySelectedValues,
                (resourceCommitmentObject, commitmentType) =>
                {
                    resourceCommitmentObject.CommitmentType = commitmentType;
                    return resourceCommitmentObject;
                },
                new
                {
                    commitmentTypeCodes,
                    employeeCodes,
                    startDate,
                    endDate,
                    ringfenceCommitmentsOnly
                },
                splitOn: "commitmentTypeCode",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());
            return commitments;
        }

        public async Task<bool> IsSTACommitmentCreated(string oldCaseCode = null, Guid? opportunityId = null, Guid? planningCardId = null)
        {
            var isSTACommitmentCreated = await _baseRepository.Context.Connection.QueryFirstOrDefaultAsync<bool>(
                 StoredProcedureMap.IsSTACommitmentCreated,
                 new
                 {
                     oldCaseCode,
                     opportunityId,
                     planningCardId
                 },
                 commandType: CommandType.StoredProcedure,
                 commandTimeout: _baseRepository.Context.TimeoutPeriod
             );

            return isSTACommitmentCreated;
        }

        public async Task<IEnumerable<CaseOppCommitmentViewModel>> GetProjectSTACommitmentDetails(string oldCaseCodes, string opportunityIds, string planningCardIds)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@oldCaseCodes", oldCaseCodes);
            parameters.Add("@opportunityIds", opportunityIds);
            parameters.Add("@planningCardIds", planningCardIds);

            return await _baseRepository.Context.Connection.QueryAsync<CaseOppCommitmentViewModel>(
                StoredProcedureMap.GetProjectSTACommitmentDetails,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod
            );
        }

        public async Task UpsertCaseOppCommitments(DataTable caseOppCommitmentDataTable)
        {
            await Task.Run(() => _baseRepository.Context.Connection.ExecuteAsync(
                StoredProcedureMap.InsertCaseOppCommitments,
                new
                {
                    commitments = caseOppCommitmentDataTable.AsTableValuedParameter("[dbo].[caseOppCommitmentTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod)
            );
        }

        public async Task DeleteCaseOppCommitments(string commitmentIds, string lastUpdatedBy)
        {
            await _baseRepository.Context.Connection.ExecuteAsync(
                 StoredProcedureMap.DeleteCaseOppCommitments,
                 new
                 {
                     commitmentIds,
                     lastUpdatedBy
                 },
                 commandType: CommandType.StoredProcedure,
                 commandTimeout: _baseRepository.Context.TimeoutPeriod
             );
        }


        public async Task<IEnumerable<NotificationRecipientMapping>> GetEmployeesForRingfenceAlert(DataTable employeeSchedulingOfficeTable, string lastUpdatedBy)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SchedulingOffices", employeeSchedulingOfficeTable.AsTableValuedParameter("dbo.EmployeeSchedulingOfficeTableType"));
            parameters.Add("@lastUpdatedBy", lastUpdatedBy);

            var result = await _baseRepository.Context.Connection.QueryAsync<NotificationRecipientMapping>(
                StoredProcedureMap.GetEmployeesForRingfenceAlert,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod
            );

            return result;
        }


        public async Task<IEnumerable<CommitmentAlert>> UpsertRingfenceCommitmentAlerts(DataTable ringfenceNotificationTable)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ringfenceAlerts", ringfenceNotificationTable.AsTableValuedParameter("dbo.ringfenceNotificationTableType"));

            var result = await _baseRepository.Context.Connection.QueryAsync<CommitmentAlert>(
                    StoredProcedureMap.UpsertRingfenceCommitmentAlerts,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _baseRepository.Context.TimeoutPeriod
                );

            return result;
        }

    }
}