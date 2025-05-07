using System;

namespace CCM.API.Models
{
    public class CaseDataFromClientCaseAPI
    {
        public string CaseAttributeDetails { get; set; }
        public int? ClientId { get; set; }
        public string ClientName { get; set; }
        public int? CaseId { get; set; }
        public string CaseCode { get; set; }
        public string ClientCode { get; set; }
        public string CaseName { get; set; }
        public DateTime? CaseCreateDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ProjectedEndDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BillingOffice { get; set; }
        public short? CaseOffice { get; set; }
        public char? StatusCode { get; set; }
        public string Status { get; set; }
        public string CaseManager { get; set; }
        public string BillingPartner { get; set; }
        public int? CaseType { get; set; }
        public string CaseTypeName { get; set; }
        public int? PrimaryIndustryTermCode { get; set; }
        public int? PrimaryCapabilityTermCode { get; set; }
        public Guid? PrimaryIndustryTagId { get; set; }
        public Guid? PrimaryCapabilityTagId { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool? IsConfidential { get; set; }
        public bool? IsRevenueProducing { get; set; }
        public Guid? PipelineId { get; set; }
        public bool? IsPegPremiumPricing { get; set; }
        public string IsPegCase { get; set; }
    }
}
