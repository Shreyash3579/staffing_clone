using System;

namespace Staffing.HttpAggregator.Models
{
    public class SupplyGroupFilterCriteria
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EmployeeCodes { get; set; }
        public string SortBy { get; set; }
    }
}
