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
    public class UserPreferenceGroupSharedInfoService : IUserPreferenceGroupSharedInfoService
    {
        private readonly IUserPreferenceGroupSharedInfoRepository _userPreferenceGroupSharedInfoRepository;

        public UserPreferenceGroupSharedInfoService(IUserPreferenceGroupSharedInfoRepository userPreferenceSupplyGroupSharedInfoRepository)
        {
            _userPreferenceGroupSharedInfoRepository = userPreferenceSupplyGroupSharedInfoRepository;
        }

        public async Task<IEnumerable<UserPreferenceGroupSharedInfo>> GetUserPreferenceGroupSharedInfo(string groupId)
        {
            return await _userPreferenceGroupSharedInfoRepository.GetUserPreferenceGroupSharedInfo(groupId);
        }

        public async Task<IEnumerable<UserPreferenceGroupSharedInfo>> UpsertUserPreferenceGroupSharedInfo(IEnumerable<UserPreferenceGroupSharedInfo> groupsSharedInfoToUpsert)
        {
            if (groupsSharedInfoToUpsert == null || !groupsSharedInfoToUpsert.Any())
                return Enumerable.Empty<UserPreferenceGroupSharedInfo>();

            var groupsSharedInfoToUpsertDataTable = CreateUserPreferencesGroupSharedInfoDataTable(groupsSharedInfoToUpsert);
            return await _userPreferenceGroupSharedInfoRepository.UpsertUserPreferenceGroupSharedInfo(groupsSharedInfoToUpsertDataTable);

        }

        public async Task<IEnumerable<UserPreferenceGroupSharedInfo>> UpdateUserPreferenceSupplyGroupSharedInfo(IEnumerable<UserPreferenceGroupSharedInfo> supplyGroupsSharedInfoToUpsert)
        {
            if (supplyGroupsSharedInfoToUpsert == null || !supplyGroupsSharedInfoToUpsert.Any())
                return Enumerable.Empty<UserPreferenceGroupSharedInfo>();

            var supplyGroupsSharedInfoToUpsertDataTable = CreateUserPreferencesGroupSharedInfoDataTable(supplyGroupsSharedInfoToUpsert);
            return await _userPreferenceGroupSharedInfoRepository.UpdateUserPreferenceSupplyGroupSharedInfo(supplyGroupsSharedInfoToUpsertDataTable);

        }

        private static DataTable CreateUserPreferencesGroupSharedInfoDataTable(IEnumerable<UserPreferenceGroupSharedInfo> supplyGroupsSharedInfoToUpsert)
        {
            var supplyGroupsSharedInfoDataTable = new DataTable();
            supplyGroupsSharedInfoDataTable.Columns.Add("id", typeof(Guid));
            supplyGroupsSharedInfoDataTable.Columns.Add("sharedWith", typeof(string));
            supplyGroupsSharedInfoDataTable.Columns.Add("userPreferenceGroupId", typeof(Guid));
            supplyGroupsSharedInfoDataTable.Columns.Add("lastUpdatedBy", typeof(string));


            foreach (var supplyGroupSharedInfo in supplyGroupsSharedInfoToUpsert)
            {
                var row = supplyGroupsSharedInfoDataTable.NewRow();
                row["id"] = (object)supplyGroupSharedInfo.Id ?? DBNull.Value;
                row["sharedWith"] = supplyGroupSharedInfo.SharedWith;
                row["userPreferenceGroupId"] = supplyGroupSharedInfo.UserPreferenceGroupId;
                row["lastUpdatedBy"] = supplyGroupSharedInfo.LastUpdatedBy;

                supplyGroupsSharedInfoDataTable.Rows.Add(row);
            }

            return supplyGroupsSharedInfoDataTable;
        }
    }
}
