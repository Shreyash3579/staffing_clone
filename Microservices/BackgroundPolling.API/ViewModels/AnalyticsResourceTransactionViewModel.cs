using BackgroundPolling.API.Core.Helpers;
using System;

namespace BackgroundPolling.API.ViewModels
{
    public class AnalyticsResourceTransactionViewModel
    {
        public string EmployeeCode { get; set; }
        public Constants.EmployeeStatus EmployeeStatusCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CurrentLevelGrade { get; set; }
        public decimal FTE { get; set; }
        public int OperatingOfficeCode { get; set; }
        public string OperatingOfficeName { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public string PositionGroupName { get; set; }
        public string Position { get; set; } // TODO: Delete once user shifted to use posiitionGroupName
        public decimal? ActualCost { get; set; }
        public decimal? EffectiveCost { get; set; } //TODO : Remove once user start using actual cost and employeeStatusCode
        public decimal? BillRate { get; set; }
        public decimal BillCode { get; set; }
        public string BillRateType { get; set; }
        public string BillRateCurrency { get; set; }
        public string EffectiveCostReason { get; set; } //TODO : Remove once user start using actual cost and employeeStatusCode
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
