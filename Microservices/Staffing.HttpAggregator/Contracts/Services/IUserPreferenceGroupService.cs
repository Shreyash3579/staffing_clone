using Staffing.HttpAggregator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IUserPreferenceGroupService
    {
        Task<IList<UserPreferenceSupplyGroupViewModel>> GetUserPreferenceSupplyGroupsDetails(string employeeCode);
        Task<IList<UserPreferenceSavedGroupWithSharedInfo>> GetUserPreferenceSavedGroupsDetails(string employeeCode);
        Task<IList<UserPreferenceGroupSharedInfoViewModel>> GetUserPreferenceGroupSharedInfo(string groupId);
        Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> UpsertUserPreferencesSupplyGroupWithSharedInfo(IEnumerable<UserPreferenceSupplyGroupWithSharedInfo> supplyGroupsWithSharedInfoToUpsert);
        Task<IEnumerable<UserPreferenceSavedGroupWithSharedInfo>> UpsertUserPreferencesSavedGroupWithSharedInfo(IEnumerable<UserPreferenceSavedGroupWithSharedInfo> savedGroupsWithSharedInfoToUpsert);
        Task<IEnumerable<UserPreferenceGroupSharedInfoViewModel>> UpsertUserPreferenceGroupSharedInfo(IEnumerable<UserPreferenceGroupSharedInfo> groupsSharedInfoToUpsert);
    }
}
