using System;
using System.Collections.Generic;

namespace Staffing.API.Models
{
    public class ResourceFilter
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string EmployeeCode { get; set; }
        public bool isDefault { get; set; }
        public string OfficeCodes { get; set; }
        public string StaffingTags { get; set; }
        public string LevelGrades { get; set; }
        public string PositionCodes { get; set; }
        public string EmployeeStatuses { get; set; }
        public string CommitmentTypeCodes { get; set; }
        public string Certificates { get; set; }
        public string Languages { get; set; }
        public string PracticeAreaCodes { get; set; }
        public short? MinAvailabilityThreshold { get; set; }
        public short? MaxAvailabilityThreshold { get; set; }
        public string LastUpdatedBy { get; set; }
        public string StaffableAsTypeCodes { get; set; }
        public string AffiliationRoleCodes { get; set; }
        public string ResourcesTabSortBy { get; set; }

    }
}
