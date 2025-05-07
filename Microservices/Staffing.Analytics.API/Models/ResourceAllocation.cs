using Staffing.Analytics.API.Core.Helpers;
using System;

namespace Staffing.Analytics.API.Models
{
    public class ResourceAllocation
    {
        // Common to case and opp
        public int? ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientGroupCode { get; set; }
        public string ClientGroupName { get; set; }

        // Opp related info
        public Guid? PipelineId { get; set; }
        public string OpportunityName { get; set; }

        // Case related info
        public string OldCaseCode { get; set; }
        public int? CaseCode { get; set; }
        public string CaseName { get; set; }
        public int? CaseTypeCode { get; set; }
        public string CaseTypeName { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }
        public int? BillingOfficeCode { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string BillingOfficeName { get; set; }
        public bool? PegCase { get; set; }
        public int? PrimaryIndustryTermCode { get; set; }
        public Guid? PrimaryIndustryTagId { get; set; }
        public string PrimaryIndustry { get; set; }
        public int? PracticeAreaIndustryCode { get; set; }
        public string PracticeAreaIndustry { get; set; }
        public int? PrimaryCapabilityTermCode { get; set; }
        public Guid? PrimaryCapabilityTagId { get; set; }
        public string PrimaryCapability { get; set; }
        public int? PracticeAreaCapabilityCode { get; set; }
        public string PracticeAreaCapability { get; set; }

        // Employee Related Info
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public Constants.EmployeeStatus EmployeeStatusCode { get; set; } = Constants.EmployeeStatus.Active;
        public decimal? Fte { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public string PositionName { get; set; }
        public string PositionGroupName { get; set; }
        public string PositionGroupCode { get; set; }
        public string PositionCode { get; set; }
        public string CurrentLevelGrade { get; set; }
        public int OperatingOfficeCode { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public string OperatingOfficeName { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string InternetAddress { get; set; }
        public string TransactionType { get; set; }

        // Staffing related info
        public Guid Id { get; set; }
        public int Allocation { get; set; }
        public int Availability { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? InvestmentCode { get; set; }
        public string InvestmentName { get; set; }
        public string CaseRoleCode { get; set; }
        public string CaseRoleName { get; set; }
        public string Notes { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }

        // Finance related info
        public decimal? BillRate { get; set; }
        public decimal BillCode { get; set; }
        public string BillRateType { get; set; }
        public string BillRateCurrency { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal OpportunityCost { get; set; }
        public decimal? CostInUSD { get; set; }
        public int? CostUSDEffectiveYear { get; set; }
        public decimal? UsdRate { get; set; }

        // Commitment related info
        public string PriorityCommitmentTypeCode { get; set; }
        public string PriorityCommitmentTypeName { get; set; }
        public string CommitmentTypeCodes { get; set; }
        public string CommitmentTypeReasonCode { get; set; }
        public string CommitmentTypeReasonName { get; set; }
        public string Ringfence { get; set; }
        public bool? isStaffingTag { get; set; }
        public bool? isOverriddenInSource { get; set; }

        //Placeholder Info
        public Guid? PlanningCardId { get; set; }
        public string PlanningCardName { get; set; }

        public bool? IncludeInCapacityReporting { get; set; }
        public bool? IsPlaceholderAllocation { get; set; }

        //This does a shallow copy ONLY
        public ResourceAllocation Clone()
        {
            return (ResourceAllocation)MemberwiseClone();
        }

        // TODO: Delete
        public string Position { get; set; }
        public decimal? EffectiveCost { get; set; }
        public string EffectiveCostReason { get; set; }
        public decimal EffectiveOpportunityCost { get; set; }
        public string EffectiveOpportunityCostReason { get; set; }
        public int EffectiveAvailability { get; set; }
        public string EffectiveAvailabilityReason { get; set; }

    }
}
