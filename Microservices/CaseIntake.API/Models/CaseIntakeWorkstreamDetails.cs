using System;

namespace CaseIntake.API.Models
{
    public class CaseIntakeWorkstreamDetails
    {
        public Guid Id { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? OpportunityId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string Name { get; set; }
        public string SkuSize { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedByName { get; set; }

    }
}
