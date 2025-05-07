using System;

namespace Staffing.HttpAggregator.Models
{
    public class PreponedCasesAllocationsAudit
    {
        public Guid? Id { get; set; }
        public int CaseCode { get; set; }
        public int ClientCode { get; set; }
        public string OldCaseCode { get; set; }
        public DateTime? OriginalCaseStartDate { get; set; }
        public DateTime? UpdatedCaseStartDate { get; set; }
        public DateTime? OriginalCaseEndDate { get; set; }
        public DateTime? UpdatedCaseEndDate { get; set; }
        public string EmployeeCode { get; set; }
        public string ServiceLineCode { get; set; }
        public short OperatingOfficeCode { get; set; }
        public string CaseLastUpdatedBy { get; set; }
        public DateTime CaseLastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
