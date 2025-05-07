using System;

namespace Staffing.AzureServiceBus.Models
{
    public class PlanningCard
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool? IsShared { get; set; }
        public string SharedOfficeCodes { get; set; }
        public string SharedStaffingTags { get; set; }
        public bool? IncludeInCapacityReporting { get; set; }
        public string CreatedBy { get; set; }
        public string PegOpportunityId { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string Source { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? SourceLastUpdated {get; set;}

        //This does a shallow copy ONLY
        public PlanningCard Clone()
        {
            return (PlanningCard)this.MemberwiseClone();
        }
    }
}
