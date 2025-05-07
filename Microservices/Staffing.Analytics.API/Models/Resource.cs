using System;

namespace Staffing.Analytics.API.Models
{
    public class Resource
    {
        public string EmployeeCode { get; set; }
        public string EmployeeType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string LevelGrade { get; set; }
        public string LevelName { get; set; }
        public decimal BillCode { get; set; }
        public decimal Fte { get; set; }
        public bool Status { get; set; }
        public string ActiveStatus { get; set; }
        public string InternetAddress { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string ProfileImageUrl { get; set; }
        public Office Office { get; set; }
        public Office OperatingOffice { get; set; }
        public ServiceLine ServiceLine { get; set; }
        public Position Position { get; set; }
    }
}
