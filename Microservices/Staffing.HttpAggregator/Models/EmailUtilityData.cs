using System;

namespace Staffing.HttpAggregator.Models
{
    public class EmailUtilityData
    {
        public Guid Id { get; set; }
        public string employeeCode { get; set; }
        public string employeeName { get; set; }
        public string positionGroupName { get; set; }
        public string currentLevelGrade { get; set; }
        public string serviceLineName { get; set; }
        public string officeName { get; set; }
        public string status { get; set; }
        public int retryCount { get; set; }
        public string exception { get; set; }
        public DateTime date { get; set; }
        public string EmailType { get; set; }
        public string lastUpdatedBy { get; set; }
    }
}