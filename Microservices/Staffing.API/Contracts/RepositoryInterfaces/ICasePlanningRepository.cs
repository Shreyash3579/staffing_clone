using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ICasePlanningRepository
    {
        Task<CasePlanningBoardDataModel> GetCasePlanningBoardDataByDateRange(DateTime date, DateTime? endDate = null, string loggedInUser = null);
        Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectEndDateAndBucketIds(DateTime startDate, string bucketIds);
        Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectIds(string oldCaseCodes, string pipelineIds, string planningCardIds);
        Task<IEnumerable<CasePlanningBoard>> GetOpportunityDataInCasePlanningBoard();
        Task<CasePlanningBoard> UpsertCasePlanningBoard(CasePlanningBoard casePlanningBoard);
        Task<IEnumerable<CasePlanningBoard>> UpsertCasePlanningBoardData(DataTable dataTable);
        Task DeleteCasePlanningBoardByIds(string ids, string lastUpdatedBy);
        Task<CasePlanningBoardBucketPreferences> UpsertCasePlanningBoardBucketPreferences(CasePlanningBoardBucketPreferences prefrencesData);
        Task<bool> UpsertCasePlanningBoardIncludeInDemandPreferences(CasePlanningBoardProjectPreferences prefrencesData);
        Task<IEnumerable<CasePlanningProjectPreferences>> UpsertCasePlanningProjectDetails(DataTable prefrencesData);
        Task<CasePlanningBoardProjectPreferences> UpsertCasePlanningBoardPreferencesOnDrop(CasePlanningBoardProjectPreferences prefrencesData);
        Task<IEnumerable<CasePlanningProjectPreferences>> GetCasePlanningProjectDetails(string oldCaseCodes, string pipelineIds, string planningCardIds);
    }
}
