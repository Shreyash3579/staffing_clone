using System.Collections.Generic;

namespace BackgroundPolling.API.Models
{
    public class SecurityUserModel
    {
        public IEnumerable<SecurityUser> SecurityUsers { get; set; }
        public IEnumerable<SecurityUser> SecurityUsersWithFeatureAccess { get; set; }

        public IEnumerable<PolarisSecurityUser> PolarisSecurityUsersWithGeography { get; set; }
    }
}
