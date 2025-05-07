using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IUserPreferenceGroupSharedInfoService
    {
        Task<IEnumerable<UserPreferenceGroupSharedInfo>> GetUserPreferenceGroupSharedInfo(string groupId);

        Task<IEnumerable<UserPreferenceGroupSharedInfo>> UpsertUserPreferenceGroupSharedInfo(IEnumerable<UserPreferenceGroupSharedInfo> supplyGroups);
        Task<IEnumerable<UserPreferenceGroupSharedInfo>> UpdateUserPreferenceSupplyGroupSharedInfo(IEnumerable<UserPreferenceGroupSharedInfo> supplyGroups);
    }
}
