namespace Staffing.API.Models
{
    public class EmployeeStaffingPreferences
    {
        public string EmployeeCode { get; set; }
        public string PreferenceTypeCode { get; set; }
        public string PreferenceTypeName { get; set; }
        public string StaffingPreference { get; set; }
        public int Priority { get; set; }
        public string LastUpdatedBy { get; set; }
        public bool Interest { get; set; }
        public bool NoInterest { get; set; }
    }
}
