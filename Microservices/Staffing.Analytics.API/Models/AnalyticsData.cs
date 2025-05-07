using System;

namespace Staffing.Analytics.API.Models
{
    public class AnalyticsData
    {
        public string Action { get; set; }
        public string SourceTable { get; set; }
        public string CapacityCategory { get; set; }
        public string CapacitySubCateogry { get; set; }
        public Guid Id { get; set; }
        public Guid? ScheduleId { get; set; }
        public string OldCaseCode { get; set; }
        public string CaseName { get; set; }
        public string ClientName { get; set; }
        public string CaseTypeName { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string CaseOfficeAbbreviation { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeStatusName { get; set; }
        public decimal Fte { get; set; }
        public string CurrentLevelGrade { get; set; }
        public decimal Allocation { get; set; }
        public DateTime Date { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public string InvestmentName { get; set; }
        public string operatingOfficeAbbreviation { get; set; }
        public int? OperatingofficeCode { get; set; }
        public decimal? BillCode { get; set; }
        public decimal? ActualCost { get; set; }
        public bool IncludeInCost { get; set; }
        public string reportTypeName { get; set; }
        public DateTime LastUpdated { get; set; }

    }
}
