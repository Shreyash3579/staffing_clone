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
    public class CasePlanningService : ICasePlanningService
    {
        private readonly ICasePlanningRepository _casePlanningRepository;

        public CasePlanningService(ICasePlanningRepository casePlanningRepository)
        {
            _casePlanningRepository = casePlanningRepository;
        }

        public async Task<CasePlanningBoardDataModel> GetCasePlanningBoardDataByDateRange(DateTime startDate, DateTime? endDate = null, string loggedInUser = null)
        {
            startDate = startDate.Equals(DateTime.MinValue) ? DateTime.Today.AddDays((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek) : startDate.Date;

            var casePlanningBoardData = await _casePlanningRepository.GetCasePlanningBoardDataByDateRange(startDate, endDate, loggedInUser);
            return casePlanningBoardData;
        }

        public async Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectEndDateAndBucketIds(DateTime startDate, string bucketIds)
        {
            startDate = startDate.Equals(DateTime.MinValue) ? DateTime.Today.AddDays((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek) : startDate.Date;
            var casePlanningBoardDataByProjectEndDateAndBucketIds = await _casePlanningRepository.GetCasePlanningBoardDataByProjectEndDateAndBucketIds(startDate, bucketIds);
            return casePlanningBoardDataByProjectEndDateAndBucketIds;

        }

        public async Task<IEnumerable<CasePlanningBoard>> GetCasePlanningBoardDataByProjectIds(string oldCaseCodes, string pipelineIds, string planningCardIds)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds) && string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<CasePlanningBoard>();

            var casePlanningBoardData = await _casePlanningRepository.GetCasePlanningBoardDataByProjectIds(oldCaseCodes, pipelineIds, planningCardIds);
            return casePlanningBoardData;
        }

        public async Task<IEnumerable<CasePlanningBoard>> GetOpportunityDataInCasePlanningBoard()
        {
            var casePlanningBoardData = await _casePlanningRepository.GetOpportunityDataInCasePlanningBoard();
            return casePlanningBoardData;
        }

        public async Task<CasePlanningBoard> UpsertCasePlanningBoard(CasePlanningBoard casePlanningBoard)
        {
            return await _casePlanningRepository.UpsertCasePlanningBoard(casePlanningBoard);
        }

        public async Task<IEnumerable<CasePlanningBoard>> UpsertCasePlanningBoardData(IEnumerable<CasePlanningBoard> casePlanningBoard)
        {
            if (casePlanningBoard == null || !casePlanningBoard.Any())
            {
                return Enumerable.Empty<CasePlanningBoard>();
            }

            var dataTable = ConvertCasePlanningBoardToDataTable(casePlanningBoard);
            var upsertedCasePlanningBoardData = await _casePlanningRepository.UpsertCasePlanningBoardData(dataTable);

            var upsertedData = upsertedCasePlanningBoardData.Where(x => x.BucketId == 2).FirstOrDefault();

            if (upsertedData != null)
            {
                var casePlanningBoardPRojectPrefernceData = new CasePlanningBoardProjectPreferences
                {
                    PlanningBoardId = upsertedData.Id,
                    LastUpdatedBy = upsertedData.LastUpdatedBy
                };
                await _casePlanningRepository.UpsertCasePlanningBoardPreferencesOnDrop(casePlanningBoardPRojectPrefernceData);
            }

            return upsertedCasePlanningBoardData;
        }

        public async Task DeleteCasePlanningBoardByIds(string ids, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(ids) || string.IsNullOrEmpty(lastUpdatedBy))
                throw new ArgumentException("Ids or lastUpdatedBy cannot be null or empty");

            await _casePlanningRepository.DeleteCasePlanningBoardByIds(ids, lastUpdatedBy);

        }
        public async Task<CasePlanningBoardBucketPreferences> UpsertCasePlanningBoardBucketPreferences(CasePlanningBoardBucketPreferences prefrencesData)
        {
            var updatedBuckets = await _casePlanningRepository.UpsertCasePlanningBoardBucketPreferences(prefrencesData);
            var casePlanningBoardPRojectPrefernceData = new CasePlanningBoardProjectPreferences
            {
                PlanningBoardId = null,
                EmployeeCode = prefrencesData.EmployeeCode,
                IncludeProjectInDemand = null,
                IsIncludeAll = true,
                IncludeBucketInDemand = prefrencesData.IncludeInDemand,
                LastUpdatedBy = prefrencesData.LastUpdatedBy
            };

            var updatedProjects = await _casePlanningRepository.UpsertCasePlanningBoardIncludeInDemandPreferences(casePlanningBoardPRojectPrefernceData);

            return updatedBuckets;
        }

        public async Task<bool> UpsertCasePlanningBoardIncludeInDemandPreferences(CasePlanningBoardProjectPreferences prefrencesData)
        {
            return await _casePlanningRepository.UpsertCasePlanningBoardIncludeInDemandPreferences(prefrencesData);
        }

        public async Task<IEnumerable<CasePlanningProjectPreferences>> UpsertCasePlanningProjectDetails(CasePlanningProjectPreferences[] preferencesData)
        {
            if (preferencesData == null || !preferencesData.Any())
            {
                return Enumerable.Empty<CasePlanningProjectPreferences>();
            }

            var dataTable = ConvertCasePlanningProjectDetailsToDataTable(preferencesData);
            var upsertedCasePlanningProjectDetailsData = await _casePlanningRepository.UpsertCasePlanningProjectDetails(dataTable);
            return upsertedCasePlanningProjectDetailsData;
        }

        public async Task<IEnumerable<CasePlanningProjectPreferences>> GetCasePlanningProjectDetails(string oldCaseCodes, string pipelineIds, string planningCardIds)
        {
            return await _casePlanningRepository.GetCasePlanningProjectDetails(oldCaseCodes, pipelineIds, planningCardIds);
        }

        #region private methods
        private static DataTable ConvertCasePlanningBoardToDataTable(IEnumerable<CasePlanningBoard> casePlanningBoardData)
        {
            var casePlanningBoardDataTable = new DataTable();
            casePlanningBoardDataTable.Columns.Add("id", typeof(Guid));
            casePlanningBoardDataTable.Columns.Add("date", typeof(DateTime));
            casePlanningBoardDataTable.Columns.Add("bucketId", typeof(int));
            casePlanningBoardDataTable.Columns.Add("projectEndDate", typeof(DateTime));
            casePlanningBoardDataTable.Columns.Add("pipelineId", typeof(Guid));
            casePlanningBoardDataTable.Columns.Add("oldCaseCode", typeof(string));
            casePlanningBoardDataTable.Columns.Add("planningCardId", typeof(Guid));
            casePlanningBoardDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var data in casePlanningBoardData)
            {
                var row = casePlanningBoardDataTable.NewRow();
                row["id"] = (object)data.Id ?? DBNull.Value;
                row["date"] = (object)data.Date ?? DBNull.Value;
                row["bucketId"] = (object)((int)data.BucketId) ?? DBNull.Value;
                row["projectEndDate"] = (object)data.ProjectEndDate ?? DBNull.Value;
                row["pipelineId"] = (object)data.PipelineId ?? DBNull.Value;
                row["oldCaseCode"] = (object)data.OldCaseCode ?? DBNull.Value;
                row["planningCardId"] = (object)data.PlanningCardId ?? DBNull.Value;
                row["lastUpdatedBy"] = data.LastUpdatedBy;

                casePlanningBoardDataTable.Rows.Add(row);
            }

            return casePlanningBoardDataTable;
        }

        private static DataTable ConvertCasePlanningProjectDetailsToDataTable(IEnumerable<CasePlanningProjectPreferences> casePlanningProjectPreferences)
        {
            var casePlanningProjectPreferncesDataTable = new DataTable();
            casePlanningProjectPreferncesDataTable.Columns.Add("id", typeof(Guid));
            casePlanningProjectPreferncesDataTable.Columns.Add("oldCaseCode", typeof(string));
            casePlanningProjectPreferncesDataTable.Columns.Add("pipelineId", typeof(Guid));
            casePlanningProjectPreferncesDataTable.Columns.Add("planningCardId", typeof(Guid));
            casePlanningProjectPreferncesDataTable.Columns.Add("includeInDemand", typeof(bool));
            casePlanningProjectPreferncesDataTable.Columns.Add("isFlagged", typeof(bool));
            casePlanningProjectPreferncesDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var data in casePlanningProjectPreferences)
            {
                var row = casePlanningProjectPreferncesDataTable.NewRow();
                row["id"] = (object)data.Id ?? DBNull.Value;
                row["oldCaseCode"] = (object)data.OldCaseCode ?? DBNull.Value;
                row["pipelineId"] = (object)(data.PipelineId) ?? DBNull.Value;
                row["planningCardId"] = (object)data.PlanningCardId ?? DBNull.Value;
                row["includeInDemand"] = (object)data.IncludeInDemand ?? DBNull.Value;
                row["isFlagged"] = (object)data.IsFlagged ?? DBNull.Value;
                row["lastUpdatedBy"] = data.LastUpdatedBy;

                casePlanningProjectPreferncesDataTable.Rows.Add(row);
            }

            return casePlanningProjectPreferncesDataTable;
        }

        #endregion
    }
}
