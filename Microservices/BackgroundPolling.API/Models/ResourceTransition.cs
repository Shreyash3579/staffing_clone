using System;

namespace BackgroundPolling.API.Models
{
    public class ResourceTransition
    {
        public string EmployeeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; }
    }
}
