using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    public class ResourceView
    {
        public Resource Resource { get; set; }
        public List<ResourceAssignmentViewModel> Allocations { get; set; }
        public List<ResourceAssignmentViewModel> PlaceholderAllocations { get; set; }
        public IEnumerable<ResourceLoA> LoAs { get; set; }
        public IEnumerable<VacationRequestViewModel> Vacations { get; set; }
        public IEnumerable<TrainingViewModel> Trainings { get; set; }
        public IEnumerable<ResourceTransfer> Transfers { get; set; }
        public IEnumerable<ResourceTransition> Transitions { get; set; }
        public IEnumerable<ResourceTermination> Terminations { get; set; }
        public IEnumerable<CommitmentViewModel> Commitments { get; set; }
        public IEnumerable<ResourceTimeOff> TimeOffs { get; set; }
        public IEnumerable<HolidayViewModel> Holidays { get; set; }
        public IEnumerable<StaffableAs> StaffableAsRoles { get; set; }
        public IEnumerable<ResourceViewNoteViewModel> ResourceViewNotes { get; set; }
        public IEnumerable<EmployeeLastBillableDateViewModel> LastBillableDates { get; set; }
        public IEnumerable<Certification> Certificates { get; set; }
        public IEnumerable<Language> Languages { get; set; }
        public IEnumerable<EmployeeStaffingPreferences> Preferences { get; set; }
        public IEnumerable<ResourceViewCDViewModel> ResourceCD { get; set; }

        public IEnumerable<ResourceViewCommercialModelViewModel> ResourceCommercialModel { get; set; }

        public IEnumerable<EmployeePracticeArea> Affiliations { get; set; }


    }
}