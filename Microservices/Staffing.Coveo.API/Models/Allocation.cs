using System;

namespace Staffing.Coveo.API.Models
{
    public class Allocation
    {
        public Guid? Id { get; set; }
        public int? ClientCode { get; set; }
        public int? CaseCode { get; set; }
        public string OldCaseCode { get; set; }
        public string EmployeeCode { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public int? OperatingOfficeCode { get; set; }
        public string CurrentlevelGrade { get; set; }
        public int? AllocationPercent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PipelineId { get; set; }
        public int? InvestmentCode { get; set; }
        public string InvestmentName { get; set; }
        public string CaseRoleCode { get; set; }
        public string CaseRoleName { get; set; }
        public string Notes { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
