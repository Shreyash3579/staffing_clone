using System;

namespace CaseIntake.API.Models
{
    public class CaseIntakeAlertViewModel
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; }
        public Guid? OpportunityId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string OldCaseCode { get; set; }
        public string DemandName { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedByName { get; set; }
    }
}
