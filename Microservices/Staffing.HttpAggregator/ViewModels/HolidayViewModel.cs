using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class HolidayViewModel
    {
        public string EmployeeCode { get; set; }
        public short? OfficeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Type { get; set; }
    }
}
