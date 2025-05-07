using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IUserPreferenceGroupSharedInfoRepository
    {
        Task<IEnumerable<UserPreferenceGroupSharedInfo>> GetUserPreferenceGroupSharedInfo(string groupId);
        Task<IEnumerable<UserPreferenceGroupSharedInfo>> UpsertUserPreferenceGroupSharedInfo(DataTable supplyGroupsSharedInfoToUpsertDataTable);
        Task<IEnumerable<UserPreferenceGroupSharedInfo>> UpdateUserPreferenceSupplyGroupSharedInfo(DataTable supplyGroupsSharedInfoToUpsertDataTable);
    }
}
