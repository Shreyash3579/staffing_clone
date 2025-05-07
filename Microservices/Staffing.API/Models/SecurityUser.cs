using System;

namespace Staffing.API.Models
{
    public class SecurityUser
    {
        public string EmployeeCode { get; set; }
        public string RoleCodes { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsAdmin { get; set; }
        public bool Override { get; set; }
        public DateTime? EndDate { get; set; }
        public string Notes { get; set; }
        public string UserTypeCode { get; set; }
        public string GeoType { get; set; }
        public string OfficeCodes { get; set; }
        public string ServiceLineCodes { get; set; }
        public string PositionGroupCodes { get; set; }
        public string LevelGrades { get; set; }
        public string PracticeAreaCodes { get; set; }
        public string RingfenceCodes { get; set; }
        public bool HasAccessToAISearch { get; set; }
        public bool HasAccessToStaffingInsightsTool { get; set; }

        public bool HasAccessToRetiredStaffingTab { get; set; }
    }
}
