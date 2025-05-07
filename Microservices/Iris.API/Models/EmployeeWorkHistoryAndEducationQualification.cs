using System.Collections.Generic;
namespace Iris.API.Models
{
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
