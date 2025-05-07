using System;

namespace BackgroundPolling.API.Models
{
    public class CaseOppCortexTeamSize
    {
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public string cortexOpportunityId { get; set; }
        public string EstimatedTeamSize { get; set; }
        public string PricingTeamSize { get; set; }
        public bool? IsPlaceholderCreatedFromCortex { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
