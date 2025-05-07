using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Linq;

namespace Staffing.API.Core.Repository
{
    public class CasePlanningRepository : ICasePlanningRepository
    {
        private readonly IBaseRepository<CasePlanningBoard> _baseRepository;

        public CasePlanningRepository(IBaseRepository<CasePlanningBoard> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<CasePlanningBoardDataModel> GetCasePlanningBoardDataByDateRange(DateTime startDate, DateTime? endDate = null, string loggedInUser = null)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
                StoredProcedureMap.GetCasePlanningBoardDataByDateRange,
                new
                {
                    startDate,
                    endDate,
                    loggedInUser
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var casePlanningBoardData = result.Read<CasePlanningBoard>().ToList();
            var casePlanningBoardPlanningCardData = result.Read<CasePlanningBoard>().ToList();

            var casePlanningAndPlanningCardsData = ConvertToCasePlanningBoardCompleteData(casePlanningBoardData, casePlanningBoardPlanningCardData);
            return casePlanningAndPlanningCardsData;
        }

        public async Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectEndDateAndBucketIds(DateTime startDate, string bucketIds)
        {
            var casePlanningBoardDataByProjectEndDateAndBucketIds = await _baseRepository.GetAllAsync(new { startDate, bucketIds }, StoredProcedureMap.GetCasePlanningBoardDataByProjectEndDateAndBucketIds);
            return casePlanningBoardDataByProjectEndDateAndBucketIds;
        }

        public async Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectIds(string oldCaseCodes, string pipelineIds, string planningCardIds)
        {
            var casePlanningBoardData = await _baseRepository.GetAllAsync(
                new
                {
                    oldCaseCodes,
                    pipelineIds,
                    planningCardIds
                }, StoredProcedureMap.GetCasePlanningBoardDataByProjectIds);
            return casePlanningBoardData;
        }

        public async Task<IEnumerable<CasePlanningBoard>> GetOpportunityDataInCasePlanningBoard()
        {
            var casePlanningBoardData = await _baseRepository.GetAllAsync(StoredProcedureMap.GetOpportunityDataInCasePlanningBoard);
            return casePlanningBoardData;
        }

        public async Task<CasePlanningBoard> UpsertCasePlanningBoard(CasePlanningBoard casePlanningBoard)
        {
            var upsertedCasePlanningBoard = await _baseRepository.UpsertAsync(StoredProcedureMap.UpsertCasePlanningBoard,
                new
                {
                    casePlanningBoard.Id,
                    casePlanningBoard.Date,
                    casePlanningBoard.BucketId,
                    casePlanningBoard.PipelineId,
                    casePlanningBoard.OldCaseCode,
                    casePlanningBoard.PlanningCardId,
                    casePlanningBoard.ProjectEndDate,
                    casePlanningBoard.LastUpdatedBy
                });

            return upsertedCasePlanningBoard;
        }

        public async Task<IEnumerable<CasePlanningBoard>> UpsertCasePlanningBoardData(DataTable casePlanningBoardDataTable)
        {
            var upsertedCasePlanningBoard = await _baseRepository.Context.Connection.QueryAsync<CasePlanningBoard>(
                StoredProcedureMap.UpsertCasePlanningBoardData,
                new
                {
                    casePlanningBoard =
                        casePlanningBoardDataTable.AsTableValuedParameter(
                            "[dbo].[casePlanningBoardTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedCasePlanningBoard;
        }

        public async Task DeleteCasePlanningBoardByIds(string ids, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { ids, lastUpdatedBy }, StoredProcedureMap.DeleteCasePlanningBoardByIds);
        }

        public async Task<CasePlanningBoardBucketPreferences> UpsertCasePlanningBoardBucketPreferences(CasePlanningBoardBucketPreferences prefrencesData)
        {
            var upsertedPrefrences = await _baseRepository.Context.Connection.QueryAsync<CasePlanningBoardBucketPreferences>(
                StoredProcedureMap.UpsertCasePlanningBoardBucketPreferences,
                new
                {
                    prefrencesData.EmployeeCode,
                    prefrencesData.BucketId,
                    prefrencesData.IncludeInDemand,
                    prefrencesData.IsPartiallyChecked,
                    prefrencesData.LastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedPrefrences.FirstOrDefault();
        }

        public async Task<bool> UpsertCasePlanningBoardIncludeInDemandPreferences(CasePlanningBoardProjectPreferences prefrencesData)
        {
            var isIncludeInDemandPartiallyChecked = await _baseRepository.Context.Connection.QueryAsync<CasePlanningBoardBucketPreferences>(
                StoredProcedureMap.UpsertCasePlanningBoardIncludeInDemandPreferences,
                new
                {
                    prefrencesData.PlanningBoardId,
                    prefrencesData.EmployeeCode,
                    prefrencesData.IncludeProjectInDemand,
                    prefrencesData.IsIncludeAll,
                    prefrencesData.IncludeBucketInDemand,
                    prefrencesData.LastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return isIncludeInDemandPartiallyChecked.FirstOrDefault().IsPartiallyChecked;
        }

        public async Task<IEnumerable<CasePlanningProjectPreferences>> UpsertCasePlanningProjectDetails(DataTable prefrencesDataTable)
        {
            var upsertedcasePlanningProjectPreferences = await _baseRepository.Context.Connection.QueryAsync<CasePlanningProjectPreferences>(
                StoredProcedureMap.UpsertCasePlanningProjectDetails,
                new
                {
                    CasePlanningProjectPreferences =
                        prefrencesDataTable.AsTableValuedParameter(
                            "[dbo].[casePlanningProjectPreferencesTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedcasePlanningProjectPreferences;
        }

        public async Task<IEnumerable<CasePlanningProjectPreferences>> GetCasePlanningProjectDetails(string oldCaseCodes, string pipelineIds, string planningCardIds)
        {
            var casePlanningProjectDetails = await _baseRepository.Context.Connection.QueryAsync<CasePlanningProjectPreferences>(
                StoredProcedureMap.GetCasePlanningProjectDetails,
                new
                {
                    oldCaseCodes,
                    pipelineIds,
                    planningCardIds
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return casePlanningProjectDetails;
        }

        public async Task<CasePlanningBoardProjectPreferences> UpsertCasePlanningBoardPreferencesOnDrop(CasePlanningBoardProjectPreferences prefrencesData)
        {
            var result = await _baseRepository.Context.Connection.QueryAsync<CasePlanningBoardProjectPreferences>(
                StoredProcedureMap.UpsertCasePlanningBoardPreferencesOnDrop,
                new
                {
                    prefrencesData.PlanningBoardId,
                    prefrencesData.LastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return result.FirstOrDefault();
        }

    private CasePlanningBoardDataModel ConvertToCasePlanningBoardCompleteData(List<CasePlanningBoard> casePlanningBoardData , List<CasePlanningBoard> casePlanningBoardPlanningCardData)
        {
            var casePlanningAndPlanningCardsData = new CasePlanningBoardDataModel
            {
                CasePlanningBoardData = casePlanningBoardData,
                CasePlanningBoardPlanningCardData = casePlanningBoardPlanningCardData
            };

            return casePlanningAndPlanningCardsData;
        }
    }
}
