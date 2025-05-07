using System;

namespace CCM.API.Models
{
    public class CaseMaster
    {
        public int ClientCode { get; set; }
        public int CaseCode { get; set; }
        public string CaseName { get; set; }
        public string CaseShortName { get; set; }
        public string StatusCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ProjectedEndDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CaseTypeCode { get; set; }
        public string CostCenterCode { get; set; }
        public bool ConfidentialFlag { get; set; }
        public string ClassCode { get; set; }
        public int? BillingCode { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public int ReplOffice { get; set; }
        public string ReplFlag { get; set; }
        public Guid? PipelineId { get; set; }
        public DateTime SysStartTime { get; set; }
        public DateTime SysEndTime { get; set; }
    }
}
