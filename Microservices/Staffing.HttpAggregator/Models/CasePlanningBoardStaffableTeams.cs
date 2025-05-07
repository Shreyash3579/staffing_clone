using System;

namespace Staffing.HttpAggregator.Models
{
    public class CasePlanningBoardStaffableTeams
    {
        public Guid? Id { get; set; }
        public DateTime WeekOf { get; set; }
        public short OfficeCode { get; set; }
        public short GCTeamCount { get; set; }
        public short PegTeamCount { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}