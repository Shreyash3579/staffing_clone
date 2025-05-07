using System;

namespace Staffing.HttpAggregator.Models
{
    public class SKUDemand
    {
        public Guid? Id { get; set; }
        public string SKUTermsCodes { get; set; } //TODO: remove this once SKU logic is changed everywhere in App
        public decimal AggregateDemand { get; set; }
        public string currentLevelGrade { get; set; }
        public string level { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string EffectiveDate { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
