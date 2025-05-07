using System;

namespace BackgroundPolling.API.Models
{
    public class ResourceTransaction
    {
        public string EmployeeCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string LevelGrade { get; set; }
        public decimal BillCode { get; set; }
        public decimal FTE { get; set; }
        public Office OperatingOffice { get; set; }
        public Position Position { get; set; }
        public ServiceLine ServiceLine { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Type { get; set; }
    }
}
