using System;

namespace Staffing.Coveo.API.ViewModels
{
    public class AnalyticsClickViewModel
    {
        public string actionCause { get; set; }
        public bool anonymous { get; set; }
        public int documentPosition { get; set; }
        public string documentUri { get; set; }
        public string documentUriHash { get; set; }
        public string language { get; set; }
        public Guid searchQueryUid { get; set; }
        public string sourceName { get; set; }
        public string originLevel1 { get; set; }
        public string queryPipeline { get; set; }
        public string userDisplayName { get; set; }
        public string username { get; set; }
        public string collectionName { get; set; }
        public string documentTitle { get; set; }
        public string originLevel2 { get; set; }
    }
}
