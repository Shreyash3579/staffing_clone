namespace Hcpd.API.Models
{
    public class SecurityUser
    {
        public string EmployeeCode { get; set; }
        public int OfficeCode { get; set; }
        public int RoleCode { get; set; }
        public string RoleName { get; set; }
        public string LevelGradeAccess { get; set; }
        public string PDGradeAccess { get; set; }
    }
}
