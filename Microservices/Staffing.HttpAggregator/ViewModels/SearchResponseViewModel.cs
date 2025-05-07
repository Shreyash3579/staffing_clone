using Staffing.HttpAggregator.Models;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class SearchResponseViewModel
    {
        public IEnumerable<SearchResourceViewModel> Searches { get; set; }
        public BossSearchQuery GeneratedLuceneSearchQuery { get; set; }
    }

    public class SearchResourceViewModel
    {
        public dynamic Document { get; set; }
        public double? Score { get; internal set; }
        public Resource Resource { get; set; }
        public List<ResourceAssignmentViewModel> Allocations { get; set; }
        public List<ScheduleMasterPlaceholder> PlaceholderAllocations { get; set; }
        public IEnumerable<ResourceLoA> LoAs { get; set; }
        public IEnumerable<VacationRequestViewModel> Vacations { get; set; }
        public IEnumerable<TrainingViewModel> Trainings { get; set; }
        public IEnumerable<ResourceTransfer> Transfers { get; set; }
        public IEnumerable<ResourceTransition> Transitions { get; set; }
        public IEnumerable<ResourceTermination> Terminations { get; set; }
        public IEnumerable<CommitmentViewModel> Commitments { get; set; }
        public IEnumerable<ResourceTimeOff> TimeOffs { get; set; }
        public IEnumerable<HolidayViewModel> Holidays { get; set; }
    }
}
