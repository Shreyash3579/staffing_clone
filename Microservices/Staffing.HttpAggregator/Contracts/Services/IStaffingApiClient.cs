using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IStaffingApiClient
    {
        // Resource Allocation            
        Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsBySelectedValues(string oldCaseCodes, string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate);
        Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsBySelectedSupplyValues(string officeCodes, DateTime startDate, DateTime endDate,
            string staffingTags, string currentLevelGrades);
        Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByPipelineIds(string pipelineIds);
        Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByPipelineId(string pipelineId);
        Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByCaseCodes(string oldCaseCodes);
        Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByCaseCode(string oldCaseCode);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsByEmployeeCodes(string employeeCodes, DateTime? startDate,
            DateTime? endDate);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByEmployeeCodes(string employeeCodes,
            DateTime? startDate, DateTime? endDate);
        //commented on 06-jun-23 as it is not being used anymore
        //Task<IList<ResourceAssignmentViewModel>> GetResourceAllocationsByOfficeCodes(string officeCodes, DateTime? startDate,
        //    DateTime? endDate);
        Task<IList<ResourceAllocation>> UpsertResourceAllocations(IEnumerable<ResourceAllocation> resourceAllocations);
        Task<IEnumerable<EmailUtilityData>> GetEmailUtilityDataLogsByDateAndEmailType(DateTime dateOfEmail, string emailType);
        Task<IList<EmailUtilityData>> UpsertEmailUtilityDataLog(IList<EmailUtilityData> employees);
        Task<IEnumerable<ResourceAllocation>> UpdateResourceAssignmentForTableau(
            IEnumerable<ResourceAllocation> resourceAllocations);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetHistoricalStaffingAllocationsForEmployee(string employeeCode);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetStaffingAllocationsForEmployee(string employeeCode);
        Task DeleteResourceAllocationByIds(string allocationIds, string lastUpdatedBy);

        Task DeletePlanningCardAndItsAllocations(Guid id, string lastUpdatedBy);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date);
        Task<IEnumerable<EmployeeLastBillableDateViewModel>> GetLastBillableDateByEmployeeCodes(string employeeCodes, DateTime? date = null);


        // Audit Trail
        Task<IList<AuditCaseHistory>> GetAuditTrailForCaseOrOpportunity(string oldCaseCode, string pipelineId, int? limit, int? offset);

        Task<IEnumerable<AuditEmployeeHistory>> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset);

        // Case Roll
        Task<IEnumerable<CaseRoll>> GetCasesOnRollByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<CaseRoll>> UpsertCaseRolls(IEnumerable<CaseRoll> upsertedCaseRolls);
        Task<string> DeleteCaseRollsByIds(string caseRollIdsToDelete, string lastUpdatedBy);
        Task<string> DeleteRolledAllocationsByScheduleIds(string rolledScheduleIds, string lastUpdatedBy);

        Task<IEnumerable<SKUCaseTerms>> GetSKUTermsForCaseOrOpportunityForDuration(string oldCaseCodes,
            string pipelineIds, DateTime startDate, DateTime endDate);

        // Case Planning
        //Task<CasePlanningBoardDataModel> GetCasePlanningBoardDataByDateRange(DateTime startDate);

        Task<IEnumerable<CasePlanningProjectPreferences>> GetCasePlanningProjectDetails(string oldCaseCodes, string pipelineIds, string planningCardIds);
        Task<CasePlanningBoardDataModel> GetCasePlanningBoardDataByDateRange(DateTime startDate, DateTime? endDate, string employeeCode);
        Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectEndDateAndBucketIds(DateTime startDate, string bucketIds);
        Task<IEnumerable<CasePlanningBoardStaffableTeams>> GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(string officeCodes, DateTime startWeek, DateTime endWeek);

        // Commitments
        Task<IEnumerable<CommitmentViewModel>> GetCommitmentsWithinDateRange(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsWithinDateRangeByEmployees(string employeeCodes,
            DateTime? startDate, DateTime? endDate, string commitmentTypeCode);
        Task<IEnumerable<Commitment>> GetResourceCommitments(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<CommitmentViewModel>> GetCommitmentBySelectedValues(string commitmentTypeCodes, string employeeCodes, DateTime? startDate,
            DateTime? endDate, bool? ringfenceCommitmentsOnly);

        Task<IEnumerable<CaseOppCommitmentViewModel>> GetProjectSTACommitmentDetails(string oldCaseCodes, string opportunityIds, string planningCardIds);

        Task<IEnumerable<Commitment>> UpsertResourceCommitment(IEnumerable<Commitment> commitments);

        Task<IEnumerable<CommitmentAlert>> UpsertRingfenceCommitmentAlerts(CommitmentEnrichment commitmentDetails);

        //Currency
        Task<IEnumerable<CurrencyRate>> GetCurrencyRatesByCurrencyCodesAndDate(string currencyCodes, string currencyRateTypeCode, DateTime effectiveFromDate, DateTime effectiveTillDate);
        
        // Lookup
        Task<IEnumerable<SKUTerm>> GetSKUTermList();
        Task<IEnumerable<InvestmentCategory>> GetInvestmentCategoryList();
        Task<IEnumerable<CaseRoleType>> GetCaseRoleTypeList();
        Task<IEnumerable<CommitmentType>> GetCommitmentTypeList();

        Task<IEnumerable<string>> GetCasesByResourceServiceLines(string oldCaseCodes, string serviceLineNames);
        Task<IEnumerable<string>> GetOpportunitiesByResourceServiceLines(string pipelineIds, string serviceLineNames);

        // Case Opp Changes
        Task<CaseOppChanges> GetPipelineChangesByPipelineId(Guid pipelineId);
        Task<IEnumerable<CaseOppChanges>> GetPipelineChangesByPipelineIds(string pipelineIds);
        Task<IList<CaseOppChanges>> GetPipelineChangesByDateRange(DateTime startDate, DateTime? endDate);
        Task<IEnumerable<CaseOppChanges>> GetCaseChangesByOldCaseCodes(string oldCaseCodes);
        Task<CaseOppChanges> UpsertCaseChanges(CaseOppChanges upsertedData);
        Task<IList<CaseOppChanges>> GetCaseChangesByDateRange(DateTime startDate, DateTime? endDate);
        Task<IEnumerable<CaseOppCortexTeamSize>> GetCaseTeamSizeByOldCaseCodes(string oldCaseCodes);
        Task<IList<CaseOppChanges>> GetCaseOppChangesByOfficesAndDateRange(string officeCodes, DateTime? startDate = null, DateTime? endDate = null);

        // Cortex Sku Controller
        Task<IEnumerable<CaseOppCortexTeamSize>> GetOppCortexPlaceholderInfoByPipelineIds(string pipelineIds);

        // Schedule Master placeholder
        Task<IList<ResourceAssignmentViewModel>> GetPlaceholderAllocationsByPipelineIds(string pipelineIds);
        Task<IList<ResourceAssignmentViewModel>> GetPlaceholderAllocationsByCaseCodes(string oldCaseCodes);
        Task<IList<ResourceAssignmentViewModel>> GetPlaceholderAllocationsByPlanningCardIds(string planningCardIds);
        Task<IList<ResourceAssignmentViewModel>> GetAllocationsByPlanningCardIds(string planningCardIds);
        Task<IList<ScheduleMasterPlaceholder>> UpsertPlaceholderAllocations(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations);

        // Note Controller
        Task<IEnumerable<ResourceViewNote>> GetResourceViewNotes(string employeeCodes, string loggedInUser, string noteTypeCode);
        Task<ResourceViewNote> UpsertResourceViewNote(ResourceViewNote resourceViewNote);

        Task<ResourceCD> UpsertResourceCD(ResourceCD resourceViewCD);

        Task<ResourceCommercialModel> UpsertResourceCommercialModel(ResourceCommercialModel resourceViewCommercialModel);
        Task<CaseViewNote> UpsertCaseViewNote(CaseViewNote caseViewNote);
        Task<IEnumerable<CaseViewNote>> GetLatestCaseViewNotes(string oldCaseCodes, string pipelineIds, string planningCardIds, string loggedInUser);
        Task<IEnumerable<CaseViewNote>> GetCaseViewNotesByPlanningCardIds(string planningCardIds, string loggedInUser);
        Task<IEnumerable<CaseViewNote>> GetCaseViewNotesByOldCaseCodes(string oldCaseCodes, string loggedInUser);
        Task<IEnumerable<CaseViewNote>> GetCaseViewNotesByPipelineIds(string pipelineIds, string loggedInUser);
        Task<IEnumerable<CaseViewNote>> GetCaseViewNotes(string oldCaseCodes, string pipelineIds, string planningCardIds, string loggedInUser);
        Task<IEnumerable<NoteAlert>> GetNotesAlert(string employeeCode);

        Task<IEnumerable<NotesSharedWithGroup>> GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(string employeeCode);
        Task<IEnumerable<ResourceCD>> GetResourceRecentCD(string employeeCodes);

        Task<IEnumerable<ResourceCommercialModel>> GetResourceCommercialModel(string employeeCodes);




        // Planning Card Controller
        Task<IEnumerable<PlanningCard>> GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(string employeeCode, string officeCodes, string staffingTags, string bucketIds = null);
        Task<IEnumerable<PlanningCard>> GetPlanningCardByPlanningCardIds(string planningCardIds);
        Task<PlanningCard> UpdatePlanningCard(PlanningCard planningCardToUpdate);

        //Security User Controller
        Task<IEnumerable<SecurityUser>> GetAllSecurityUsers();

        // Employee staffing preference Controller
        Task<IEnumerable<EmployeeStaffingPreferences>> GetEmployeeStaffingPreferences(string employeeCode);
        Task<IEnumerable<EmployeeStaffingPreferences>> UpsertEmployeeStaffingPreferences(IEnumerable<EmployeeStaffingPreferences> employeeStaffingPreferences);
        Task DeleteEmployeeStaffingPreferenceByType(string employeeCode, string preferenceTypeCode);
        Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlacholderAndPlanningCardAllocationsWithinDateRange(string employeeCodes, DateTime? startDate, DateTime? endDate);
        //commented on 06-jun-23 as it is not being used anymore
        //Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlanningCardAllocationsByEmployeeCodesAndDuration(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<StaffableAs>> GetResourceActiveStaffableAsByEmployeeCodes(string employeeCodes);

        Task<IEnumerable<StaffingResponsible>> GetResourceStaffingResponsibleData(string employeeCodes);

        // RingFence Management Controller
        Task<IEnumerable<RingfenceManagement>> GetRingfencesDetailsByOfficesAndCommitmentCodes(string officeCodes, string commitmentTypeCodes);
        Task<IEnumerable<RingfenceManagement>> GetRingfenceAuditLogByOfficeAndCommitmentCode(string officeCode, string commitmentTypeCode);

        //Office Closure Controller
        //commented on 06-jun-23 as it is not being used anymore
        //Task<OfficeClosureCases> GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate);

        //Sku Controller
        Task<IEnumerable<SKUDemand>> GetSKUTermForProjects(string oldCaseCodes, string pipelineIds, string planningCardIds);

        //User Preference Supply Group Controller
        Task<IList<UserPreferenceSupplyGroup>> GetUserPreferenceSupplyGroups(string employeeCode);
        Task<IList<UserPreferenceSavedGroup>> GetUserPreferenceSavedGroups(string employeeCode);
        Task<IList<UserPreferenceGroupSharedInfo>> GetUserPreferenceGroupSharedInfo(string groupId);

        Task<IEnumerable<UserPreferenceSupplyGroup>> UpsertUserPreferenceSupplyGroups(IEnumerable<UserPreferenceSupplyGroup> supplyGroupsToUpsert);
        Task<IEnumerable<UserPreferenceSavedGroup>> UpsertUserPreferenceSavedGroups(IEnumerable<UserPreferenceSavedGroup> savedGroupsToUpsert);

        Task<IList<UserPreferenceGroupSharedInfo>> UpsertUserPreferenceGroupSharedInfo(IEnumerable<UserPreferenceGroupSharedInfo> sharedWithInfo);

        //Preponed Cases Allocations Audit Controller
        Task<IEnumerable<PreponedCasesAllocationsAudit>> GetPreponedCaseAllocationsAudit(string serviceLineCodes, string officeCodes,
            DateTime? startDate = null, DateTime? endDate = null);
        Task<PlanningCard> UpsertPlanningCardData(PlanningCard planningCard);
    }
}