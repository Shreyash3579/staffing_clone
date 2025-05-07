using System;

namespace Staffing.Coveo.API.Models
{
    public class AnalyticsSearchParameters
    {
        public AnalyticsCommonParameters CommonParameters { get; set; }
        public string QueryText { get; set; }
        public string AdvancedQuery { get; set; }
        public int? ResponseTime { get; set; }
        public Guid? SearchQueryUid { get; set; }
    }
}
