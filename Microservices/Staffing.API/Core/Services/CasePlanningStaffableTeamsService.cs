using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class CasePlanningStaffableTeamsService : ICasePlanningStaffableTeamsService
    {
        private readonly ICasePlanningStaffableTeamsRepository _casePlanningRepository;

        public CasePlanningStaffableTeamsService(ICasePlanningStaffableTeamsRepository casePlanningRepository)
        {
            _casePlanningRepository = casePlanningRepository;
        }

        public async Task<IEnumerable<CasePlanningBoardStaffableTeams>> GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(string officeCodes, DateTime startWeek, DateTime? endWeek = null)
        {
            if (string.IsNullOrEmpty(officeCodes) || startWeek.Equals(DateTime.MinValue))
                throw new ArgumentException("officeCodes and startWeek cannot be null or empty");
            else if (endWeek.HasValue && endWeek.Value.CompareTo(startWeek) < 0)
                throw new ArgumentException("endWeek cannot be before the startWeek");

            return await _casePlanningRepository.GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(officeCodes, startWeek, endWeek);
        }

        public async Task<IEnumerable<CasePlanningBoardStaffableTeams>> UpsertCasePlanningBoardStaffableTeams(IEnumerable<CasePlanningBoardStaffableTeams> staffableTeamsToUpsert)
        {
            if (staffableTeamsToUpsert == null || !staffableTeamsToUpsert.Any())
            {
                return Enumerable.Empty<CasePlanningBoardStaffableTeams>();
            }

            var dataTable = ConvertCasePlanningBoardStaffableTeamsToDataTable(staffableTeamsToUpsert);
            return await _casePlanningRepository.UpsertCasePlanningBoardStaffableTeams(dataTable);
        }
        #region private methods
        private static DataTable ConvertCasePlanningBoardStaffableTeamsToDataTable(IEnumerable<CasePlanningBoardStaffableTeams> casePlanningBoardStaffableTeams)
        {
            var casePlanningBoardStaffableTeamsDataTable = new DataTable();
            casePlanningBoardStaffableTeamsDataTable.Columns.Add("id", typeof(Guid));
            casePlanningBoardStaffableTeamsDataTable.Columns.Add("weekOf", typeof(DateTime));
            casePlanningBoardStaffableTeamsDataTable.Columns.Add("officeCode", typeof(short));
            casePlanningBoardStaffableTeamsDataTable.Columns.Add("gcTeamCount", typeof(short));
            casePlanningBoardStaffableTeamsDataTable.Columns.Add("pegTeamCount", typeof(short));
            casePlanningBoardStaffableTeamsDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var data in casePlanningBoardStaffableTeams)
            {
                var row = casePlanningBoardStaffableTeamsDataTable.NewRow();
                row["id"] = (object)data.Id ?? DBNull.Value;
                row["weekOf"] = data.WeekOf;
                row["officeCode"] = data.OfficeCode;
                row["gcTeamCount"] = data.GCTeamCount;
                row["pegTeamCount"] = data.PegTeamCount;
                row["lastUpdatedBy"] = data.LastUpdatedBy;

                casePlanningBoardStaffableTeamsDataTable.Rows.Add(row);
            }

            return casePlanningBoardStaffableTeamsDataTable;
        }
        #endregion
    }
}
