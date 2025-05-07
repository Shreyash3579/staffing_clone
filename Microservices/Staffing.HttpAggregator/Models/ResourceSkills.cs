using System;

namespace Staffing.HttpAggregator.Models
{
    public class ResourceSkills
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string Skill { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
