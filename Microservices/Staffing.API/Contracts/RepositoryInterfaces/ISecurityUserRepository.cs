using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ISecurityUserRepository
    {
        Task<IEnumerable<SecurityUser>> GetAllSecurityUsers();
        Task DeleteSecurityUserByEmployeeCode(string employeeCode, string lastUpdatedBy);
        Task<SecurityUser> UpsertBOSSSecurityUser(SecurityUser securityUser);
        Task<IEnumerable<SecurityGroup>> GetAllSecurityGroups();
        Task<SecurityGroup> UpsertBOSSSecurityGroup(SecurityGroup securityGroup);
        Task DeleteSecurityGroupById(string groupIdToDelete, string lastUpdatedBy);

        Task<IEnumerable<RevOffice>> GetAllRevOffices();

        Task saveRevOfficeList(DataTable officeDataTable);

        Task UpdateSecurityUserForWFPRole(DataTable officeDataTable, DataTable serviceLineDataTable);

        Task<IEnumerable<ServiceLineHierarchy>> GetServiceLineList();

        Task saveServiceLineList(DataTable serviceLineDataTable);

    }
}
