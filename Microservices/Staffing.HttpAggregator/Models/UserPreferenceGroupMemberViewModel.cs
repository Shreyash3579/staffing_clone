using System;

namespace Staffing.HttpAggregator.Models
{
    public class UserPreferenceGroupMemberViewModel
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string PositionName { get; set; }
        public string CurrentLevelGrade { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
