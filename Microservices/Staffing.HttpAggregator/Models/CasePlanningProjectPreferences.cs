using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
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
