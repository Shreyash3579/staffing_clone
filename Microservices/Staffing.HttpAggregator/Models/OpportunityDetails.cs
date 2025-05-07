using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    public class OpportunityDetails
    {
        public Guid PipelineId { get; set; }
        public string CortexId { get; set; }
        public string EstimatedTeamSize { get; set; }
        public string PricingTeamSize { get; set; }
        public string CoordinatingPartnerCode { get; set; }
        public string CoordinatingPartnerName { get; set; }
        public string BillingPartnerCode { get; set; }
        public string BillingPartnerName { get; set; }
        public string OtherPartnersCodes { get; set; }
        public string OtherPartnersNamesWithOfficeAbbreviations { get; set; }
        public string OpportunityName { get; set; }
        public string OpportunityStatus { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string PrimaryIndustry { get; set; }
        public string IndustryPracticeArea { get; set; }
        public string PrimaryCapability { get; set; }
        public string CapabilityPracticeArea { get; set; }
        public string ManagingOfficeName { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string BillingOfficeName { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Duration { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string Type { get; set; }
        public string CaseAttributes { get; set; }
        public string Notes { get; set; }
        public bool? CaseServedByRingfence { get; set; }

        public bool isSTACommitmentCreated { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> AllocatedResources { get; set; }
    }
}
