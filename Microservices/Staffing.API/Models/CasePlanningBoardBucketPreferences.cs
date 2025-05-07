using System;

namespace Staffing.API.Models
{
    public class CasePlanningBoardBucketPreferences
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public short BucketId { get; set; }
        public bool IncludeInDemand { get; set; }
        public bool IsPartiallyChecked { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
