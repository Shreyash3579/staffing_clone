using System;

namespace BackgroundPolling.API.Models
{
    public class AuditLog
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string ServiceLineName { get; set; }
        public string OldCaseCode { get; set; }
        public string CaseName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Allocation { get; set; }
        public string StaffingUserECode { get; set; }
        public string StaffingUserName { get; set; }
        public DateTime LastUpdated { get; set; }


    }
}
