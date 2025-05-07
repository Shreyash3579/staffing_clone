using System;

namespace BackgroundPolling.API.Models
{
    public class RevenueTransaction
    {
        public Guid RevId { get; set; }
        public int ClientId { get; set; }
        public int? CaseId { get; set; }
        public Guid? PipelineId { get; set; }
        public string ServiceLineCode { get; set; }
        public string TransactionType { get; set; }
        public string TransferType { get; set; }
        public int ServedOfficeCode { get; set; }
        public DateTime FeeDate { get; set; }
        public decimal Probability { get; set; }
        public string TransactionCurrency { get; set; }
        public decimal transactionAmount { get; set; }
        public bool IsFinancial { get; set; }
        public bool IsMgmtActivity { get; set; }
        public bool IsMgmtCash { get; set; }
        public string AtRiskType { get; set; }
        public string RevenueDetail { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public string DeletedBy { get; set; }
        public string DeleteDateTime { get; set; }
    }
}
