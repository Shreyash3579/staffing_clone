using System;

namespace Staffing.API.Models
{
    public class EmailUtilityData
    {
        public Guid Id { get; set; }
        public string EmailType { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string CurrentLevelGrade { get; set; }
        public string PositionGroupName { get; set; }
        public string ServiceLineName { get; set; }
        public string OfficeName { get; set; }
        public string Status { get; set; }
        public int RetryCount { get; set; }
        public string Exception { get; set; }
        public DateTime Date { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}