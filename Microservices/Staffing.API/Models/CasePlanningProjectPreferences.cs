using System;

namespace Staffing.API.Models
{
    public class CasePlanningProjectPreferences
    {
        public Guid? Id { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public bool? IncludeInDemand { get; set; }
        public bool? IsFlagged { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
