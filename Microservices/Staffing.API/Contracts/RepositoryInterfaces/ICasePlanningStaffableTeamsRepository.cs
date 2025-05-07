using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ICasePlanningStaffableTeamsRepository
    {
        Task<IEnumerable<CasePlanningBoardStaffableTeams>> GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(string officeCodes, DateTime startWeek, DateTime? endWeek = null);
        Task<IEnumerable<CasePlanningBoardStaffableTeams>> UpsertCasePlanningBoardStaffableTeams(DataTable staffableTeamsToUpsertDataTable);
    }
}
