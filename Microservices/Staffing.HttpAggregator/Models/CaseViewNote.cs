using System;

namespace Staffing.HttpAggregator.Models
{
    public class CaseViewNote
    {
        public Guid? Id { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string Note { get; set; }
        public bool IsPrivate { get; set; }
        public string SharedWith { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
