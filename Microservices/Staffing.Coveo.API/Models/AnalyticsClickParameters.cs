using System;

namespace Staffing.Coveo.API.Models
{
    public class AnalyticsClickParameters
    {
        public AnalyticsCommonParameters CommonParameters { get; set; }
        public int? ResponseTime { get; set; }
        public Guid? SearchQueryUid { get; set; }
    }
}
