using System;

namespace Staffing.HttpAggregator.Models
{
    public class CommitmentAlert
    {
        public int CommitmentId { get; set; }
        public string EmployeeCode { get; set; }
        public string LevelGrade { get; set; }
        public string OperatingOfficeName { get; set; }
        public string OperatingOfficeCode { get; set; }
        public string AlertStatus { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string CreatedBy { get; set; }
    }
}
