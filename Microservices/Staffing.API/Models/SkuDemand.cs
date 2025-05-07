using System;

namespace Staffing.API.Models
{
    public class SkuDemand
    {
        public Guid Id { get; set; }
        public string currentLevelGrade { get; set; }
        public string level { get; set; }
        public decimal AggregateDemand { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public Guid? PlanningCardId { get; set; }
    }
}
