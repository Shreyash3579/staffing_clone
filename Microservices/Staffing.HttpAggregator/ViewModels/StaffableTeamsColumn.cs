using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class StaffableTeamsColumn
    {
        public DateTime WeekOf { get; set; }
        public StaffableTeamViewModel StaffableTeams { get; set; }
    }
}
