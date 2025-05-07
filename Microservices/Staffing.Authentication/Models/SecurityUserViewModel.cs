using System.Collections.Generic;


namespace Staffing.Authentication.Models
{
    public class SecurityUserViewModel
    {
        public string EmployeeCode { get; set; }
        public bool IsAdmin { get; set; }
        public bool Override { get; set; }
        public string[] RoleNames { get; set; }
        public IList<FeatureAccess> Features { get; set; }
        public int[] OfficeCodes { get; set; }
        public string[] DemandTypes { get; set; }
        public bool HasAccessToAISearch { get; set; }
        public bool HasAccessToStaffingInsightsTool { get; set; }

        public bool HasAccessToRetiredStaffingTab { get; set; }
    }
}
