using System;

namespace Staffing.HttpAggregator.Models
{
    public class ResourceTransition
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string LevelGrade { get; set; }
        public decimal BillCode { get; set; }
        public decimal FTE { get; set; }
        public Office OperatingOffice { get; set; }
        public Position Position { get; set; }
        public string Type { get; set; }
    }
}
