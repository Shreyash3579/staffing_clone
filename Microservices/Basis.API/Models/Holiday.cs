using System;

namespace Basis.API.Models
{
    public class Holiday
    {
        public string EmployeeCode { get; set; }
        public short? OfficeCode { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Description { get; set; }
    }
}
