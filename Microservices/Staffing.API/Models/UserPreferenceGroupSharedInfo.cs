using System;

namespace Staffing.API.Models
{
    public class UserPreferenceGroupSharedInfo
    {
        public Guid Id { get; set; }
        public string SharedWith { get; set; }
        public Guid UserPreferenceGroupId { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
