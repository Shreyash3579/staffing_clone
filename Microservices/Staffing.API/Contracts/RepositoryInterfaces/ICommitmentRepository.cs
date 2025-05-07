using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ICommitmentRepository
    {
        Task<IEnumerable<CommitmentType>> GetCommitmentTypeLookupList(bool? showHidden);
        Task<IEnumerable<Commitment>> UpsertResourcesCommitments(DataTable resourcesCommitments);
        Task<IEnumerable<Commitment>> GetResourceCommitments(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<Commitment>> GetResourceCommitmentsByIds(string commitmentIds);
        Task<IEnumerable<Commitment>> GetResourceCommitmentsByDeletedIds(string commitmentIds);
        Task<IEnumerable<Commitment>> GetResourceCommitmentsWithinDateRangeByEmployees(string employeeCodes, DateTime? startDate, DateTime? endDate, string commitmentTypeCode);
        Task DeleteResourceCommitmentById(Guid id, string lastUpdatedBy);
        Task<IEnumerable<Commitment>> GetCommitmentsWithinDateRange(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Commitment>> GetCommitmentBySelectedValues(string commitmentTypeCodes, string employeeCodes, DateTime? startDate, DateTime? endDate,
            bool? ringfenceCommitmentsOnly);
        Task DeleteResourceCommitmentByIds(string commitmentIds, string lastUpdatedBy);
        Task<CommitmentType> UpsertCommitmentTypes(CommitmentType ringfenceCommitment);
        Task<bool> IsSTACommitmentCreated(string oldCaseCode = null, Guid? opportunityId = null, Guid? planningCardId = null);
        Task<IEnumerable<CaseOppCommitmentViewModel>> GetProjectSTACommitmentDetails(string oldCaseCodes, string opportunityIds, string planningCardIds);
        Task UpsertCaseOppCommitments(DataTable caseOppCommitmentDataTable);
        Task DeleteCaseOppCommitments(string commitmentIds, string lastUpdatedBy);

        Task<IEnumerable<NotificationRecipientMapping>> GetEmployeesForRingfenceAlert(DataTable employeeSchedulingOfficeTable, string lastUpdatedBy);

        Task<IEnumerable<CommitmentAlert>> UpsertRingfenceCommitmentAlerts(DataTable ringfenceNotificationTable);
    }
}
