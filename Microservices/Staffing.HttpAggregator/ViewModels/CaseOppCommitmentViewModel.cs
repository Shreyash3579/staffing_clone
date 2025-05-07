using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class CaseOppCommitmentViewModel
    {

            public Guid ScheduleId { get; set; }
            public Guid CommitmentId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int? Allocation { get; set; }
            public string Notes { get; set; }
            public bool? IsSourceStaffing { get; set; }
            public string OldCaseCode { get; set; }
            public Guid? OpportunityId { get; set; }
            public Guid? PlanningCardId { get; set; }

    }
}
