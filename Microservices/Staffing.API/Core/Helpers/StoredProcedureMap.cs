namespace Staffing.API.Core.Helpers
{
    public static class StoredProcedureMap
    {
        //Audit Trail Controller
        public const string GetAuditTrailForCaseOrOpportunity = "getAuditTrailForCaseOrOpportunity";
        public const string GetAuditTrailForEmployee = "getAuditTrailForEmployee";

        //Azure Search Query Log Controller
        public const string InsertAzureSearchQueryLog = "insertAzureSearchQueryLog";

        //Case Planning Controller
        public const string GetCasePlanningBoardDataByDateRange = "getCasePlanningBoardDataByDateRange";
        public const string GetCasePlanningBoardDataByProjectEndDateAndBucketIds = "getCasePlanningBoardDataByProjectEndDateAndBucketIds";
        public const string GetCasePlanningBoardDataByProjectIds = "getCasePlanningBoardDataByProjectIds";
        public const string GetOpportunityDataInCasePlanningBoard = "getOpportunityDataInCasePlanningBoard";
        public const string UpsertCasePlanningBoard = "upsertCasePlanningBoard";
        public const string UpsertCasePlanningBoardData = "upsertCasePlanningBoardData";
        public const string DeleteCasePlanningBoardByIds = "deleteCasePlanningBoardByIds";
        public const string UpsertCasePlanningBoardBucketPreferences = "upsertCasePlanningBoardBucketPreferences";
        public const string UpsertCasePlanningBoardIncludeInDemandPreferences = "upsertCasePlanningBoardIncludeInDemandPreferences";
        public const string UpsertCasePlanningProjectDetails = "upsertCasePlanningProjectDetails";
        public const string GetCasePlanningProjectDetails = "getCasePlanningProjectDetails";
        public const string UpsertCasePlanningBoardPreferencesOnDrop = "upsertCasePlanningBoardPreferencesOnDrop";

        //Case Planning Staffable Teams Controller
        public const string GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange = "getCasePlanningBoardStaffableTeamsByOfficesAndDateRange";
        public const string UpsertCasePlanningBoardStaffableTeams = "upsertCasePlanningBoardStaffableTeams";

        //CaseRoll Controller
        public const string GetCasesOnRollByCaseCodes = "getCasesOnRollByCaseCodes";
        public const string GetAllUnprocessedCasesOnCaseRoll = "getAllUnprocessedCasesOnCaseRoll";
        public const string UpsertCaseRolls = "upsertCaseRolls";
        public const string DeleteCaseRollsByIds = "deleteCaseRollsByIds";
        public const string DeleteRolledAllocationsByScheduleIds = "deleteRolledAllocationsByScheduleIds";
        public const string DeleteRolledAllocationsMappingFromCaseRollTracking = "deleteRolledAllocationsMappingFromCaseRollTracking";
        public const string GetCaseRollsRecentlyProcessedInStaffing = "getCaseRollsRecentlyProcessedInStaffing";

        //Commitments Controller
        public const string GetCommitmentTypeLookupList = "getcommitmentTypeLookupList";
        public const string GetCommitmentTypeReasonLookupList = "getCommitmentTypeReason";
        public const string GetResourceCommitments = "getResourceCommitments";
        public const string GetResourceCommitmentsByIds = "getResourceCommitmentsByIds";
        public const string GetResourceCommitmentsByDeletedIds = "getResourceCommitmentsByDeletedIds";
        public const string GetResourceCommitmentsWithinDateRangeByEmployees = "getResourceCommitmentsWithinDateRangeByEmployees";
        public const string UpsertResourcesCommitments = "upsertResourcesCommitments";
        public const string DeleteResourceCommitmentById = "deleteResourceCommitmentById";
        public const string DeleteResourceCommitmentByIds = "deleteResourceCommitmentByIds";
        public const string GetCommitmentsWithinDateRange = "getCommitmentsWithinDateRange";
        public const string GetCommitmentBySelectedValues = "getCommitmentBySelectedValues";
        public const string UpsertPracticeBasedRingfences = "upsertPracticeBasedRingfences";
        public const string IsSTACommitmentCreated = "isSTACommitmentCreated";
        public const string GetProjectSTACommitmentDetails = "getProjectSTACommitmentDetails";
        public const string InsertCaseOppCommitments = "insertCaseOppCommitments";
        public const string DeleteCaseOppCommitments = "deleteCaseOppCommitments";
        public const string GetEmployeesForRingfenceAlert = "getEmployeesForRingfenceAlert";
        public const string UpsertRingfenceCommitmentAlerts = "upsertRingfenceCommitmentAlerts";

        //CortexSku Controller
        public const string GetCortexSkuMappings = "getCortexSkuMappings";
        public const string GetOppCortexPlaceholderInfoByPipelineIds = "getOppCortexPlaceholderInfoByPipelineIds";
        public const string UpsertPlaceholderCreatedForCortexSKUs = "upsertPlaceholderCreatedForCortexPlaceholders";
        public const string UpsertPricingSKU = "upsertPricingSKU";
        public const string UpsertPricingSkuDataLog = "upsertPricingSkuDataLog";

        //Lookup Controller
        public const string GetInvestmentCategoryLookupList = "getInvestmentCategoryLookupList";
        public const string GetCaseRoleTypeLookupList = "getCaseRoleTypeLookupList";
        public const string GetStaffableAsTypeLookupList = "getStaffableAsTypeLookupList";
        public const string GetCasePlanningBoardBucketLookupListByEmployee = "getCasePlanningBoardBucketLookupListByEmployee";
        public const string GetUserPersonaTypeLookupList = "getUserPersonaTypeLookupList";
        public const string GetIndustryPracticeAreaLookupList = "getIndustryPracticeAreaLookupList";
        public const string GetCapabilityPracticeAreaLookupList = "getCapabilityPracticeAreaLookupList";
        public const string GetSecurityRoleLookupList = "getSecurityRoleLookupList";
        public const string GetSecurityFeatureLookupList = "getSecurityFeatureLookupList";
        public const string GetStaffingPreferencesLookupList = "getStaffingPreferencesLookupList";

        //Pipeline Changes Controller
        public const string GetPipelineChangesByPipelineIds = "getPipelineChangesByPipelineIds";
        public const string GetPipelineChangesByDateRange = "getPipelineChangesByDateRange";
        public const string UpsertPipelineChanges = "upsertPipelineChanges";
        public const string GetCaseChangesByOldCaseCodes = "getCaseChangesByOldCaseCode";
        public const string GetCaseTeamSizeByOldCaseCodes = "getCaseTeamSizeByOldCaseCodes";
        public const string UpsertCaseChanges = "upsertCaseChanges";
        public const string GetCaseChangesByDateRange = "getCaseChangesByDateRange";
        public const string GetCaseOppChangesByOfficesAndDateRange = "getCaseOppChangesByOfficesAndDateRange";

        //Schedule Master PlaceHolder Controller
        public const string DeletePlaceholderAllocationsByIds = "deletePlaceholderAllocationsByIds";
        public const string UpsertPlaceholderAllocations = "upsertPlaceholderAllocations";
        public const string GetPlaceholderAllocationsByPlaceholderScheduleIds = "getPlaceholderAllocationsByPlaceholderScheduleIds";
        public const string GetPlaceholderAllocationsByCaseCodes = "getPlaceholderAllocationsByCaseCodes";
        public const string GetPlaceholderAllocationsByPipelineIds = "getPlaceholderAllocationsByPipelineIds";
        public const string GetPlaceholderAllocationsBySelectedValues = "getPlaceholderAllocationsBySelectedValues";
        public const string GetPlaceholderAllocationsByEmployeeCodes = "getPlaceholderAllocationsByEmployeeCodes";
        public const string GetPlaceholderAllocationsByOfficeCodes = "getPlaceholderAllocationsByOfficeCodes";
        public const string GetPlaceholderAllocationsByPlanningCardIds = "getPlaceholderAllocationsByPlanningCardIds";
        public const string GetAllocationsByPlanningCardIds = "getAllocationsByPlanningCardIds";
        //Email Utility Data Log Controller
        public const string GetEmailUtilityDataLogsByDate = "getEmailUtilityDataLogsByDate";
        public const string UpsertEmailUtilityDataLog = "upsertEmailUtilityDataLog";

        //Resource Allocation Controller
        public const string UpsertAnalyticsReportData = "upsertScheduleMasterDetail";
        public const string UpsertResourceAllocations = "upsertResourceAllocations";
        public const string DeleteResourceAllocationById = "deleteAssignedResourceById";
        public const string DeleteResourceAllocationByIds = "deleteAssignedResourceByIds";
        public const string DeleteAnalyticsDataByScheduleId = "deleteAnalyticsDataByScheduleId";
        public const string DeleteAnalyticsDataByScheduleIds = "deleteAnalyticsDataByScheduleIds";
        public const string GetResourceAllocationsByCaseCodes = "getResourceAllocationsByCaseCodes";
        public const string GetResourceAllocationsCountByCaseCodes = "getResourceAllocationsCountByCaseCodes";
        public const string GetResourceAllocationsOnCaseRollByCaseCodes = "getResourceAllocationsOnCaseRollByCaseCodes";
        public const string GetResourceAllocationsByPipelineIds = "getResourceAllocationsByPipelineIds";
        public const string GetResourceAllocationsByScheduleIds = "getResourceAllocationsByScheduleIds";
        public const string GetResourceAllocationsBySelectedValues = "getResourceAllocationsBySelectedValues";
        public const string GetResourceAllocationsBySelectedValues_v2 = "getResourceAllocationsBySelectedValues_v2";
        public const string GetResourceAllocationsBySelectedValues_v3 = "getResourceAllocationsBySelectedValues_v3";
        public const string GetResourceAllocationsByEmployeeCodes = "getEmployeeAllocationsByEmployeeCodes";
        public const string GetResourceAllocationsByOfficeCodes = "getEmployeeAllocationsByOfficeCodes";
        public const string GetResourcesWithNoAvailabilityRecords = "getResourcesWithNoAvailabilityRecords";
        public const string GetResourcesFullAvailabilityDateRange = "getResourcesFullAvailabiltyDateRange";
        public const string UpdateCostForResourcesAvailableInFullCapacity = "UpdateCostForResourcesAvailableInFullCapacity";
        public const string GetRecordsUpdatedOnCaseRoll = "getRecordsUpdatedOnCaseRoll";
        public const string GetAllocationsForEmployeesOnPrePost = "getAllocationsForEmployeesOnPrePost";
        public const string GetResourceAllocationsBySelectedSupplyValues = "getResourceAllocationsBySelectedSupplyValues";
        public const string GetLastTeamByEmployeeCode = "getLastTeamByEmployeeCode";
        public const string GetLastBillableDateByEmployeeCodes = "getLastBillableDateByEmployeeCodes";

        //Resource History Controller
        public const string GetHistoricalStaffingAllocationsForEmployee = "getHistoricalStaffingAllocationsForEmployee";

        //Ringfence Management Controller
        public const string GetRingfencesDetailsByOfficesAndCommitmentCodes = "getRingfencesDetailsByOfficesAndCommitmentCodes";
        public const string GetRingfenceAuditLogByOfficeAndCommitmentCode = "getRingfenceAuditLogByOfficeAndCommitmentCode";
        public const string UpsertRingfenceDetails = "upsertRingfenceDetails";

        //User Custom Filters Controller
        public const string GetUserCustomResourceFiltersByEmployeeCode = "getUserCustomResourceFiltersByEmployeeCode";
        public const string UpsertUserCustomResourceFilter = "upsertUserCustomResourceFilter";
        public const string DeleteUserCustomResourceFilters = "deleteUserCustomResourceFilters";

        //User Preferences Controller
        public const string DeleteUserPreferences = "deleteUserPreferences";
        public const string GetUserPreferences = "getUserPreferences";
        public const string InsertUserPreferences = "insertUserPreferences";
        public const string UpdateUserPreferences = "updateUserPreferences";
        public const string UpsertUserPreferences = "upsertUserPreferences";

        //User Preferences Supply Group Controller
        public const string GetUserPreferenceSupplyGroups = "getUserPreferenceSupplyGroups";
        public const string UpsertUserPreferenceSupplyGroups = "upsertUserPreferenceSupplyGroups";
        public const string DeleteUserPreferenceSupplyGroupByIds = "deleteUserPreferenceSupplyGroupByIds";
        public const string UpdateUserPreferenceSupplyGroupSharedInfo = "updateUserPreferenceSupplyGroupSharedInfo";

        //User Preferences Supply Group Shared Info Controller
        public const string GetUserPreferenceGroupSharedInfo = "getUserPreferenceGroupSharedInfo";
        public const string UpsertUserPreferenceGroupSharedInfo = "upsertUserPreferenceGroupSharedInfo";

        // Notification Controller
        public const string GetUserNotifications = "getUserNotifications";
        public const string UpdateUserNotificationStatus = "updateUserNotificationStatus";

        // SKU Term Controller
        public const string GetSKUTermList = "getSKUTermList";
        public const string GetSKUTermsForOpportunity = "getSKUTermsForOpportunity";
        public const string GetSKUTermsForCase = "getSKUTermsForCase";
        public const string InsertSKUCaseTerms = "insertSKUCaseTerms";
        public const string UpdateSKUCaseTerms = "updateSKUCaseTerms";
        public const string DeleteSKUCaseTerms = "deleteSKUCaseTerms";
        public const string GetSKUTermsForOldCaseCodeOrPipelineIdForDuration = "getSKUTermsForOldCaseCodeOrPipelineIdForDuration";

        // Sku Controller
        public const string GetSkuForProjects = "getSkuForProjects";
        public const string UpsertSkuForProjects = "upsertSkuForProjects";

        // Staff-able As Controller
        public const string GetResourceActiveStaffableAsByEmployeeCodes = "getResourceActiveStaffableAsByEmployeeCodes";
        public const string UpsertResourceStaffableAs = "upsertResourceStaffableAs";
        public const string DeleteResourceStaffableAsById = "deleteResourceStaffableAsById";

        //Staffing Responsible Controller
        public const string GetResourceStaffingResponsibleByEmployeeCodes = "getResourceStaffingResponsibleByEmployeeCodes";
        public const string UpsertEmployeeStaffingResponsible = "upsertEmployeeStaffingResponsible";

        //Staffing Preferences Controller
        public const string GetEmployeeStaffingPreferencesForInsightsTool = "getEmployeeStaffingPreferencesForInsightsTool";
        public const string GetAllEmployeeStaffingPreferencesForInsightsTool = "getAllEmployeeStaffingPreferencesForInsightsTool";
        public const string UpsertEmployeeStaffingPreferencesForInsightsTool = "upsertEmployeeStaffingPreferencesForInsightsTool";

        // Tagged Case Controller
        public const string GetCasesByResourceServiceLines = "getCasesByResourceServiceLines";
        public const string GetOpportunitiesByResourceServiceLines = "getOpportunitiesByResourceServiceLines";

        // NoteLogs Controller
        public const string GetResourceViewNotes = "getResourceViewNotes";
        public const string GetResourceNotesByLastUpdatedDate = "getResourceNotesByLastUpdatedDate";
        public const string UpsertResourceViewNote = "upsertResourceViewNote";
        public const string DeleteResourceViewNotes = "deleteResourceViewNotes";
        public const string GetCasePlanningViewNotes = "getCasePlanningViewNotes";
        public const string UpsertCasePlanningViewNote = "upsertCasePlanningViewNote";
        public const string DeleteCasePlanningViewNotes = "deleteCasePlanningViewNotes";
        public const string GetNotesAlert = "getNotesAlert";
        public const string GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode = "getMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode";
        public const string UpdateNotesAlertStatus = "updateNotesAlertStatus";

        // PlanningCard Controller
        public const string DeletePlanningCardAndItsAllocation = "deletePlanningCardAndItsAllocation";
        public const string InsertPlanningCard = "insertPlanningCard";
        public const string UpdatePlanningCard = "updatePlanningCard";
        public const string UpsertPlanningCard = "upsertPlanningCard";
        public const string GetPlanningCardAndItsAllocationsByEmployeeCodeAndFilters = "getPlanningCardAndItsAllocationsByEmployeeCodeAndFilters";
        public const string GetPlanningCardAllocationsByEmployeeCodesAndDuration = "getPlanningCardAllocationsByEmployeeCodesAndDuration";
        public const string SharePlanningCard = "sharePlanningCard";
        public const string GetPlanningCardsPlaceholdersAllocationsWithinDateRange = "getPlanningCardsAndPlacholdersAllocationsWithinDateRange";
        public const string GetPlanningCardByPlanningCardIds = "getPlanningCardByPlanningCardIds";
        public const string GetPlanningCardByPegOpportunityIds = "getPlanningCardByPegOpportunityIds";
        public const string GetPlanningCardsForTypeahead = "getPlanningCardsForTypeahead";

        // Security User Controller
        public const string GetAllSecurityUsers = "getAllSecurityUsers";
        public const string DeleteSecurityUser = "deleteSecurityUser";
        public const string UpsertBOSSSecurityUser = "upsertBOSSSecurityUser";
        public const string GetAllSecurityGroups = "getAllSecurityGroups";
        public const string UpsertBOSSSecurityGroup = "upsertBOSSSecurityGroup";
        public const string DeleteSecurityGroup = "deleteSecurityGroup";
        public const string GetAllRevOfficeList = "[ccm].[getAllRevOfficeList]";
        public const string GetServiceLineList = "[workday].[getAllServiceLineList]";
        public const string UpsertRevOffices = "[ccm].[upsertRevOffices]";
        public const string UpsertServiceLineList = "[workday].[upsertServiceLines]";
        public const string UpdateSecurityUserForWFPRole = "[dbo].[updateSecurityUserForWFPRole]";

        // Employee staffing preferences Controller
        public const string GetEmployeeStaffingPreferences = "getEmployeeStaffingPreferences";
        public const string UpsertEmployeeStaffingPreferences = "upsertEmployeeStaffingPreferences";
        public const string DeleteEmployeeStaffingPreferenceByType = "deleteEmployeeStaffingPreferenceByType";

        // Office Closure Cases Component
        public const string UpsertOfficeClosureCases = "upsertOfficeClosureCases";
        public const string GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType = "getOfficeClosureChangesWithinDateRangeByOfficeAndCaseType";

        // Preponed Cases Allocations Audit Controller
        public const string UpsertPreponedCaseAllocationsAudit = "upsertPreponedCaseAllocationsAudit";
        public const string GetPreponedCaseAllocationsAudit = "getPreponedCaseAllocationsAudit";

        // Share point controller for SMAP Missions
        public const string GetSmapMissionNotesByEmployeeCodes = "getSmapMissionNotesByEmployeeCodes";

        // Get Data Sync Mismatch for count in Staffing
        public const string GetCountforSyncTablesInStaffing = "getCountforSyncTablesInStaffing";

        // Get Resource commercial model and CD
        public const string GetCommercialModelList = "getCommercialModelList";
        public const string GetResourceCommercialModel = "getCommercialModelForEmployees";
        public const string GetResourceRecentCD = "getRecentCDForEmployees";
        public const string UpsertSkills = "upsertSkills";
        public const string UpsertResourceCommercialModel = "upsertCommercialModelForEmployee";
        public const string UpsertResourceRecentCD = "upsertCDforEmployee";
        public const string GetRecentCDList = "getRecentCDList";
        public const string DeleteResourceViewCD = "deleteResourceViewCD";
        public const string DeleteResourceViewCommercialModel = "deleteResourceViewCommercialModel";
    }

}