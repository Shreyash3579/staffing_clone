using System;

namespace Staffing.HttpAggregator.Models.Workday
{
    public class EmployeeTransaction
    {
        public string BusinessProcessEvent { get; set; }
        public string BusinessProcessReason { get; set; }
        public string BusinessProcessType { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string EmployeeStatus { get; set; }
        public DateTime? MostRecentCorrectionDate { get; set; }
        public DateTime? TerminationEffectiveDate { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string TransactionStatus { get; set; }
        public EmployeeTransactionProcess Transaction { get; set; }
    }
}