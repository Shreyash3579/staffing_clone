using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IUserPreferenceSupplyGroupService
    {
        Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> GetUserPreferenceSupplyGroups(string employeeCode);
        Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> UpsertUserPreferenceSupplyGroups(IEnumerable<UserPreferenceSupplyGroupViewModel> supplyGroups);
        Task<string> DeleteUserPreferenceSupplyGroupByIds(string listSupplyGroupIdsToDelete, string lastUpdatedBy);
    }
}
