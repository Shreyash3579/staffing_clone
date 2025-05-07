using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface ICommitmentService
    {
        Task<IEnumerable<CommitmentType>> GetCommitmentTypeLookupList(bool? showHidden);
        Task<IEnumerable<Commitment>> UpsertResourcesCommitments(IList<Commitment> resourcesCommitments);
        Task<CommitmentType> UpsertCommitmentTypes(CommitmentType ringfenceCommitments);
        Task<IEnumerable<Commitment>> GetResourceCommitments(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsByIds(string commitmentIds);
        Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsByDeletedIds(string commitmentIds);
        Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsWithinDateRangeByEmployees(string employeeCodes, DateTime? startDate,
            DateTime? endDate, string commitmentTypeCode);
        Task DeleteResourceCommitmentById(Guid id, string lastUpdatedBy);
        Task<IEnumerable<CommitmentViewModel>> GetCommitmentsWithinDateRange(DateTime startDate, DateTime endDate);
        Task<(string, IEnumerable<CommitmentViewModel>)> GetCommitmentBySelectedValues(string commitmentTypeCodes, string employeeCodes, DateTime? startDate,
            DateTime? endDate, bool? ringfenceCommitmentsOnly, string clientId = "");
        Task DeleteResourceCommitmentByIds(string commitmentIds, string lastUpdatedBy);
        Task<IEnumerable<CommitmentViewModel>> checkPegRingfenceAllocationAndInsertDownDayCommitments(IList<ResourceAllocation> resourceAllocations);
        Task<bool> IsSTACommitmentCreated(string oldCaseCode = null, Guid? opportunityId = null, Guid? planningCardId = null);
        Task<IEnumerable<CaseOppCommitmentViewModel>> GetProjectSTACommitmentDetails(string oldCaseCodes, string opportunityIds, string planningCardIds);
        Task<IEnumerable<CommitmentWithCaseOppInfo>> UpsertCaseOppCommitments(IEnumerable<InsertCaseOppCommitmentViewModel> insertCaseOppCommitments);
        Task DeleteCaseOppCommitments(string commitmentIds, string lastUpdatedBy);
        Task<IEnumerable<CommitmentAlert>> UpsertRingfenceCommitmentAlerts(CommitmentEnrichment commitmentDetails);
    }
}