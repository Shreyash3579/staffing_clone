using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class UserPreferenceSupplyGroupService: IUserPreferenceSupplyGroupService
    {
        private readonly IUserPreferenceSupplyGroupRepository _userPreferenceSupplyGroupsRepository;

        public UserPreferenceSupplyGroupService(IUserPreferenceSupplyGroupRepository userPreferenceSupplyGroupsRepository)
        {
            _userPreferenceSupplyGroupsRepository = userPreferenceSupplyGroupsRepository;
        }

        public async Task<string> DeleteUserPreferenceSupplyGroupByIds(string listSupplyGroupIdsToDelete, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(listSupplyGroupIdsToDelete))
                return string.Empty;

            await _userPreferenceSupplyGroupsRepository.DeleteUserPreferenceSupplyGroupByIds(listSupplyGroupIdsToDelete, lastUpdatedBy);

            return listSupplyGroupIdsToDelete;
        }

        public async Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> GetUserPreferenceSupplyGroups(string employeeCode)
        {
            return await _userPreferenceSupplyGroupsRepository.GetUserPreferenceSupplyGroups(employeeCode);
        }

        public async Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> UpsertUserPreferenceSupplyGroups(IEnumerable<UserPreferenceSupplyGroupViewModel> supplyGroupsToUpsert)
        {
            if (supplyGroupsToUpsert == null || !supplyGroupsToUpsert.Any())
                return Enumerable.Empty<UserPreferenceSupplyGroupViewModel>();
            supplyGroupsToUpsert = AssignIdToGroup(supplyGroupsToUpsert);
            var supplyGroupsToUpsertDataTable = CreateUserPreferencesSupplyGroupsDataTable(supplyGroupsToUpsert);
            var supplyGroupsFiltersInfoToUpsertDataTable = CreateUserPreferencesSupplyGroupFilterInfoDataTable(supplyGroupsToUpsert);
            return await _userPreferenceSupplyGroupsRepository.UpsertUserPreferenceSupplyGroups(supplyGroupsToUpsertDataTable, supplyGroupsFiltersInfoToUpsertDataTable);
        }

        #region helper Methods

        private static IEnumerable<UserPreferenceSupplyGroupViewModel> AssignIdToGroup(IEnumerable<UserPreferenceSupplyGroupViewModel> supplyGroupsToUpsert)
        {
            foreach (var supplyGroup in supplyGroupsToUpsert)
            {
                supplyGroup.Id = supplyGroup.Id ?? Guid.NewGuid();
                if (supplyGroup.FilterBy != null)
                {
                    foreach (var filterBy in supplyGroup.FilterBy)
                    {
                        filterBy.GroupId = (Guid)supplyGroup.Id;
                    }
                }
            }
            return supplyGroupsToUpsert;
        }

        private static DataTable CreateUserPreferencesSupplyGroupsDataTable(IEnumerable<UserPreferenceSupplyGroupViewModel> supplyGroupsToUpsert)
        {
            var supplyGroupsDataTable = new DataTable();
            supplyGroupsDataTable.Columns.Add("id", typeof(Guid));
            supplyGroupsDataTable.Columns.Add("name", typeof(string));
            supplyGroupsDataTable.Columns.Add("description", typeof(string));
            supplyGroupsDataTable.Columns.Add("createdBy", typeof(string));
            supplyGroupsDataTable.Columns.Add("isDefault", typeof(bool));
            supplyGroupsDataTable.Columns.Add("isDefaultForResourcesTab", typeof(bool));
            supplyGroupsDataTable.Columns.Add("isShared", typeof(bool));
            supplyGroupsDataTable.Columns.Add("groupMemberCodes", typeof(string));
            supplyGroupsDataTable.Columns.Add("sortBy", typeof(string));
            supplyGroupsDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var supplyGroup in supplyGroupsToUpsert)
            {
                var row = supplyGroupsDataTable.NewRow();
                row["id"] = (object)supplyGroup.Id ?? DBNull.Value;
                row["name"] = supplyGroup.Name;
                row["description"] = (object)supplyGroup.Description ?? DBNull.Value;
                row["isDefault"] = supplyGroup.IsDefault;
                row["isDefaultForResourcesTab"] = supplyGroup.IsDefaultForResourcesTab;
                row["isShared"] = supplyGroup.IsShared;
                row["groupMemberCodes"] = supplyGroup.GroupMemberCodes;
                row["sortBy"] = supplyGroup.SortBy;
                row["createdBy"] = supplyGroup.CreatedBy;
                row["lastUpdatedBy"] = supplyGroup.LastUpdatedBy;

                supplyGroupsDataTable.Rows.Add(row);
            }

            return supplyGroupsDataTable;
        }

        private static DataTable CreateUserPreferencesSupplyGroupFilterInfoDataTable(IEnumerable<UserPreferenceSupplyGroupViewModel> supplyGroupsToUpsert)
        {
            var supplyGroupsFiltersInfoDataTable = new DataTable();
            supplyGroupsFiltersInfoDataTable.Columns.Add("id", typeof(Guid));
            supplyGroupsFiltersInfoDataTable.Columns.Add("groupId", typeof(Guid));
            supplyGroupsFiltersInfoDataTable.Columns.Add("andOr", typeof(string));
            supplyGroupsFiltersInfoDataTable.Columns.Add("filterField", typeof(string));
            supplyGroupsFiltersInfoDataTable.Columns.Add("filterOperator", typeof(string));
            supplyGroupsFiltersInfoDataTable.Columns.Add("filterValue", typeof(string));
            supplyGroupsFiltersInfoDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var supplyGroup in supplyGroupsToUpsert)
            {
                if (supplyGroup.FilterBy != null)
                {
                    foreach (var supplyGroupFilters in supplyGroup.FilterBy)
                    {
                        var row = supplyGroupsFiltersInfoDataTable.NewRow();
                        row["id"] = (object)supplyGroupFilters.Id ?? DBNull.Value;
                        row["groupId"] = supplyGroupFilters.GroupId;
                        row["andOr"] = supplyGroupFilters.AndOr;
                        row["filterField"] = supplyGroupFilters.FilterField;
                        row["filterOperator"] = supplyGroupFilters.FilterOperator;
                        row["filterValue"] = supplyGroupFilters.FilterValue;
                        row["lastUpdatedBy"] = supplyGroup.LastUpdatedBy;

                        supplyGroupsFiltersInfoDataTable.Rows.Add(row);
                    }
                }

            }

            return supplyGroupsFiltersInfoDataTable;
        }
        #endregion

    }
}
