using System;

namespace Staffing.Coveo.API.Models
{
    public class Opportunity
    {
        public Guid? PipelineId { get; set; }
        public string CoordinatingPartnerCode { get; set; }
        public string BillingPartnerCode { get; set; }
        public string OpportunityName { get; set; }
        public string OpportunityStatus { get; set; }        
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Duration { get; set; }
        public int? ProbabilityPercent { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }
        public string Source { get; set; }
        public string Uri { get; set; }
        public string UriHash { get; set; }
        public string SysCollection { get; set; }
        public Guid SearchUid { get; set; }
        public int RequestDuration { get; set; }
        public string Title { get; set; }
    }

    public class OpportunityResponse
    {
        public int TotalCount { get; set; }
        public Guid SearchUid { get; set; }
        public int Duration { get; set; }
        public OpportunityResult[] Results { get; set; }
    }
    public class OpportunityResult
    {
        public Opportunity Raw { get; set; }
    }
}
