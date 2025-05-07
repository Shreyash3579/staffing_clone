using System;

namespace Staffing.HttpAggregator.Models
{
    public class ResourceTransfer
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? StartDate { get; set; }
        public string LevelGrade { get; set; }
        public decimal BillCode { get; set; }
        public decimal FTE { get; set; }
        public Office OperatingOffice { get; set; } //TODO: Remove this field later after testing
        public Office OperatingOfficeCurrent { get; set; }
        public Office OperatingOfficeProposed { get; set; }
        public Position Position { get; set; }
        public string Type { get; set; }
    }
}
