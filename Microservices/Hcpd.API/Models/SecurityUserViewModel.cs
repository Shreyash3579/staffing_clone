using System.Collections.Generic;

namespace Hcpd.API.Models
{
    public class SecurityUserViewModel
    {
        public string EmployeeCode { get; set; }
        public int RoleCode { get; set; }
        public string RoleName { get; set; }
        public IList<HcpdSecurityAccess> SecurityAccessList { get; set; }
    }

    public class HcpdSecurityAccess
    {
        public int Office { get; set; }
        public string[] PDGradeAccess { get; set; }
    }
}
