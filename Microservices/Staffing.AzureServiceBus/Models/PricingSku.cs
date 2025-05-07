using System;

namespace Staffing.AzureServiceBus.Models
{
    public class PricingSku
    {
        public Guid? Id { get; set; }
        public Guid? PipelineId { get; set; }
        public string CortexOpportunityId { get; set; }
        public string PricingTeamSize { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
