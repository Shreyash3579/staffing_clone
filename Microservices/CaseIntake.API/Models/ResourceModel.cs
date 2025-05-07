using System;
using CaseIntake.API.Models.Workday;

namespace CaseIntake.API.Models
{
    public class ResourceModel
    {
        public string EmployeeCode { get; set; }
        public string EmployeeType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FamiliarFirstName { get; set; }
        public string FamiliarLastName { get; set; }
        public string FullName { get; set; }
        public decimal Fte { get; set; }
        public string MentorName { get; set; }
        public string MentorEcode { get; set; }
        public string LevelGrade { get; set; }
        public string LevelName { get; set; }
        public ServiceLineModel ServiceLine { get; set; }
        public PositionModel Position { get; set; }
        public decimal BillCode { get; set; }
        public bool Status { get; set; }
        public string ActiveStatus { get; set; }
        public string InternetAddress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string ProfileImageUrl { get; set; }
        public Office Office { get; set; }
        public DepartmentModel Department { get; set; }
        public Office SchedulingOffice { get; set; }
    }
}
