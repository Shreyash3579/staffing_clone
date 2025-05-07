using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class StaffingPreference
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Interest { get; set; }
        public bool NoInterest { get; set; }
    }

    public class StaffingPreferenceComparer: IEqualityComparer<StaffingPreference>
    {
        public bool Equals(StaffingPreference x, StaffingPreference y)
        {
            return x.Code == y.Code && x.Name.ToLower() == y.Name.ToLower();
        }

        public int GetHashCode(StaffingPreference obj)
        {
            return obj.Code.GetHashCode();
        }
    }
}
