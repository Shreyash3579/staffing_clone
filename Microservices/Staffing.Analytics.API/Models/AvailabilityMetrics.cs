using System;

namespace Staffing.Analytics.API.Models
{
    public class AvailabilityMetrics
    {
        public DateTime Week_Of { get; set; }
        public string CurrentLevelGrade { get; set; }
        public string Level { get; set; }
        public string CapacitySubCategory { get; set; }
        public decimal AggregateAvailability { get; set; }
    }
}
