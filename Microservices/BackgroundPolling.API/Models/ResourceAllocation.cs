using System;

namespace BackgroundPolling.API.Models
{
    public class ResourceAllocation
    {
        public string EmployeeCode { get; set; }
        public int? OperatingOfficeCode { get; set; }
        public string OldCaseCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? QueryExecutionTimestamp { get; set; }
    }
}
