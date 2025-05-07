using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface ICasePlanningMetricsService
    {
        //Task<IEnumerable<CasePlanningBoardColumn>> GetCasePlanningBoardData(DemandFilterCriteria demandFilterCriteria, string employeeCode);
        Task<IEnumerable<CasePlanningBoardColumn>> GetCasePlanningBoardColumnsData(DemandFilterCriteria demandFilterCriteria, string employeeCode);
        Task<CasePlanningBoardColumn> GetCasePlanningBoardNewDemandsData(DemandFilterCriteria demandFilterCriteria, string employeeCode);
        Task<IEnumerable<StaffableTeamsColumn>> GetCasePlanningBoardStaffableTeams(string officeCodes, DateTime startWeek, DateTime endWeek);
    }
}
