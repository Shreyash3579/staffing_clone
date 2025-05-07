using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class UserPreferenceSupplyGroupRepository: IUserPreferenceSupplyGroupRepository
    {
        private readonly IBaseRepository<UserPreferenceSupplyGroup> _baseRepository;

        public UserPreferenceSupplyGroupRepository(IBaseRepository<UserPreferenceSupplyGroup> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> GetUserPreferenceSupplyGroups(string employeeCode)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
                StoredProcedureMap.GetUserPreferenceSupplyGroups,
                new
                {
                    employeeCode
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var supplyGroups = result.Read<UserPreferenceSupplyGroup>().ToList();
            var supplyGroupFilters = result.Read<UserPreferenceGroupFilters>().ToList();
            var supplyGroupsSharedInfo = result.Read<UserPreferenceGroupSharedInfo>().ToList();

            var userPreferenceSupplyGroups = ConvertToUserPreferenceViewModel(supplyGroups, supplyGroupFilters, supplyGroupsSharedInfo);
            return userPreferenceSupplyGroups;
        }

        public async Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> UpsertUserPreferenceSupplyGroups(DataTable supplyGroupsToUpsertDataTable, 
            DataTable supplyGroupsFiltersInfoToUpsertDataTable)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
                StoredProcedureMap.UpsertUserPreferenceSupplyGroups,
                new
                {
                    userPreferencesSupplyGroups =
                        supplyGroupsToUpsertDataTable.AsTableValuedParameter(
                            "[dbo].[UserPreferenceSupplyGroupTableType]"),
                    userPreferencesGroupFilters =
                        supplyGroupsFiltersInfoToUpsertDataTable.AsTableValuedParameter(
                            "[dbo].[userPreferenceGroupFiltersTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var supplyGroups = result.Read<UserPreferenceSupplyGroup>().ToList();
            var supplyGroupFilters = result.Read<UserPreferenceGroupFilters>().ToList();
            var supplyGroupsSharedInfo = result.Read<UserPreferenceGroupSharedInfo>().ToList();

            var userPreferenceSupplyGroups = ConvertToUserPreferenceViewModel(supplyGroups, supplyGroupFilters, supplyGroupsSharedInfo);

            return userPreferenceSupplyGroups;
        }

        public async Task DeleteUserPreferenceSupplyGroupByIds(string listSupplyGroupIdsToDelete, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { listSupplyGroupIdsToDelete, lastUpdatedBy }, StoredProcedureMap.DeleteUserPreferenceSupplyGroupByIds);
        }

        #region private methods
        private IEnumerable<UserPreferenceSupplyGroupViewModel> ConvertToUserPreferenceViewModel(List<UserPreferenceSupplyGroup> supplyGroups, 
            List<UserPreferenceGroupFilters> supplyGroupFilters, List<UserPreferenceGroupSharedInfo> supplyGroupsSharedInfo)
        {
            var userPreferenceGroups = supplyGroups.Select(item => new UserPreferenceSupplyGroupViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                CreatedBy = item.CreatedBy,
                IsDefault = item.IsDefault,
                IsDefaultForResourcesTab = item.IsDefaultForResourcesTab,
                IsShared = item.IsShared,
                GroupMemberCodes = item.GroupMemberCodes,
                SortBy = item.SortBy,
                LastUpdatedBy = item.LastUpdatedBy,
                FilterBy = supplyGroupFilters.Where(filter => filter.GroupId == item.Id),
                SharedWith = supplyGroupsSharedInfo?.Where(sharedWith => sharedWith.UserPreferenceGroupId == item.Id)
            });

            return userPreferenceGroups;
        }

        #endregion
    }
}