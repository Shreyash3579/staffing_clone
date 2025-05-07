using System;

namespace Staffing.HttpAggregator.Models
{
    public class PegOpportunityMap
    {
        public string OpportunityId { get; set; }
        public string OldCaseCode { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
