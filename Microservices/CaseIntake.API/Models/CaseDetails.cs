using System;

namespace CaseIntake.API.Models
{
    public class CaseDetails
    {
        public int CaseCode { get; set; }
        public string CaseName { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string OldCaseCode { get; set; }
        public string CaseManagerCode { get; set; }
        public string CaseManagerFullName { get; set; }
        public string CaseManagerOfficeAbbreviation { get; set; }
        public string CaseBillingPartnerCode { get; set; }
        public string CaseBillingPartnerFullName { get; set; }
        public string CaseBillingPartnerOfficeAbbreviation { get; set; }
        public string PegIndustryTerm { get; set; }
        public int? CaseTypeCode { get; set; }
        public string CaseType { get; set; }
        public string EstimatedTeamSize { get; set; }
        public string PricingTeamSize { get; set; }
        public string PrimaryIndustry { get; set; }
        public string IndustryPracticeArea { get; set; }
        public string PrimaryCapability { get; set; }
        public string CapabilityPracticeArea { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }
        public int? BillingOfficeCode { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string BillingOfficeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPrivateEquity { get; set; }
        public string CaseAttributes { get; set; }
        public bool? CaseServedByRingfence { get; set; }
        public string Type { get; set; }
    }
}
