using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    public class VacationRequestViewModel
    {
        public string EmployeeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public List<string> ApprovedBy { get; set; }
    }
}
