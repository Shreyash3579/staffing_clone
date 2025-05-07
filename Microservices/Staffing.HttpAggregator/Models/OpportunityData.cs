using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    public class OpportunityData
    {
        public Guid? PipelineId { get; set; }
        public string CortexId { get; set; }
        public string EstimatedTeamSize { get; set; }
        public string PricingTeamSize { get; set; }
        public string CoordinatingPartnerCode { get; set; }
        public string CoordinatingPartnerName { get; set; }
        public string BillingPartnerCode { get; set; }
        public string BillingPartnerName { get; set; }
        public string OtherPartnersCodes { get; set; }
        public string OpportunityName { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public int? BillingOfficeCode { get; set; }
        public string OtherOfficeAbbreviation { get; set; }
        public int? OtherOfficeCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? OriginalStartDate { get; set; }
        public DateTime? OverrideStartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? OriginalEndDate { get; set; }
        public DateTime? OverrideEndDate { get; set; }
        public string Duration { get; set; }
        public int? ProbabilityPercent { get; set; }
        public int? OriginalProbabilityPercent { get; set; }
        public int? OverrideProbabilityPercent { get; set; }
        public string PrimaryIndustry { get; set; }
        public string PrimaryCapability { get; set; }
        public string Type { get; set; }
        public string CaseAttributes { get; set; }
        public string Notes { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> AllocatedResources { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> PlaceholderAllocations { get; set; }
        public SKUCaseTermsViewModel SKUCaseTerms { get; set; } // TODO: remove this once new logic for SKU is implemented
        public IEnumerable<SKUDemand> SKUTerm { get; set; }
        public string ClientPriority { get; set; }
        public short? ClientPrioritySortOrder { get; set; }
        public bool isStartDateUpdatedInBOSS { get; set; }
        public bool isEndDateUpdatedInBOSS { get; set; }
        public short? StaffingOfficeCode { get; set; }
        public string StaffingOfficeAbbreviation { get; set; }
        public int? IndustryPracticeAreaCode { get; set; }
        public string IndustryPracticeArea { get; set; }
        public int? CapabilityPracticeAreaCode { get; set; }
        public string CapabilityPracticeArea { get; set; }
        public IEnumerable<CaseViewNote> CasePlanningViewNotes { get; set; }
        public bool? CaseServedByRingfence { get; set; }
        public bool? IsPlaceholderCreatedFromCortex { get; set; }
        public bool isSTACommitmentCreated { get; set; }
    }
}
