using System;

namespace CaseIntake.API.Models
{
    public class LastUpdates
    {
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedByName { get; set; }
    }
}
