using System;

namespace Staffing.HttpAggregator.Models
{
    public class UserPreferenceGroupSharedInfo
    {
        public Guid? Id { get; set; }
        public Guid? UserPreferenceGroupId { get; set; }
        public string SharedWith { get; set; }
        public bool IsDefault { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
