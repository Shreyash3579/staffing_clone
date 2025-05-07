using System;

namespace Staffing.Analytics.API.Models
{
    public class AvailabilityMetrics_Nupur
    {
        public DateTime Week_Of { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string CurrentLevelGrade { get; set; }
        public string Level { get; set; }
        public string CapacitySubCategory { get; set; }
        public decimal Allocation { get; set; }
        public DateTime AvailabilityDate { get; set; }
    }
}