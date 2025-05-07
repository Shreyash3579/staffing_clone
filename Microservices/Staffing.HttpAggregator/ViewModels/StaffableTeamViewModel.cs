using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class StaffableTeamViewModel
    {
        public string OfficeName { get; set; }
        public short OfficeCode { get; set; }
        public short GCTeamCount { get; set; }
        public short PegTeamCount { get; set; }
        public IEnumerable<StaffableTeamViewModel> staffableTeamChildren { get; set; }
    }
}
