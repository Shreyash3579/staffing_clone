using System;

namespace Iris.API.Models
{
    public class EmployeeSchoolHistory
    {
        public string SchoolName { get; set; }
        public string FieldOfStudy { get; set; }
        public string Degree { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
