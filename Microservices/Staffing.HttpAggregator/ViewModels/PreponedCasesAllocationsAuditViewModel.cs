using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class PreponedCasesAllocationsAuditViewModel
    {
        public Guid? Id { get; set; }
        public int CaseCode { get; set; }
        public int ClientCode { get; set; }
        public string OldCaseCode { get; set; }
        public string CaseName { get; set; }
        public string ClientName { get; set; }
        public DateTime? OriginalCaseStartDate { get; set; }
        public DateTime? UpdatedCaseStartDate { get; set; }
        public DateTime? OriginalCaseEndDate { get; set; }
        public DateTime? UpdatedCaseEndDate { get; set; }
        public string CaseLastUpdatedBy { get; set; }
        public string CaseLastUpdatedByName { get; set; }
        public DateTime CaseLastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string EmployeeCodes { get; set; }
        public string EmployeeNames { get; set; }
    }
}
