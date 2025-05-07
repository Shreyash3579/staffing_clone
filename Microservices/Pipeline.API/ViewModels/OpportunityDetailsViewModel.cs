using System;

namespace Pipeline.API.ViewModels
{
    public class OpportunityDetailsViewModel
    {
        public Guid PipelineId { get; set; }
        public string CortexOpportunityId { get; set; }
        public string EstimatedTeamSize { get; set; }
        public string CoordinatingPartnerCode { get; set; }
        public string BillingPartnerCode { get; set; }
        public string OtherPartnersCodes { get; set; }
        public string OpportunityName { get; set; }
        public string OpportunityStatus { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientGroupCode { get; set; }
        public string ClientGroupName { get; set; }
        public string PrimaryIndustry { get; set; }
        public string IndustryPracticeArea { get; set; }
        public string PrimaryCapability { get; set; }
        public string CapabilityPracticeArea { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Duration { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string CaseAttributes { get; set; }
    }
}
