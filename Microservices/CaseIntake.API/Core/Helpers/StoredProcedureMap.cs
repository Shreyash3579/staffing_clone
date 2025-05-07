namespace CaseIntake.API.Core.Helpers
{
    public static class StoredProcedureMap
    {
        public static string GetCaseIntakeLeadershipbyOldCaseCodeOrOpportunityId = "getCaseIntakeLeadershipbyOldCaseCodeOrOpportunityId";
        public static string GetCaseIntakeDetailbyOldCaseCodeOrOpportunityId = "getCaseIntakeDetailbyOldCaseCodeOrOpportunityId";
        public static string UpsertCaseIntakeLeadership = "upsertCaseIntakeLeadership";
        public static string UpsertCaseIntakeDetail = "upsertCaseIntakeDetail";
        public static string GetCaseIntakeWorkstreamRolesByCaseOrOppId = "getCaseIntakeWorkstreamRolesByCaseOrOppId";
        public static string UpsertCaseIntakeRoleByCaseOrOppId = "upsertCaseIntakeRole";
        public static string UpsertCaseIntakeRoleAndWorkstream = "upsertCaseIntakeRoleAndWorkstream";
        public static string DeleteWorkStreamByIds = "deleteWorkStreamByIds";
        public static string DeleteRoleByIds = "deleteRoleByIds";
        public static string DeleteLeadershipById = "deleteLeadershipById";
        public static string DeleteLeadershipByCaseRoleCode = "deleteLeadershipByCaseRoleCode";
        public static string GetMostRecentUpdateInCaseIntake = "getMostRecentUpdateInCaseIntake";
        public static string GetExpertiseRequirementList = "getExpertiseRequirementList";
        public static string UpsertExpertiseRequirementList = "upsertExpertiseRequirementList";
        public static string UpsertCaseIntakeAlertsForEmployees = "upsertCaseIntakeAlertsForEmployees";
        public static string GetCaseIntakeAlert = "getCaseIntakeAlert";
        public static string GetEmployeesForCaseIntakeAlert = "GetEmployeesForCaseIntakeAlert";
        public static string GetPlacesForTypeAhead = "getPlacesTypeAhead";
    }
}
