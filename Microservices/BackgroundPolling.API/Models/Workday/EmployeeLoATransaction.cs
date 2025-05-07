using System;

namespace BackgroundPolling.API.Models.Workday
{
    public class EmployeeLoATransaction
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
        public LoATransactionProcess Transaction { get; set; }
    }

    public class LoATransactionProcess
    {
        public DateTime? LastDayOfWork { get; set; }
        public DateTime? FirstDayOfLeave { get; set; }
        public DateTime? EstimatedLastDayOfLeave { get; set; }
        public DateTime? ActualLastDayOfLeave { get; set; }
        public DateTime? FirstDayBackAtWork { get; set; }
        public DateTime? FirstDayAtWork { get; set; }
    }
}
