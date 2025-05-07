using System;

namespace OcelotAPIGateway.Models
{
    public class Project
    {
        public Int16 ClientCode { get; set; }
        public string OldCaseCode { get; set; }
        public Int16 CaseCode { get; set; }
        public string CaseName { get; set; }
        public string ClientName { get; set; }
        public string OfficeAbbreviation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PipelineId { get; set; }
        public string OpportunityName { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string Type { get; set; }

    }
}
