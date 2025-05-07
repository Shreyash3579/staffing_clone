using System;

namespace Polaris.API.Models
{
    public class CaseIntakeDetail
    {
        public string OfficeCode { get; set; }
        public string ClientEngagementModel { get; set; }
        public string CaseDescription { get; set; }
        public string ExpertiseRequirement { get; set; }
        public string Languages { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? OpportunityId { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}