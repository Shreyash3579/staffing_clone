using System;

namespace CCM.API.ViewModels
{
    public class CaseViewModel
    {
        public int CaseCode { get; set; }
        public string CaseName { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientGroupCode { get; set; }
        public string ClientGroupName { get; set; }
        public string OldCaseCode { get; set; }
        public string CaseManagerCode { get; set; }
        public string CaseBillingPartnerCode { get; set; }
        public int? CaseTypeCode { get; set; }
        public string CaseType { get; set; }
        public int? PrimaryIndustryTermCode { get; set; }
        public Guid? PrimaryIndustryTagId { get; set; }
        public string PrimaryIndustry { get; set; }
        public int? IndustryPracticeAreaCode { get; set; }
        public string IndustryPracticeArea { get; set; }
        public string PegIndustryTerm { get; set; }
        public int? PrimaryCapabilityTermCode { get; set; }
        public Guid? PrimaryCapabilityTagId { get; set; }
        public string PrimaryCapability { get; set; }
        public int? CapabilityPracticeAreaCode { get; set; }
        public string CapabilityPracticeArea { get; set; }
        public int? ManagingOfficeCode { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }
        public int? BillingOfficeCode { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string BillingOfficeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? OriginalStartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? OriginalEndDate { get; set; }
        public bool IsPrivateEquity { get; set; }
        public string CaseAttributes { get; set; }
        public string Type { get; set; }
        public bool? CaseServedByRingfence { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
