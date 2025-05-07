using System;

namespace Staffing.Coveo.API.Models
{
    public class Training
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Role { get; set; }
        public string TrainingName { get; set; }
    }
}
