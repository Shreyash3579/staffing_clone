using System;

namespace Polaris.API.Models
{
    public class SecurityUser
    {
        public string EmployeeCode { get; set; }
        public int OfficeCode { get; set; }
        public string Source { get; set; }
        public string RoleType { get; set; }
        public bool IsBossSystemUser { get; set; }
        public bool CapacityBreakdownFlag { get; set; }
        public bool PriceRealizationFlag { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Notes { get; set; }
        public DateTime EndDate { get; set; }
    }
}
