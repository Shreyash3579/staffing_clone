using System;

namespace OcelotAPIGateway.API.Models
{
    public class ResourceAllocation
    {
        public Guid? Id { get; set; }
        public int ClientCode { get; set; }
        public int? CaseCode { get; set; }
        public string OldCaseCode { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public String CurrentLevelGrade { get; set; }
        public string OfficeAbbreviation { get; set; }
        public int Allocation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PipelineId { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
