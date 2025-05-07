using System;

namespace Staffing.Coveo.API.Models
{
    public class TimeOff
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Notes { get; set; }
    }
}
