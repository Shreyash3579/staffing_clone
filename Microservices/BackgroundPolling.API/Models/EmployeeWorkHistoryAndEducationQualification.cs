using System;
using System.Collections.Generic;
namespace BackgroundPolling.API.Models
{
    public class EmployeeWorkHistory
    {
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string JobTitle { get; set; }
    }

    public class EmployeeSchoolHistory
    {
        public string SchoolName { get; set; }
        public string FieldOfStudy { get; set; }
        public string Degree { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class EmployeeWorkAndSchoolHistory
    {
        public string Ecode { get; set; }
        public IList<EmployeeSchoolHistory> EducationHistory { get; set; }
        public IList<EmployeeWorkHistory> EmploymentHistory { get; set; }
    }

    public class EmployeesQualifications
    {
        public int TotalCount { get; set; }
        public IList<EmployeeWorkAndSchoolHistory> Items { get; set; }
    }
}
