using System;

namespace Staffing.Coveo.API.Models
{
    public class Commitment
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public short? Allocation { get; set; }
        public string Notes { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool? IsSourceStaffing { get; set; }
        public bool? IsOverridenInSource { get; set; }
    }
}
