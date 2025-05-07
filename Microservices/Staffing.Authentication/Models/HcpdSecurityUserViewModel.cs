using System.Collections.Generic;


namespace Staffing.Authentication.Models
{
    public class HcpdSecurityUserViewModel
    {
        public string EmployeeCode { get; set; }
        public IList<HcpdSecurityAccess> SecurityAccessList { get; set; }
    }
}
