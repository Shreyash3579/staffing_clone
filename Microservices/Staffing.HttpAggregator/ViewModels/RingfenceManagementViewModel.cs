using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class RingfenceManagementViewModel
    {
        public Guid? Id { get; set; }
        public short OfficeCode { get; set; }
        public string OfficeName { get; set; }
        public decimal? RfTeamsOwed { get; set; }
        public int TotalRFResources { get; set; }
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedByName { get; set; }
    }
}
