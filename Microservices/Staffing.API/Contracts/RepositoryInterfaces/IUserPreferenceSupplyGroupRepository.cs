using Staffing.API.Models;
using Staffing.API.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IUserPreferenceSupplyGroupRepository
    {
        Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> GetUserPreferenceSupplyGroups(string employeeCode);
        Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> UpsertUserPreferenceSupplyGroups(DataTable supplyGroupsToUpsertDataTable, DataTable supplyGroupsFiltersInfoToUpsertDataTable);
        Task DeleteUserPreferenceSupplyGroupByIds(string listSupplyGroupIdsToDelete, string lastUpdatedBy);
    }
}
