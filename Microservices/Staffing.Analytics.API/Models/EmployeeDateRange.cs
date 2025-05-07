using System;

namespace Staffing.Analytics.API.Models
{
    public class AvailabilityDateRange
    {
        public string EmployeeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
