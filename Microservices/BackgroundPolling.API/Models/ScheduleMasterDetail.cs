using BackgroundPolling.API.Core.Helpers;
using System;

namespace BackgroundPolling.API.Models
{
    public class ScheduleMasterDetail
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public Constants.EmployeeStatus EmployeeStatusCode { get; set; } = Constants.EmployeeStatus.Active;
        public decimal Fte { get; set; }
        public int OperatingOfficeCode { get; set; }
        public string OperatingOfficeName { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public string CurrentLevelGrade { get; set; }
        public int Allocation { get; set; }
        public DateTime Date { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public string PositionGroupName { get; set; }
        public string Position { get; set; } // TODO: Delete once user shifted to use posiitionGroupName
        public int? InvestmentCode { get; set; }
        public string CaseRoleCode { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? CostInUSD { get; set; }
        public int? CostUSDEffectiveYear { get; set; }
        public decimal? UsdRate { get; set; }
        public decimal? EffectiveCost { get; set; }
        public string EffectiveCostReason { get; set; }
        public decimal? BillRate { get; set; }
        public decimal BillCode { get; set; }
        public string BillRateType { get; set; }
        public string BillRateCurrency { get; set; }
        public string TransactionType { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
