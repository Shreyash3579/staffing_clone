using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    public class ResourceCommitment
    {
        public IEnumerable<Resource> Resources { get; set; }
        public IEnumerable<ResourceAssignmentViewModel> Allocations { get; set; }
        public IEnumerable<ResourceLoA> LoAs { get; set; }
        public IEnumerable<VacationRequestViewModel> Vacations { get; set; }
        public IEnumerable<TrainingViewModel> Trainings { get; set; }
        public IEnumerable<CommitmentViewModel> Commitments { get; set; }
        public IEnumerable<ResourceTransition> Transitions { get; set; }
        public IEnumerable<ResourceTransfer> Transfers { get; set; }
        public IEnumerable<ResourceTermination> Terminations { get; set; }
        public IEnumerable<ResourceTimeOff> TimeOffs { get; set; }
        public IEnumerable<HolidayViewModel> Holidays { get; set; }
        public IEnumerable<ScheduleMasterPlaceholder> PlaceholderAllocations { get; set; }
        public IEnumerable<StaffableAs> StaffableAsRoles { get; set; }
    }
}
