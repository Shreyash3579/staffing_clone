using Staffing.API.Core.Helpers;
using System;

namespace Staffing.API.Models
{
    public class ResourceAllocation
    {
        public string Action { get; set; }
        public Guid Id { get; set; }
        public string OldCaseCode { get; set; }
        public int? CaseCode { get; set; }
        public string CaseName { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public int? CaseTypeCode { get; set; }
        public string CaseTypeName { get; set; }
        public Guid? PipelineId { get; set; }
        public string OpportunityName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public Constants.EmployeeStatus EmployeeStatusCode { get; set; } = Constants.EmployeeStatus.Active;
        public decimal Fte { get; set; }
        public String ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public string Position { get; set; }
        public string PositionName { get; set; }
        public string PositionGroupName { get; set; }
        public string PositionCode { get; set; }
        public String CurrentLevelGrade { get; set; }
        public int OperatingOfficeCode { get; set; }
        public string OperatingOfficeName { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string ManagingOfficeName { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public int? BillingOfficeCode { get; set; }
        public string BillingOfficeName { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public int Allocation { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? InvestmentCode { get; set; }
        public string InvestmentName { get; set; }
        public string CaseRoleCode { get; set; }
        public string CaseRoleName { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? EffectiveCost { get; set; }
        public string EffectiveCostReason { get; set; }
        public decimal? BillRate { get; set; }
        public decimal BillCode { get; set; }
        public string BillRateType { get; set; }
        public string BillRateCurrency { get; set; }
        public decimal OpportunityCost { get; set; }
        public decimal EffectiveOpportunityCost { get; set; }
        public string EffectiveOpportunityCostReason { get; set; }
        public int Availability { get; set; }
        public int EffectiveAvailability { get; set; }
        public string EffectiveAvailabilityReason { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TransactionType { get; set; }
        public string InternetAddress { get; set; }
        public string Notes { get; set; }
        public bool? IsPlaceholderAllocation { get; set; }
        public int ResourceCount { get; set; }
    }
}
