using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class CasePlanningBoardViewModel
    {
        public Guid? PlanningBoardId { get; set; }
        public int? BucketId { get; set; }
        public string BucketName { get; set; }
        public DateTime? Date { get; set; }
        public bool? IncludeInDemand { get; set; }
        public int? CaseCode { get; set; }
        public string CaseName { get; set; }
        public int? ClientCode { get; set; }
        public string ClientName { get; set; }
        public string OldCaseCode { get; set; }
        public int? CaseTypeCode { get; set; }
        public string CaseType { get; set; }
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
        public DateTime? OriginalStartDate { get; set; }
        public DateTime? OverrideStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? OriginalEndDate { get; set; }
        public DateTime? OverrideEndDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Type { get; set; }
        public CaseRoll CaseRoll { get; set; }
        public bool? IsPrivateEquity { get; set; }
        public string CaseAttributes { get; set; }
        public bool? CaseServedByRingfence { get; set; }
        public string CaseManagerCode { get; set; }
        public string CaseManagerName { get; set; }
        public string CaseManagerOfficeAbbreviation { get; set; }
        public string Notes { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> AllocatedResources { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> PlaceholderAllocations { get; set; }
        public IEnumerable<SKUDemand> SKUTerm { get; set; }
        public string CombinedSkuTerm { get; set; }
        public Guid? PipelineId { get; set; }
        public string EstimatedTeamSize { get; set; }
        public string CoordinatingPartnerCode { get; set; }
        public string CoordinatingPartnerName { get; set; }
        public string BillingPartnerCode { get; set; }
        public string BillingPartnerName { get; set; }
        public string OtherPartnersCodes { get; set; }
        public string OpportunityName { get; set; }
        public string OtherOfficeAbbreviation { get; set; }
        public int? OtherOfficeCode { get; set; }
        public string Duration { get; set; }
        public int? OriginalProbabilityPercent { get; set; }
        public int? OverrideProbabilityPercent { get; set; }
        public int? ProbabilityPercent { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string Name { get; set; }
        public int? Office { get; set; }
        public bool? IsShared { get; set; }
        public string SharedOfficeCodes { get; set; }
        public string SharedOfficeAbbreviations { get; set; }
        public string SharedStaffingTags { get; set; }
        public string PegOpportunityId { get; set; }
        public string ClientPriority { get; set; }
        public short? ClientPrioritySortOrder { get; set; }
        public bool isStartDateUpdatedInBOSS { get; set; }
        public bool isEndDateUpdatedInBOSS { get; set; }
        public short? OriginalStaffingOfficeCode { get; set; }
        public short? StaffingOfficeCode { get; set; }
        public string OriginalStaffingOfficeAbbreviation { get; set; }
        public string StaffingOfficeAbbreviation { get; set; }
        public CaseViewNoteViewModel LatestCasePlanningBoardViewNote { get; set; }
        public bool? IsPlaceholderCreatedFromCortex { get; set; }
    }
}
