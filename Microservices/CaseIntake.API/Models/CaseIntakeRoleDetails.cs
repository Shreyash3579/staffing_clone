using System;

namespace CaseIntake.API.Models
{
    public class CaseIntakeRoleDetails
    {
        public Guid? Id { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? OpportunityId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public Guid? WorkstreamId { get; set; }
        public string Name { get; set; }
        public string PositionCode { get; set; }
        public string ExpertiseRequirementCodes { get; set; }
        public string MustHaveExpertiseCodes { get; set; }
        public string NiceToHaveExpertiseCodes { get; set; }
        public string OfficeCodes { get; set; }
        public string LanguageCodes { get; set; }
        public string MustHaveLanguageCodes { get; set; }
        public string NiceToHaveLanguageCodes { get; set; }
        public string ClientEngagementModel { get; set; }
        public string ClientEngagementModelCodes { get; set; }
        public string RoleDescription { get; set; }
        public string ServiceLineCode { get; set; }
        public bool IsLead { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedByName { get; set; }
    }

}


