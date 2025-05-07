using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface ISecurityUserService
    {
        Task<IEnumerable<SecurityUser>> GetAllSecurityUsers();
        Task<IEnumerable<SecurityGroup>> GetAllSecurityGroups();
        Task<List<string>> GetGroupNamesBySearchString(string searchString);
        Task DeleteSecurityUserByEmployeeCode(string employeeCode, string lastUpdatedBy);
        Task<SecurityUser> UpsertBOSSSecurityUser(SecurityUser securityUser);
        Task<SecurityGroup> UpsertBOSSSecurityGroup(SecurityGroup securityGroup);
        Task DeleteSecurityGroupById(string groupIdToDelete, string lastUpdatedBy);
        Task<IEnumerable<RevOffice>> GetRevOfficeList();

        Task<IEnumerable<ServiceLineHierarchy>> GetServiceLineList();

        Task SaveServiceLineList(IEnumerable<ServiceLineHierarchy> serviceLineList);
        Task saveRevOfficeList(IEnumerable<RevOffice> officeList);

        Task UpdateSecurityUserForWFPRole(IEnumerable<OfficeHierarchy> officeList, IEnumerable<ServiceLineHierarchy> newServiceLines);
    }
}
