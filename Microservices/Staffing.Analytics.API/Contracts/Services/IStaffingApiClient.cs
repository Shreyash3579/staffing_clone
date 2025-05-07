using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface IStaffingApiClient
    {
        Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByScheduleIds(string scheduleIds);
        Task<IEnumerable<PlaceholderAllocationViewModel>> GetPlaceholderAllocationsByScheduleIds(string placeholderScheduleIds);
        Task<IEnumerable<PlanningCard>> GetPlanningCardByPlanningCardIds(string planningCardIds);
        Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsByCommitmentIds(string commitmentIds);
        Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsByDeletedCommitmentIds(string commitmentIds);
        Task<IEnumerable<InvestmentCategory>> GetInvestmentCategoryLookupList();
        Task<IEnumerable<CaseRoleType>> GetCaseRoleTypeLookupList();
        Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsWithinDateRangeByEmployees(string employeeCodes, DateTime? startDate,
                DateTime? endDate, string commitmentTypeCode);
        Task<IEnumerable<CommitmentType>> GetCommitmentTypeList(bool? showHidden);
    }
}
