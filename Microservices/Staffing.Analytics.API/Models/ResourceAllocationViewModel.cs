using System;

namespace Staffing.Analytics.API.Models
{
    public class ResourceAllocationViewModel
    {
        public Guid? Id { get; set; }
        public int? CaseCode { get; set; }
        public int? ClientCode { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public string EmployeeCode { get; set; }
        public string CurrentLevelGrade { get; set; }
        public int? OperatingOfficeCode { get; set; }
        public string ServiceLineCode { get; set; }
        public int? Allocation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? InvestmentCode { get; set; }
        public string InvestmentName { get; set; }
        public string CaseRoleCode { get; set; }
        public string CaseRoleName { get; set; }
        public string PositionGroupCode { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Notes { get; set; }
       
    }
}
