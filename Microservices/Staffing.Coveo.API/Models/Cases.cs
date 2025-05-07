using System;

namespace Staffing.Coveo.API.Models
{
    public class Case
    {
        public int? CaseCode { get; set; }
        public string CaseName { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string StatusCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OldCaseCode { get; set; }
        public string CaseTypeName { get; set; }
        public string CaseTypeCode { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }
        public int BillingOfficeCode { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string BillingOfficeName { get; set; }
        public string PrimaryIndustry { get; set; }
        public string PrimaryCapability { get; set; }
        public string PracticeAreaIndustry { get; set; }
        public string PracticeAreaCapability { get; set; }
        public byte? IsPrivateEquity { get; set; }
        public string CaseManagerCode { get; set; }
        public string CaseManagerName { get; set; }
        public string Source { get; set; }
        public string Uri { get; set; }
        public string UriHash { get; set; }
        public string SysCollection { get; set; }
        public Guid SearchUid { get; set; }
        public int RequestDuration { get; set; }
        public string Title { get; set; }
    }

    public class CaseResponse
    {
        public int TotalCount { get; set; }
        public Guid SearchUid { get; set; }
        public int Duration { get; set; }
        public CaseResult[] Results { get; set; }
    }
    public class CaseResult
    {
        public Case Raw { get; set; }
    }
}
