using System;

namespace Staffing.API.Models
{
    public class CasePlanningBoard
    {
        public Guid? Id { get; set; }
        public DateTime Date { get; set; }
        public int BucketId { get; set; }
        public Guid? PipelineId { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PlanningCardId { get; set; }
        public bool IncludeInDemand { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
