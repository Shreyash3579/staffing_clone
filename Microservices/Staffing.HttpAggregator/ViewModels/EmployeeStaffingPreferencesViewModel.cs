using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class EmployeeStaffingPreferencesViewModel
    {
        public string EmployeeCode { get; set; }
        public string PreferenceType { get; set; }
        public IList<StaffingPreference> StaffingPreferences { get; set; }
        public string LastUpdatedBy { get; set; }        
    }
}
