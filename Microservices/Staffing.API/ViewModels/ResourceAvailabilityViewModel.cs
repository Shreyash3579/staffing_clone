using Staffing.API.Core.Helpers;
using System;

namespace Staffing.API.ViewModels
{
    public class ResourceAvailabilityViewModel
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
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public string PositionGroupName { get; set; }
        public string Position { get; set; } // TODO: Remove once user starts using positionGroupName
        public int Availability { get; set; }
        public int EffectiveAvailability { get; set; } // TODO: Remove once user starts using employeeStatusCode
        public string EffectiveAvailabilityReason { get; set; } // TODO: Remove once user starts using employeeStatusCode
        public decimal? BillRate { get; set; }
        public decimal BillCode { get; set; }
        public string BillRateType { get; set; }
        public string BillRateCurrency { get; set; }
        public decimal? OpportunityCost { get; set; }
        public decimal? EffectiveOpportunityCost { get; set; } // TODO: Remove once user starts using employeeStatusCode
        public string EffectiveOpportunityCostReason { get; set; } // TODO: Remove once user starts using employeeStatusCode
        public string LastUpdatedBy { get; set; }
    }
}
