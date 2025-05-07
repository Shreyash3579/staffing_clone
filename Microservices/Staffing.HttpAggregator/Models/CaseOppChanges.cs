using System;

namespace Staffing.HttpAggregator.Models
{
    public class CaseOppChanges
    {
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public string PegOpportunityId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public short? ProbabilityPercent { get; set; }
        public string Notes { get; set; }
        public bool? CaseServedByRingfence { get; set; }
        public short? StaffingOfficeCode { get; set; }
        public string LastUpdatedBy { get; set; }
        
    }
}
