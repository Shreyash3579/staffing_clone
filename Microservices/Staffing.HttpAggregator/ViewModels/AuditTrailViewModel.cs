using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class AuditTrailViewModel
    {
        public string EventDescription { get; set; }
        public string Old { get; set; }
        public string New { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Date { get; set; }
    }
}
