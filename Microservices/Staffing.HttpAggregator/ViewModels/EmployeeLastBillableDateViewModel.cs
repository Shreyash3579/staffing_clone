using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class EmployeeLastBillableDateViewModel
    {
        public string EmployeeCode { get; set; }
        public DateTime LastBillableDate { get; set; }
    }
}
