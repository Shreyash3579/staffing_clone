using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class CasePlanningStaffableTeamsRepository : ICasePlanningStaffableTeamsRepository
    {
        private readonly IBaseRepository<CasePlanningBoardStaffableTeams> _baseRepository;

        public CasePlanningStaffableTeamsRepository(IBaseRepository<CasePlanningBoardStaffableTeams> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<CasePlanningBoardStaffableTeams>> GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(string officeCodes, DateTime startWeek, DateTime? endWeek = null)
        {
            var staffableTeams = await _baseRepository.GetAllAsync(
                new
                {
                    officeCodes,
                    startWeek,
                    endWeek
                },
                StoredProcedureMap.GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange
            );

            return staffableTeams;
        }

        public async Task<IEnumerable<CasePlanningBoardStaffableTeams>> UpsertCasePlanningBoardStaffableTeams(DataTable staffableTeamsToUpsertDataTable)
        {
            var upsertedCasePlanningBoard = await _baseRepository.Context.Connection.QueryAsync<CasePlanningBoardStaffableTeams>(
                StoredProcedureMap.UpsertCasePlanningBoardStaffableTeams,
                new
                {
                    casePlanningBoardStaffableTeams =
                        staffableTeamsToUpsertDataTable.AsTableValuedParameter(
                            "[dbo].[casePlanningBoardStaffableTeamsTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedCasePlanningBoard;
        }
    }
}
