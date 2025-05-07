using OcelotAPIGateway.API.Models;
using System;
using System.Collections.Generic;

namespace OcelotAPIGateway.Models
{
    public class Project
    {
        public int ClientCode { get; set; }
        public string OldCaseCode { get; set; }
        public int CaseCode { get; set; }
        public string CaseName { get; set; }
        public string ClientName { get; set; }
        public string PrimaryIndustry { get; set; }
        public string IndustryPracticeArea { get; set; }
        public string PrimaryCapability { get; set; }
        public string CapabilityPracticeArea { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public string ManagingOfficeName { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string BillingOfficeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Duration { get; set; }
        public Guid? PipelineId { get; set; }
        public string CoordinatingPartnerCode { get; set; }
        public string BillingPartnerCode { get; set; }
        public string OpportunityName { get; set; }
        public string OpportunityStatus { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string Type { get; set; }
        public bool IsCaseOnRoll { get; set; }
        public bool? IsUpdateEndDateFromCCM { get; set; }
        public DateTime? CaseRollExpectedEndDate { get; set; }
        public IEnumerable<ResourceAllocation> AllocatedResources { get; set; }
        public SKUCaseTermsViewModel SKUCaseTerms { get; set; }
    }

}
