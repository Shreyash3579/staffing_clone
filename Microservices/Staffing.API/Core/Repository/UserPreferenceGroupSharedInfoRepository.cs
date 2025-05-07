using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class UserPreferenceGroupSharedInfoRepository : IUserPreferenceGroupSharedInfoRepository
    {
        private readonly IBaseRepository<UserPreferenceGroupSharedInfo> _baseRepository;

        public UserPreferenceGroupSharedInfoRepository(IBaseRepository<UserPreferenceGroupSharedInfo> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<UserPreferenceGroupSharedInfo>> GetUserPreferenceGroupSharedInfo(string groupId)
        {
            var userPreferenceGroupSharedInfo = await _baseRepository.GetAllAsync(
                    new { groupId },
                    StoredProcedureMap.GetUserPreferenceGroupSharedInfo
                );

            return userPreferenceGroupSharedInfo;
        }

        public async Task<IEnumerable<UserPreferenceGroupSharedInfo>> UpsertUserPreferenceGroupSharedInfo(DataTable groupsSharedInfoToUpsertDataTable)
        {
            var upsertedGroupSharedInfo = await _baseRepository.Context.Connection.QueryAsync<UserPreferenceGroupSharedInfo>(
                StoredProcedureMap.UpsertUserPreferenceGroupSharedInfo,
                new
                {
                    userPreferencesGroupsSharedInfo =
                        groupsSharedInfoToUpsertDataTable.AsTableValuedParameter(
                            "[dbo].[UserPreferenceGroupSharedInfoTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedGroupSharedInfo;
        }

        public async Task<IEnumerable<UserPreferenceGroupSharedInfo>> UpdateUserPreferenceSupplyGroupSharedInfo(DataTable supplyGroupsSharedInfoToUpsertDataTable)
        {
            var updatedSupplyGroupSharedInfo = await _baseRepository.Context.Connection.QueryAsync<UserPreferenceGroupSharedInfo>(
                StoredProcedureMap.UpdateUserPreferenceSupplyGroupSharedInfo,
                new
                {
                    userPreferencesSupplyGroupsSharedInfo =
                        supplyGroupsSharedInfoToUpsertDataTable.AsTableValuedParameter(
                            "[dbo].[UserPreferenceSupplyGroupSharedInfoTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return updatedSupplyGroupSharedInfo;
        }
    }
}