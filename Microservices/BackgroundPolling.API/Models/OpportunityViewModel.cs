using System;

namespace BackgroundPolling.API.Models
{
    public class OpportunityViewModel
    {
        public Guid PipelineId { get; set; }
        public string CoordinatingPartnerCode { get; set; }
        public string BillingPartnerCode { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string OpportunityName { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Duration { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string PrimaryIndustry { get; set; }
        public string PrimaryCapability { get; set; }
        public string CaseAttributes { get; set; }
        public string CortexOpportunityId { get; set; }
        public string EstimatedTeamSize { get; set; }
        public bool? CaseServedByRingfence { get; set; }
    }
}
