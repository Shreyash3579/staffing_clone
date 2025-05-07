using System;

namespace Staffing.HttpAggregator.Models
{
    public class CaseRoll
    {
        public Guid? Id { get; set; }
        public string RolledFromOldCaseCode { get; set; }
        public string RolledToOldCaseCode { get; set; }
        public string RolledToPlanningCardId { get; set; }
        public string PlanningCardName { get; set; }
        public DateTime? CurrentCaseEndDate { get; set; }
        public DateTime? ExpectedCaseEndDate { get; set; }
        public bool isProcessedFromCCM { get; set; }
        public string RolledScheduleIds { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
