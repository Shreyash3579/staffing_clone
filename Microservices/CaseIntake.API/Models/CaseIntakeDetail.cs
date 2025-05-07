using System;

namespace CaseIntake.API.Models
{
    public class CaseIntakeDetail
    {
        public string OfficeCodes { get; set; }
        public string OfficeNames { get; set; }
        public string ClientEngagementModel { get; set; }
        public string ClientEngagementModelCodes { get; set; }
        public string CaseDescription { get; set; }
        public string ExpertiseRequirement { get; set; }
        public string Languages { get; set; }
        public string ReadyToStaffNotes { get; set; }
        public string BackgroundCheckNotes { get; set; }
        public string IndustryPracticeAreaCodes { get; set; }
        public string CapabilityPracticeAreaCodes { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? OpportunityId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedByName { get; set; }
    }
}