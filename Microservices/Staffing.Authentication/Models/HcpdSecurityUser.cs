using System.Collections.Generic;

namespace Staffing.Authentication.Models
{
    public class HcpdSecurityUser
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
