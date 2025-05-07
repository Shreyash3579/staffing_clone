using Staffing.Analytics.API.Core.Helpers;
using System;

namespace Staffing.Analytics.API.Models
{
    public class ResourceAvailabilityViewModel
    {
        public Guid? Id { get; set; }
        // Employee Related info
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public Constants.EmployeeStatus EmployeeStatusCode { get; set; } = Constants.EmployeeStatus.Active;
        public decimal Fte { get; set; }
        public int OperatingOfficeCode { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public string OperatingOfficeName { get; set; }
        public string CurrentLevelGrade { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public string PositionGroupName { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public decimal BillCode { get; set; }
        // Staffing related info
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Availability { get; set; }
        public string LastUpdatedBy { get; set; }
        // Finance related info
        public decimal? BillRate { get; set; }
        public string BillRateType { get; set; }
        public string BillRateCurrency { get; set; }
        public decimal? OpportunityCost { get; set; }
        public decimal? OpportunityCostInUSD { get; set; }
        public decimal? costUSDEffectiveYear { get; set; }
        public decimal? UsdRate { get; set; }

        // Commitment related info
        public string PriorityCommitmentTypeCode { get; set; }
        public string PriorityCommitmentTypeName { get; set; }
        public string CommitmentTypeCodes { get; set; }

        public string CommitmentTypeReasonCode { get; set; }
        public string CommitmentTypeReasonName { get; set; }
        public string Ringfence { get; set; }
        public bool? isStaffingTag { get; set; }
        public bool? isOverriddenInSource { get; set; }
        // TODO : Delete
        public string Position { get; set; } 
        public int EffectiveAvailability { get; set; }
        public string EffectiveAvailabilityReason { get; set; } 
        public decimal? EffectiveOpportunityCost { get; set; }
        public string EffectiveOpportunityCostReason { get; set; }
    }
}
