using System;

namespace Staffing.Coveo.API.Models
{
    public class Transaction
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeStatus { get; set; }
        public string BusinessProcessEvent { get; set; }
        public string BusinessProcessReason { get; set; }
        public string BusinessProcessType { get; set; }
        public string BusinessProcessName { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public DateTime? MostRecentCorrectionDate { get; set; }
        public DateTime? TerminationEffectiveDate { get; set; }
        public string TransactionStatus { get; set; }
        public decimal? BillCodeCurrent { get; set; }
        public decimal? BillCodeProposed { get; set; }
        public decimal? FteCurrent { get; set; }
        public decimal? FteProposed { get; set; }
        public short? HomeOfficeCodeCurrent { get; set; }
        public short? HomeOfficeCodeProposed { get; set; }
        public short? OperatingOfficeCodeCurrent { get; set; }
        public short? OperatingOfficeCodeProposed { get; set; }
        public string PDGradeCurrent { get; set; }
        public string PDGradeProposed { get; set; }
        public string PositionNameCurrent { get; set; }
        public string PositionNameProposed { get; set; }
        public string PositionGroupNameCurrent { get; set; }
        public string PositionGroupNameProposed { get; set; }
        public string ServiceLineCodeCurrent { get; set; }
        public string ServiceLineCodeProposed { get; set; }
        public string ServiceLineNameCurrent { get; set; }
        public string ServiceLineNameProposed { get; set; }
        public DateTime? TransitionStartDate { get; set; }
        public DateTime? TransitionEndDate { get; set; }
        public string CostCentreIdCurrent { get; set; }
        public string CostCentreIdProposed { get; set; }
        public string TargetLastUpdated { get; set; }

    }
}
