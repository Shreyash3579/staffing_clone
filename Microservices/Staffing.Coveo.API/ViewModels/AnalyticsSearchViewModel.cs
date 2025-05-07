using System;

namespace Staffing.Coveo.API.ViewModels
{
    public class AnalyticsSearchViewModel
    {
        public string actionCause { get; set; }
        public string language { get; set; }
        public string queryText { get; set; }
        public int responseTime { get; set; }
        public Guid searchQueryUid { get; set; }
        public bool anonymous { get; set; }
        public string queryPipeline { get; set; }
        public string originLevel1 { get; set; }
        public string originLevel2 { get; set; }
        public string userDisplayName { get; set; }
        public string username { get; set; }
        public string documentTitle { get; set; }
        public int numberOfResults { get; set; }
    }
}
