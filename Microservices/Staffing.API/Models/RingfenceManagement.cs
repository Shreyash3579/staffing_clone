using System;

namespace Staffing.API.Models
{
    public class RingfenceManagement
    {
        public Guid? Id { get; set; }
        public short OfficeCode { get; set; }
        public decimal RfTeamsOwed { get; set; }
        public string CommitmentTypeCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
