using System;

namespace Staffing.HttpAggregator.Models
{
    public class SupplyFilterCriteria
    {
        public string AvailabilityIncludes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string OfficeCodes { get; set; }
        public string LevelGrades { get; set; }
        public string StaffingTags { get; set; }
        public string SortBy { get; set; }
        public string Certificates { get; set; }
        public string Languages { get; set; }
        public string PracticeAreaCodes { get; set; }
        public string EmployeeStatuses { get; set; }
        public string PositionCodes { get; set; }
        public string StaffableAsTypeCodes { get; set; }
        public string AffiliationRoleCodes { get; set; }
    }
}
