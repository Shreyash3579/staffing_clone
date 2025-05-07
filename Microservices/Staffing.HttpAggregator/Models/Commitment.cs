using System;
namespace Staffing.HttpAggregator.Models
{
    public class Commitment
    {

        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Allocation { get; set; }
        public string Notes { get; set; }
        public bool? IsSourceStaffing { get; set; }
        public bool? IsOverridenInSource { get; set; }
        public CommitmentType CommitmentType { get; set; }

        public string CommitmentTypeReasonCode { get; set; }
        public string CommitmentTypeReasonName { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
