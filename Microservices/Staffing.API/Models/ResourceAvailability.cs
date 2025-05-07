using Staffing.API.Core.Helpers;
using System;

namespace Staffing.API.Models
{
    public class ResourceAvailability
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public Constants.EmployeeStatus EmployeeStatusCode { get; set; } = Constants.EmployeeStatus.Active;
        public decimal Fte { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public string Position { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public string PositionGroupName { get; set; }
        public String CurrentLevelGrade { get; set; }
        public int OperatingOfficeCode { get; set; }
        public string OperatingOfficeName { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public int Availability { get; set; }
        public int EffectiveAvailability { get; set; }
        public string EffectiveAvailabilityReason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal OpportunityCost { get; set; }
        public decimal EffectiveOpportunityCost { get; set; }
        public string EffectiveOpportunityCostReason { get; set; }
        public decimal? BillRate { get; set; }
        public decimal BillCode { get; set; }
        public string BillRateType { get; set; }
        public string BillRateCurrency { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
