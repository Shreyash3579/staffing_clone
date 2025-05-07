using System;

namespace Staffing.Coveo.API.Models
{
    public class ResourceLoA
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Type => "LOA";
    }
}
