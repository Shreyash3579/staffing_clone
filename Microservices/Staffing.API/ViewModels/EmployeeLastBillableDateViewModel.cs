using System;

namespace Staffing.API.ViewModels
{
    public class EmployeeLastBillableDateViewModel
    {
        public string EmployeeCode { get; set; }
        public DateTime LastBillableDate { get; set; }
    }
}
