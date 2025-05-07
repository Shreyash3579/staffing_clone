using System;
using System.Collections.Generic;

namespace Staffing.Analytics.API.Models
{
    public class PlaceholderScheduleIdsViewModel
    {
        public IEnumerable<Guid> ScheduleIdsToDelete { get; set; }

        public IEnumerable<Guid> ScheduleIdsToUpsert { get; set; }
    }
}
