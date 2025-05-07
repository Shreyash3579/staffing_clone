using System;

namespace Staffing.HttpAggregator.Models
{
    public class LOA
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Type => "LOA";
    }
}
