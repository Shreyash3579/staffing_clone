using System;

namespace Staffing.API.Models
{
    public class EmployeeTransaction
    {
        public string Type { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
