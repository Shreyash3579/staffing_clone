using System;
using System.Collections.Generic;

namespace CaseIntake.API.Models
{
    public class CaseIntakeRoleAndWorkstream
    {
        public IEnumerable<CaseIntakeRoleDetails> roleDetails { get; set; }
        public IEnumerable<CaseIntakeWorkstreamDetails> workStreamDetails { get; set; }
        public string lastUpdatedBy { get; set; }
        public string lastUpdatedByName { get; set; }
        public DateTime lastUpdated { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? OpportunityId { get; set; }
        public Guid? PlanningCardId { get; set; }
    }
}
