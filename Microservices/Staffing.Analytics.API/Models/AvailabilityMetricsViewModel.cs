using System;
using System.Collections.Generic;

namespace Staffing.Analytics.API.Models
{
    public class AvailabilityMetricsViewModel
    {
        public Guid? PlaygroundId { get; set; }
        public IList<AvailabilityMetrics> AvailabilityMetrics { get; set; }
        public IList<AvailabilityMetrics_Nupur> AvailabilityMetrics_Nupur { get; set; }
    }
}
