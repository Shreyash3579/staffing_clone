using System;

namespace Staffing.HttpAggregator.Models
{
    public class AuditEmployeeHistory
    {
        public string EventDescription { get; set; }
        public string Employee { get; set; }
        public string Old { get; set; }
        public string New { get; set; }
        public Guid PipelineId { get; set; }
        public Guid NewPipelineId { get; set; }
        public string OldCaseCode { get; set; }
        public string NewOldCaseCode { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Date { get; set; }
    }
}
