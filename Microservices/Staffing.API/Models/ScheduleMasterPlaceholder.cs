using System;

namespace Staffing.API.Models
{
    public class ScheduleMasterPlaceholder
    {
        public Guid? Id { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string PlanningCardTitle { get; set; }
        public int? ClientCode { get; set; }
        public int? CaseCode { get; set; }
        public string OldCaseCode { get; set; }
        public string EmployeeCode { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public short? OperatingOfficeCode { get; set; }
        public string CurrentLevelGrade { get; set; }
        public short? Allocation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PipelineId { get; set; }
        public short? InvestmentCode { get; set; }
        public string CaseRoleCode { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Notes { get; set; }
        public bool? IsPlaceholderAllocation { get; set; }
        public bool? IsPlanningCardShared { get; set; }
        public string CaseName { get; set; }
        public string ClientName { get; set; }
        public string OpportunityName { get; set; }
        public int? CaseTypeCode { get; set; }
        public string CaseTypeName { get; set; }
        public string EmployeeName { get; set; }
        public string OperatingOfficeName { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string ManagingOfficeName { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public int? BillingOfficeCode { get; set; }
        public string BillingOfficeName { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string InvestmentName { get; set; }
        public string CaseRoleName { get; set; }
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeName { get; set; }
        public bool? IsConfirmed { get; set; }
        public string PositionGroupCode { get; set; }
        public bool? IncludeInCapacityReporting { get; set; }

        //This does a shallow copy ONLY
        public ScheduleMasterPlaceholder Clone()
        {
            return (ScheduleMasterPlaceholder)this.MemberwiseClone();
        }
    }
}
