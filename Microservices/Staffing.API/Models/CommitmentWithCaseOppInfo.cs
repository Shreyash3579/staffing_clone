using System;

namespace Staffing.API.Models
{
    public class CommitmentWithCaseOppInfo : Commitment
    {
        public string OldCaseCode { get; set; }
        public Guid? PlanningCardId { get; set; }
        public Guid? OpportunityId { get; set; }
    }
}
