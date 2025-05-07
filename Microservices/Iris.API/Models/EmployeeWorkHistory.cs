using System;

namespace Iris.API.Models
{
    public class EmployeeWorkHistory
    {
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string JobTitle { get; set; }
    }
}
