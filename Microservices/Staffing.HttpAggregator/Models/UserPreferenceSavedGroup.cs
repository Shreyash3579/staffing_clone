using System.Collections.Generic;
using System;

namespace Staffing.HttpAggregator.Models
{
    public class UserPreferenceSavedGroup
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool isDefault { get; set; }
        public string OfficeCodes { get; set; }
        public string StaffingTags { get; set; }
        public string LevelGrades { get; set; }
        public string PositionCodes { get; set; }
        public string EmployeeStatuses { get; set; }
        public string PracticeAreaCodes { get; set; }
        public short? MinAvailabilityThreshold { get; set; }
        public short? MaxAvailabilityThreshold { get; set; }
        public string LastUpdatedBy { get; set; }
        public string StaffableAsTypeCodes { get; set; }
        public string AffiliationRoleCodes { get; set; }
        public string ResourcesTabSortBy { get; set; }
        public IEnumerable<UserPreferenceGroupFilters> FilterBy { get; set; }
        public IEnumerable<UserPreferenceGroupSharedInfo> SharedWith { get; set; }
    }
}
