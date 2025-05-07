using System;

namespace CCM.API.Models
{
    public class CaseOpportunityMap
    {
        public Guid PipelineId { get; set; }
        public string OldCaseCode { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public int CaseCode { get; set; }
        public string CaseName { get; set; }
        public int CaseTypeCode { get; set; }
        public string CaseTypeName { get; set; }
        public int CaseOfficeCode { get; set; }
        public string CaseOfficeName { get; set; }
        public string CaseOfficeAbbreviation { get; set; }
        public int BillingOfficeCode { get; set; }
        public string BillingOfficeName { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
