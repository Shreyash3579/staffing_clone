using System;

namespace BackgroundPolling.API.Models
{
    public class SecurityUser
    {
        public string EmployeeCode { get; set; }
        public int RoleCode { get; set; }
        public int FeatureCode { get; set; }
        public bool IsBossSystemUser { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Source { get; set; }
    }
}
