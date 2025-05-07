using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    /// <summary>
    /// Project contains aggregation of case and opportunity data
    /// </summary>
    public class ProjectData
    {
        // Opportunity specific properties
        public Guid? PipelineId { get; set; }
        public string CortexId { get; set; }
        public string EstimatedTeamSize { get; set; }
        public string PricingTeamSize { get; set; }
        public string CoordinatingPartnerCode { get; set; }
        public string CoordinatingPartnerName { get; set; }
        public string BillingPartnerCode { get; set; }
        public string BillingPartnerName { get; set; }
        public string OpportunityName { get; set; }
        public int? OriginalProbabilityPercent { get; set; }
        public int? OverrideProbabilityPercent { get; set; }
        public int? ProbabilityPercent { get; set; }
        public bool isStartDateUpdatedInBOSS { get; set; }
        public bool isEndDateUpdatedInBOSS { get; set; }

        // case specific properties
        public int CaseCode { get; set; }
        public string OldCaseCode { get; set; }
        public string CaseName { get; set; }
        public int? CaseTypeCode { get; set; }
        public string CaseType { get; set; }
        public int ClientCode { get; set; }
        public CaseRoll CaseRoll { get; set; }
        public bool IsPrivateEquity { get; set; }
        public string CaseAttributes { get; set; }
        public bool? IncludeInDemand { get; set; }
        public bool? IsFlagged { get; set; }
        public bool? CaseServedByRingfence { get; set; }
        public string CaseManagerCode { get; set; }
        public string CaseManagerFullName { get; set; }
        public string CaseManagerOfficeAbbreviation { get; set; }

        public string PegOpportunityId { get; set; }

        // common properties
        public string ClientName { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }
        public int? BillingOfficeCode { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string BillingOfficeName { get; set; }
        public DateTime? OriginalStartDate { get; set; }
        public DateTime? OverrideStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? OriginalEndDate { get; set; }
        public DateTime? OverrideEndDate { get; set; }
        public string Type { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> AllocatedResources { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> PlaceholderAllocations { get; set; }
        public IEnumerable<SKUDemand> SkuTerm { get; set; }
        public string CombinedSkuTerm {get; set;}
        public SKUCaseTermsViewModel SkuCaseTerms { get; set; } //TODO: remove once new SKu logic is implemented
        public string ProjectStatus { get; set; }
        public string Notes { get; set; }
        public string ClientPriority { get; set; }
        public short? ClientPrioritySortOrder { get; set; }
        public short? StaffingOfficeCode { get; set; }
        public string StaffingOfficeAbbreviation { get; set; }
        public int? IndustryPracticeAreaCode { get; set; }
        public string IndustryPracticeArea { get; set; }
        public int? CapabilityPracticeAreaCode { get; set; }
        public string CapabilityPracticeArea { get; set; }
        public IEnumerable<CaseViewNoteViewModel> CasePlanningViewNotes { get; set; }
        public bool? IsPlaceholderCreatedFromCortex { get; set; }

        public bool isSTACommitmentCreated { get; set; }
    }
}
