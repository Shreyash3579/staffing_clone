using System;

namespace BackgroundPolling.API.Models
{
    public class ResourceLOA
    {
        public string EmployeeCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Type { get; set; }
    }
}
