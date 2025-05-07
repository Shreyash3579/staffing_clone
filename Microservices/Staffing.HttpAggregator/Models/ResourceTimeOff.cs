using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    public class ResourceTimeOff
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public List<string> ApprovedBy { get; set; }
    }
}
