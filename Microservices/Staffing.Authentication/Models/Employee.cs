using System.Collections.Generic;

namespace Staffing.Authentication.Models
{
    public class Employee
    {
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string LevelName { get; set; }
        public string LevelGrade { get; set; }
        public string InternetAddress { get; set; }
        public Office Office { get; set; }
        public Office OperatingOffice { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Token { get; set; }
        public decimal FTE { get; set; }
        public bool IsAdmin { get; set; }
        public bool Override { get; set; }
        public bool HasAccessToAISearch { get; set; }
        public bool HasAccessToStaffingInsightsTool { get; set; }

        public bool HasAccessToRetiredStaffingTab { get; set; }
        public int[] HcpdOfficeCodes { get; set; }
        public string RoleName { get; set; }
        public IList<FeatureAccess> Features { get; set; }

    }
}
