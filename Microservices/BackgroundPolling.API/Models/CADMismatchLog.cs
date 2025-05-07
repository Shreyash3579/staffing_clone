using System;

namespace BackgroundPolling.API.Models
{
    public class CADMismatchLog
    {
        public string SourceTable { get; set; }
        public int? CountMismatch { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Remarks { get; set; }
    }
}
