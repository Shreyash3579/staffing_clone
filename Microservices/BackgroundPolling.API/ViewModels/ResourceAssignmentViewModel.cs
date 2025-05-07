using BackgroundPolling.API.Core.Helpers;
using System;

namespace BackgroundPolling.API.ViewModels
{
    public class ResourceAssignmentViewModel
    {
        public Guid? Id { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string OldCaseCode { get; set; }
        public string CaseName { get; set; }
        public Guid? PipelineId { get; set; }
        public string OpportunityName { get; set; }
        public string ClientName { get; set; }
        public int? CaseTypeCode { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string InternetAddress { get; set; }
        public string CurrentLevelGrade { get; set; }
        public int? OperatingOfficeCode { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public int? Allocation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public String ServiceLineCode { get; set; }
        public String ServiceLineName { get; set; }
        public int? InvestmentCode { get; set; }
        public string CaseRoleCode { get; set; }
        public string PrimaryIndustry { get; set; }
        public string PrimaryCapability { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? CaseStartDate { get; set; }
        public DateTime? CaseEndDate { get; set; }
        public DateTime? OpportunityStartDate { get; set; }
        public DateTime? OpportunityEndDate { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string Notes { get; set; }
        public DateTime? TerminationDate { get; set; }
        public bool? IsPlaceholderAllocation { get; set; }
        public bool? isConfirmed { get; set; }
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeName { get; set; }
        public int? CaseCode { get; set; }
        public int ClientCode { get; set; }
        public decimal Fte { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public int? BillingOfficeCode { get; set; }
        public DateTime HireDate { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? EffectiveCost { get; set; }
        public decimal? BillRate { get; set; }
        public decimal BillCode { get; set; }
        public decimal OpportunityCost { get; set; }
        public decimal EffectiveOpportunityCost { get; set; }
        public int Availability { get; set; }
        public int EffectiveAvailability { get; set; }
        public DateTime LastUpdated { get; set; }

        //This does a shallow copy ONLY
        public ResourceAssignmentViewModel Clone()
        {
            return (ResourceAssignmentViewModel)MemberwiseClone();
        }
    }
}
