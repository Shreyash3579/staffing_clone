using System;

namespace Staffing.HttpAggregator.Models
{
    public class ResourceCD
    {
        public Guid? Id { get; set; }
        public string RecentCD { get; set; }
        public string EmployeeCode { get; set; }

        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
