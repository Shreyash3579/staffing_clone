using System;

namespace Staffing.API.Models
{
    public class AuditCaseHistory
    {
        public string EventDescription { get; set; }
        public string Employee { get; set; }
        public string Old { get; set; }
        public string New { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Date { get; set; }
    }
}
