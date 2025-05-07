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
    public class UserCustomFilterRepository : IUserCustomFilterRepository
    {
        private readonly IBaseRepository<ResourceFilter> _baseRepository;

        public UserCustomFilterRepository(IBaseRepository<ResourceFilter> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<ResourceFilterViewModel>> GetCustomResourceFiltersByEmployeeCode(string employeeCode)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
                StoredProcedureMap.GetUserCustomResourceFiltersByEmployeeCode,
                new 
                { 
                    employeeCode 
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var userResourceFilters = result.Read<ResourceFilter>().ToList();
            var savedGroupFilters = result.Read<UserPreferenceGroupFilters>().ToList();
            var savedGroupSharedInfo = result.Read<UserPreferenceGroupSharedInfo>().ToList();

            var userPreferenceGroups = ConvertToUserPreferenceViewModel(userResourceFilters, savedGroupFilters, savedGroupSharedInfo);
            return userPreferenceGroups;
        }

        public async Task<IEnumerable<ResourceFilterViewModel>> UpsertCustomResourceFilters(DataTable upsertedResourceFilters, DataTable savedGroupFiltersDataTable)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
                StoredProcedureMap.UpsertUserCustomResourceFilter,
                new
                {
                    userResourceFilters =
                        upsertedResourceFilters.AsTableValuedParameter(
                            "[dbo].[userCustomResourceFilterTableType]"),
                    userPreferencesGroupFilters =
                        savedGroupFiltersDataTable.AsTableValuedParameter(
                            "[dbo].[userPreferenceGroupFiltersTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var userResourceFilters = result.Read<ResourceFilter>().ToList();
            var savedGroupFilters = result.Read<UserPreferenceGroupFilters>().ToList();
            var savedGroupSharedInfo = result.Read<UserPreferenceGroupSharedInfo>().ToList();

            var userPreferenceGroups = ConvertToUserPreferenceViewModel(userResourceFilters, savedGroupFilters, savedGroupSharedInfo);

            return userPreferenceGroups;

        }
        public async Task<string> DeleteCustomResourceFilterById(string filterIdToDelete, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { filterIdToDelete, lastUpdatedBy }, StoredProcedureMap.DeleteUserCustomResourceFilters);

            return filterIdToDelete;
        }

        #region private methods
        private IEnumerable<ResourceFilterViewModel> ConvertToUserPreferenceViewModel(List<ResourceFilter> userResourceFilters, 
            List<UserPreferenceGroupFilters> savedGroupFilters, List<UserPreferenceGroupSharedInfo> savedGroupsSharedInfo)
        {
            var userPreferenceGroups = userResourceFilters.Select(item => new ResourceFilterViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                isDefault = item.isDefault,
                OfficeCodes = item.OfficeCodes,
                StaffingTags = item.StaffingTags,
                LevelGrades = item.LevelGrades,
                PositionCodes = item.PositionCodes,
                EmployeeStatuses = item.EmployeeStatuses,
                PracticeAreaCodes = item.PracticeAreaCodes,
                MinAvailabilityThreshold = item.MinAvailabilityThreshold,
                MaxAvailabilityThreshold = item.MaxAvailabilityThreshold,
                LastUpdatedBy = item.LastUpdatedBy,
                StaffableAsTypeCodes = item.StaffableAsTypeCodes,
                AffiliationRoleCodes = item.AffiliationRoleCodes,
                ResourcesTabSortBy = item.ResourcesTabSortBy,
                FilterBy = savedGroupFilters.Where(filter => filter.GroupId == item.Id),
                SharedWith = savedGroupsSharedInfo?.Where(sharedWith => sharedWith.UserPreferenceGroupId == item.Id)
            });

            return userPreferenceGroups;
        }

        #endregion
    }
}
