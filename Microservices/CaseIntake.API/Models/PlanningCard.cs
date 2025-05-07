using System;

namespace CaseIntake.API.Models
{
    public class PlanningCard
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Office { get; set; }
        public bool? IsShared { get; set; }
        public string SharedOfficeCodes { get; set; }
        public string SharedStaffingTags { get; set; }
        public bool? IncludeInCapacityReporting { get; set; }
        public string PegOpportunityId { get; set; }
        public string MergedCaseCode { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public bool? IsMerged { get; set; }
        public bool? IsSyncedWithPeg { get; set; }
    }
}
