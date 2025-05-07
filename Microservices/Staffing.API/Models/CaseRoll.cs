using System;

namespace Staffing.API.Models
{
    public class CaseRoll
    {
        public Guid? Id { get; set; }
        public string RolledFromOldCaseCode { get; set; }
        public string RolledToOldCaseCode { get; set; }
        public Guid? RolledToPlanningCardId { get; set; }
        public string PlanningCardName { get; set; }
        public DateTime? CurrentCaseEndDate { get; set; }
        public DateTime? ExpectedCaseEndDate { get; set; }
        public bool IsProcessedFromCCM { get; set; }
        public string RolledScheduleIds { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
