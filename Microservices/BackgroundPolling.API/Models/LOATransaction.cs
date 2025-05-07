using System;

namespace BackgroundPolling.API.Models
{
    public class LOATransaction
    {
        public string Id { get; set; }
        public string BusinessProcessEvent { get; set; }
        public string BusinessProcessReason { get; set; }
        public string BusinessProcessType { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string EmployeeStatus { get; set; }
        public DateTime? MostRecentCorrectionDate { get; set; }
        public DateTime? TerminationStatusEffectiveDate { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string TransactionStatus { get; set; }
        public LOATransactionProcess Transaction { get; set; }
    }
}
