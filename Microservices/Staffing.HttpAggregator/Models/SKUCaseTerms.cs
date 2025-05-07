using System;

namespace Staffing.HttpAggregator.Models
{
    public class SKUCaseTerms
    {
        public Guid? Id { get; set; }
        public string SKUTermsCodes { get; set; } //TODO: remove this once SKU logic is changed everywhere in App
        public string SkuTerm { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public string EffectiveDate { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
