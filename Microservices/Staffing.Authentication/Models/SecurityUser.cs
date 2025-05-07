namespace Staffing.Authentication.Models
{
    public class SecurityUser
    {
        public string EmployeeCode { get; set; }
        public bool IsAdmin { get; set; }
        public bool Override { get; set; }
        public string Roles { get; set; }
        public string FeatureName { get; set; }
        public string AccessTypeName { get; set; }
        public string OfficeCodes { get; set; }
        public string DemandTypes { get; set; }
        public bool HasAccessToAISearch { get; set; }
        public bool HasAccessToStaffingInsightsTool { get; set; }

        public bool HasAccessToRetiredStaffingTab { get; set; }
    }
}
