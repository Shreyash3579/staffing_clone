using System;

namespace Staffing.Analytics.API.Models
{
    public class CasePlanningBoardPlaygroundFilters
    {
        public Guid? PlaygroundId { get; set; }
        public DateTime SupplyStartDate { get; set; }
        public DateTime DemandStartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SupplyViewOfficeCodes { get; set; }
        public string SupplyViewStaffingTags { get; set; }
        public string LevelGrades { get; set; }
        public string PositionCodes { get; set; }
        public string PracticeAreaCodes { get; set; }
        public string AffiliationRoleCodes { get; set; }
        public string DemandViewOfficeCodes { get; set; }
        public string CaseAttributeNames { get; set; }
        public string CaseTypeCodes { get; set; }
        public string DemandTypes { get; set; }
        public string OpportunityStatusTypeCodes { get; set; }
        public short MinOpportunityProbability { get; set; }
        public string IndustryPracticeAreaCodes { get; set; }
        public string CapabilityPracticeAreaCodes { get; set; }
        public bool IsCountOfIndividualResourcesToggle { get; set; }
        public bool EnableMemberGrouping { get; set; }
        public bool EnableNewlyAvailableHighlighting { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
