using System;

namespace BackgroundPolling.API.Models
{
    public class Commitment
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; }
        public CommitmentType CommitmentType { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Allocation { get; set; }
        public string Notes { get; set; }
        public string LastUpdatedBy { get; set; }
        public string IsSourceStaffing { get; set; }
    }
}
