using System;

namespace Staffing.Analytics.API.Models
{
    public class CasePlanningBoardPlaygroundAllocation
    {
        public string EmployeeCode { get; set; }
        public DateTime? NewStartDate { get; set; }
        public DateTime? NewEndDate { get; set; }
        public short? NewAllocation { get; set; }
        public short? NewInvestmentCode { get; set; }
        public DateTime? PreviousStartDate { get; set; }
        public DateTime? PreviousEndDate { get; set; }
        public short? PreviousAllocation { get; set; }
        public short? PreviousInvestmentCode { get; set; }
        public bool IsOpportunity { get; set; }
        public DateTime? CaseEndDate { get; set; }
    }
}
