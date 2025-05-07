using System;

namespace Staffing.API.Models
{
    public class SKUCaseTerms
    {
        public Guid? Id { get; set; }
        public string SKUTermsCodes { get; set; }
        public string SKUTerm { get; set; } //Used in new SKU calculation
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
