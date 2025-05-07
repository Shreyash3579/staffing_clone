using System;

namespace Staffing.API.Models
{
    public class CaseOppCommitment
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public Guid CommitmentId { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? OpportunityId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
