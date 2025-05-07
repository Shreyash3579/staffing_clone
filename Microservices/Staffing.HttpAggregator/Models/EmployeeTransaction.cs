using System;

namespace Staffing.HttpAggregator.Models
{
    public class EmployeeTransaction
    {
        public string Type { get; set; }
        public DateTime EffectiveDate { get; set; }
        public Office OperatingOffice { get; set; }
    }
}
