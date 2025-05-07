using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface ICasePlanningStaffableTeamsService
    {
        Task<IEnumerable<CasePlanningBoardStaffableTeams>> GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(string officeCodes, DateTime startWeek, DateTime? endWeek = null);
        Task<IEnumerable<CasePlanningBoardStaffableTeams>> UpsertCasePlanningBoardStaffableTeams(IEnumerable<CasePlanningBoardStaffableTeams> staffableTeamsToUpsert);
    }
}
