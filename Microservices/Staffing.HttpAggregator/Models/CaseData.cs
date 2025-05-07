using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    public class CaseData
    {
        public int CaseCode { get; set; }
        public string CaseName { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string OldCaseCode { get; set; }
        public int? CaseTypeCode { get; set; }
        public string CaseType { get; set; }
        public string EstimatedTeamSize { get; set; }
        public bool? IsPlaceholderCreatedFromCortex { get; set; }
        public int? IndustryPracticeAreaCode { get; set; }
        public string IndustryPracticeArea { get; set; }
        public string PrimaryIndustry { get; set; }
        public int? CapabilityPracticeAreaCode { get; set; }
        public string CapabilityPracticeArea { get; set; }
        public string PrimaryCapability { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }
        public int? BillingOfficeCode { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string BillingOfficeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; }
        public CaseRoll CaseRoll { get; set; }

        public bool isSTACommitmentCreated { get; set; }
        public bool IsPrivateEquity { get; set; }
        public string CaseAttributes { get; set; }
        public bool? CaseServedByRingfence { get; set; }
        public string CaseManagerName { get; set; }
        public string CaseManagerCode { get; set; }
        public string CaseManagerOfficeAbbreviation { get; set; }
        public string Notes { get; set; }
        public IEnumerable<CaseViewNote> CasePlanningViewNotes { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> AllocatedResources { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> PlaceholderAllocations { get; set; }
        public IEnumerable<SKUDemand> SkuTerm { get; set; }
        public SKUCaseTermsViewModel SKUCaseTerms { get; set; } //TODO: remove the following line once new SKU logic is implemented
        public string ClientPriority { get; set; }
        public short? ClientPrioritySortOrder { get; set; }
        public short? OriginalStaffingOfficeCode { get; set; }
        public short? StaffingOfficeCode { get; set; }
        public string? OriginalStaffingOfficeAbbreviation { get; set; }
        public string StaffingOfficeAbbreviation { get; set; }
        public string PegIndustryTerm { get; set; }

        public string PegOpportunityId { get; set; }
    }
}
