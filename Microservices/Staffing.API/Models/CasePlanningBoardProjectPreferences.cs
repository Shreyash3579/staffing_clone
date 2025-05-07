using System;

namespace Staffing.API.Models
{
    public class CasePlanningBoardProjectPreferences
    {
        public Guid? Id { get; set; }
        public Guid? PlanningBoardId { get; set; }
        public string EmployeeCode { get; set; }
        public bool? IncludeProjectInDemand { get; set; }
        public bool? IsIncludeAll { get; set; }
        public bool IncludeBucketInDemand { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
