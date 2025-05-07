using System;

namespace Staffing.Analytics.API.Models
{
    public class ResourceCommitment
    {
        public string EmployeeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Allocation { get; set; }
        public bool? IsSourceStaffing { get; set; }
        public bool? IsOverridenInSource { get; set; }
        public string PriorityCommitmentTypeCode { get; set; }
        public string PriorityCommitmentTypeName { get; set; }
        public string CommitmentTypeCodes { get; set; }
        public string CommitmentTypeReasonCode { get; set; }
        public string CommitmentTypeReasonName { get; set; }
        public string Ringfence { get; set; }
        public bool? isStaffingTag { get; set; }
        public ResourceCommitment Clone()
        {
            return (ResourceCommitment)this.MemberwiseClone();
        }
    }
}
