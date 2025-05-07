using System;

namespace Staffing.HttpAggregator.Models
{
    public class TrainingViewModel
    {
        public string EmployeeCode { get; set; }
        public string Role { get; set; }
        public string TrainingName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; }
    }
}
