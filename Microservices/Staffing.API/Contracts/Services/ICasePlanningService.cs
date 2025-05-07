using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface ICasePlanningService
    {
        Task<CasePlanningBoardDataModel> GetCasePlanningBoardDataByDateRange(DateTime startDate, DateTime? endDate = null, string loggedInUser = null);
        Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectEndDateAndBucketIds(DateTime startDate, string bucketIds);
        Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectIds(string oldCaseCodes, string pipelineIds, string planningCardIds);
        Task<IEnumerable<CasePlanningBoard>> GetOpportunityDataInCasePlanningBoard();
        Task<CasePlanningBoard> UpsertCasePlanningBoard(CasePlanningBoard casePlanningBoard);
        Task<IEnumerable<CasePlanningBoard>> UpsertCasePlanningBoardData(IEnumerable<CasePlanningBoard> casePlanningBoard);
        Task DeleteCasePlanningBoardByIds(string ids, string lastUpdatedBy);
        Task<CasePlanningBoardBucketPreferences> UpsertCasePlanningBoardBucketPreferences(CasePlanningBoardBucketPreferences prefrencesData);
        Task<bool> UpsertCasePlanningBoardIncludeInDemandPreferences(CasePlanningBoardProjectPreferences prefrencesData);
        Task<IEnumerable<CasePlanningProjectPreferences>> UpsertCasePlanningProjectDetails(CasePlanningProjectPreferences[] preferencesData);
        Task<IEnumerable<CasePlanningProjectPreferences>> GetCasePlanningProjectDetails(string oldCaseCodes, string pipelineIds, string planningCardIds);
    }
}
