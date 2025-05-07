using System.Collections.Generic;
using Staffing.HttpAggregator.Models;

namespace Staffing.HttpAggregator.ViewModels
{
    public class ResourceCommitmentViewModel
    {
        public IEnumerable<ResourceAssignmentViewModel> staffingAllocations { get; set; }
        public IEnumerable<VacationRequestViewModel> vacationRequests { get; set; }
        public IEnumerable<HolidayViewModel> officeHolidays { get; set; }
        public IEnumerable<TrainingViewModel> trainings { get; set; }
        public IEnumerable<LOA> loa { get; set; }
        public IEnumerable<Commitment> commitmentsSavedInStaffing { get; set; }
        public ResourceTransition employeeTransition { get; set; }
        public IEnumerable<ResourceTransfer> employeeTransfers { get; set; }
        public ResourceTermination employeeTermination { get; set; }
        public IEnumerable<ResourceTimeOff> employeeTimeOffs { get; set; }
        public IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations { get; set; }
    }
}
