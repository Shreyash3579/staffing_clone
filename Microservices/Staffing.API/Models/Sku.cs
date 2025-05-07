using System;

namespace Staffing.API.Models
{
    public class Sku
    {
        public Guid Id { get; set; }
        public string SkuTerm { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public Guid? PlanningCardId { get; set; }
        //public DateTime LastUpdated { get; set; }
        //public string LastUpdatedBy { get; set; }
    }
}
