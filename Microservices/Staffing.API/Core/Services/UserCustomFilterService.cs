using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class UserCustomFilterService : IUserCustomFilterService
    {
        private readonly IUserCustomFilterRepository _userCustomFilterRepository;

        public UserCustomFilterService(IUserCustomFilterRepository userCustomFilterRepository)
        {
            _userCustomFilterRepository = userCustomFilterRepository;
        }

        public async Task<IEnumerable<ResourceFilterViewModel>> GetCustomResourceFiltersByEmployeeCode(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                throw new ArgumentException("employeeCode cannot be null or empty");
            }
            return await _userCustomFilterRepository.GetCustomResourceFiltersByEmployeeCode(employeeCode);
        }
        public async Task<IEnumerable<ResourceFilterViewModel>> UpsertCustomResourceFilters(IEnumerable<ResourceFilterViewModel> upsertedResourceFilters)
        {

            upsertedResourceFilters = AssignIdToGroup(upsertedResourceFilters);
            var resourceFiltersDataTable = CreateResourceFiltersDataTable(upsertedResourceFilters);
            var savedGroupFiltersDataTable = CreateUserPreferencesSavedGroupFiltersDataTable(upsertedResourceFilters);

            return await _userCustomFilterRepository.UpsertCustomResourceFilters(resourceFiltersDataTable, savedGroupFiltersDataTable);
        }
        public async Task<string> DeleteCustomResourceFilterById(string filterIdToDelete, string lastUpdatedBy)
        {
            await _userCustomFilterRepository.DeleteCustomResourceFilterById(filterIdToDelete, lastUpdatedBy);
            return filterIdToDelete;
        }

        #region private Helper methods


        private static IEnumerable<ResourceFilterViewModel> AssignIdToGroup(IEnumerable<ResourceFilterViewModel> upsertedResourceFilters)
        {
            foreach (var resourceFilter in upsertedResourceFilters)
            {
                resourceFilter.Id = resourceFilter.Id ?? Guid.NewGuid();
                if (resourceFilter.FilterBy != null)
                {
                    foreach (var filterBy in resourceFilter.FilterBy)
                    {
                        filterBy.GroupId = (Guid)resourceFilter.Id;
                    }
                }
            }
            return upsertedResourceFilters;
        }
        private static DataTable CreateResourceFiltersDataTable(IEnumerable<ResourceFilterViewModel> upsertedResourceFilters)
        {
            var resourceFiltersDataTable = new DataTable();
            resourceFiltersDataTable.Columns.Add("id", typeof(Guid));
            resourceFiltersDataTable.Columns.Add("title", typeof(string));
            resourceFiltersDataTable.Columns.Add("description", typeof(string));
            resourceFiltersDataTable.Columns.Add("isDefault", typeof(string));
            resourceFiltersDataTable.Columns.Add("officeCodes", typeof(string));
            resourceFiltersDataTable.Columns.Add("staffingTags", typeof(string));
            resourceFiltersDataTable.Columns.Add("levelGrades", typeof(string));
            resourceFiltersDataTable.Columns.Add("positionCodes", typeof(string));
            resourceFiltersDataTable.Columns.Add("employeeStatuses", typeof(string));
            resourceFiltersDataTable.Columns.Add("practiceAreaCodes", typeof(string));
            resourceFiltersDataTable.Columns.Add("resourcesTabSortBy", typeof(string));
            resourceFiltersDataTable.Columns.Add("minAvailabilityThreshold", typeof(short));
            resourceFiltersDataTable.Columns.Add("maxAvailabilityThreshold", typeof(short));
            resourceFiltersDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            resourceFiltersDataTable.Columns.Add("staffableAsTypeCodes", typeof(string));
            resourceFiltersDataTable.Columns.Add("affiliationRoleCodes", typeof(string));

            foreach (var resourceFilter in upsertedResourceFilters)
            {
                var row = resourceFiltersDataTable.NewRow();
                row["id"] = (object)resourceFilter.Id ?? DBNull.Value;
                row["title"] = resourceFilter.Title;
                row["description"] = resourceFilter.Description;
                row["isDefault"] = resourceFilter.isDefault;
                row["officeCodes"] = (object)resourceFilter.OfficeCodes ?? DBNull.Value;
                row["staffingTags"] = (object)resourceFilter.StaffingTags ?? DBNull.Value;
                row["levelGrades"] = (object)resourceFilter.LevelGrades ?? DBNull.Value;
                row["positionCodes"] = (object)resourceFilter.PositionCodes ?? DBNull.Value;
                row["employeeStatuses"] = (object)resourceFilter.EmployeeStatuses ?? DBNull.Value;
                row["practiceAreaCodes"] = (object)resourceFilter.PracticeAreaCodes ?? DBNull.Value;
                row["resourcesTabSortBy"] = (object)resourceFilter.ResourcesTabSortBy ?? DBNull.Value;
                row["minAvailabilityThreshold"] = (object)resourceFilter.MinAvailabilityThreshold ?? DBNull.Value;
                row["maxAvailabilityThreshold"] = (object)resourceFilter.MaxAvailabilityThreshold ?? DBNull.Value;
                row["lastUpdatedBy"] = resourceFilter.LastUpdatedBy;
                row["staffableAsTypeCodes"] = resourceFilter.StaffableAsTypeCodes;
                row["affiliationRoleCodes"] = (object)resourceFilter.AffiliationRoleCodes ?? DBNull.Value;
                resourceFiltersDataTable.Rows.Add(row);
            }

            return resourceFiltersDataTable;
        }

        private static DataTable CreateUserPreferencesSavedGroupFiltersDataTable(IEnumerable<ResourceFilterViewModel> upsertedResourceFilters)
        {
            var savedGroupFiltersDataTable = new DataTable();
            savedGroupFiltersDataTable.Columns.Add("id", typeof(Guid));
            savedGroupFiltersDataTable.Columns.Add("groupId", typeof(Guid));
            savedGroupFiltersDataTable.Columns.Add("andOr", typeof(string));
            savedGroupFiltersDataTable.Columns.Add("filterField", typeof(string));
            savedGroupFiltersDataTable.Columns.Add("filterOperator", typeof(string));
            savedGroupFiltersDataTable.Columns.Add("filterValue", typeof(string));
            savedGroupFiltersDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var savedGroup in upsertedResourceFilters)
            {
                if (savedGroup.FilterBy != null)
                {
                    foreach (var savedGroupFilters in savedGroup.FilterBy)
                    {
                        var row = savedGroupFiltersDataTable.NewRow();
                        row["id"] = (object)savedGroupFilters.Id ?? DBNull.Value;
                        row["groupId"] = savedGroupFilters.GroupId;
                        row["andOr"] = savedGroupFilters.AndOr;
                        row["filterField"] = savedGroupFilters.FilterField;
                        row["filterOperator"] = savedGroupFilters.FilterOperator;
                        row["filterValue"] = savedGroupFilters.FilterValue;
                        row["lastUpdatedBy"] = savedGroup.LastUpdatedBy;

                        savedGroupFiltersDataTable.Rows.Add(row);
                    }
                }
            }
            return savedGroupFiltersDataTable;
        }

        #endregion
    }
}
