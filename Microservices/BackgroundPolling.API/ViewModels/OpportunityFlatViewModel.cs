using System;

namespace BackgroundPolling.API.ViewModels
{
    public class OpportunityFlatViewModel
    {
        public Guid PipelineId { get; set; }
        public string CoordinatingPartnerCode { get; set; }
        public string PrimaryPartnerCode { get; set; }
        public string OpportunityName { get; set; }
        public short OpportunityStatusCode { get; set; }
        public string OpportunityStatusName { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Duration { get; set; }
        public int? ProbabilityPercent { get; set; }
        public short? ManagingOfficeCode { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }
        public int? IndustryPracticeAreaCode { get; set; }
        public string IndustryPracticeArea { get; set; }
        public int? CapabilityPracticeAreaCode { get; set; }
        public string CapabilityPracticeArea { get; set; }
        public string PrimaryIndustry { get; set; }
        public string PrimaryCapability { get; set; }
        public string CaseAttributes { get; set; }
        public string ClientPriority { get; set; }
        public short? ClientPrioritySortOrder { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }

    }
}
