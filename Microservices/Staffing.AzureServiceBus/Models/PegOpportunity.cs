using System;

namespace Staffing.AzureServiceBus.Models
{
    public class PegOpportunity
    {
        public string OpportunityId { get; set; }
        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string OfficeCodes { get; set; }
        public int? ProbabilityPercent { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public Guid? PlanningCardId { get; set; }
    }
}
