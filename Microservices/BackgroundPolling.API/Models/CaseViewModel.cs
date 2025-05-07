using System;

namespace BackgroundPolling.API.Models
{
    public class CaseViewModel
    {
        public int ClientCode { get; set; }
        public string OldCaseCode { get; set; }
        public int CaseCode { get; set; }
        public string CaseName { get; set; }
        public string ClientName { get; set; }
        public string CaseType { get; set; }
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
        public DateTime? OriginalStartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? OriginalEndDate { get; set; }
        public string Type { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}