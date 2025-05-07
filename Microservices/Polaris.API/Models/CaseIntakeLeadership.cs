using System;

namespace Polaris.API.Models
{
    public class CaseIntakeLeadership
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; }
        public string CaseRoleName { get; set; }
        public short? AllocationPercentage { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? OpportunityId { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
