using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class SecurityUserDetailsViewModel
    {
        public string EmployeeCode { get; set; }
        public string RoleCodes { get; set; }
        public string FullName { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsAdmin { get; set; }
        public string JobTitle { get; set; }
        public string ServiceLine { get; set; }
        public bool IsTerminated { get; set; }
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
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }

    }
}
