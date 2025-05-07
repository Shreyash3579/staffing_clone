using System;

namespace Staffing.API.Models
{
    public class SecurityGroup
    {
        public Guid? Id { get; set; }
        public string GroupName { get; set; }
        public string RoleCodes { get; set; }
        public string FeatureCodes { get; set; }
        public string Notes { get; set; }
        public string OfficeRegionCodes { get; set; }
        public bool IsBossSystemUser { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
