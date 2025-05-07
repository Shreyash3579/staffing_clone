using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Models
{
    public class AnalyticsViewModel
    {
        public int totalCount { get; set; }
        public IList<AnalyticsData> result { get; set; }
    }
}
