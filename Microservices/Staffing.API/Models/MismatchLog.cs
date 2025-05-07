using System;

namespace Staffing.API.Models
{
    public class MismatchLog
    {
        public string SourceTable { get; set; }
        public string TargetTable { get; set; }
        public int CountMismatch { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
