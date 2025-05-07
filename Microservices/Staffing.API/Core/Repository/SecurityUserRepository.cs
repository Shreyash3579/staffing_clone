using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class SecurityUserRepository : ISecurityUserRepository
    {
        private readonly IBaseRepository<SecurityUser> _baseRepository;

        public SecurityUserRepository(IBaseRepository<SecurityUser> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;

        }

        public async Task<IEnumerable<SecurityUser>> GetAllSecurityUsers()
        {
            var securityUsers = await _baseRepository.GetAllAsync(StoredProcedureMap.GetAllSecurityUsers);
            return securityUsers;
        }

        public async Task DeleteSecurityUserByEmployeeCode(string employeeCode, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { employeeCodes = employeeCode, lastUpdatedBy }, StoredProcedureMap.DeleteSecurityUser);
        }

        public async Task<SecurityUser> UpsertBOSSSecurityUser(SecurityUser securityUser)
        {
            var updatedSecurityUser = await _baseRepository.UpdateAsync(
                    new
                    {
                        securityUser.EmployeeCode,
                        securityUser.RoleCodes,
                        securityUser.IsAdmin,
                        securityUser.LastUpdatedBy,
                        securityUser.Override,
                        securityUser.Notes,
                        securityUser.EndDate,
                        securityUser.UserTypeCode,
                        securityUser.GeoType,
                        securityUser.OfficeCodes,
                        securityUser.ServiceLineCodes,
                        securityUser.PositionGroupCodes,
                        securityUser.LevelGrades,
                        securityUser.PracticeAreaCodes,
                        securityUser.RingfenceCodes,
                        securityUser.HasAccessToAISearch,
                        securityUser.HasAccessToStaffingInsightsTool,
                        securityUser.HasAccessToRetiredStaffingTab
                    },
                    StoredProcedureMap.UpsertBOSSSecurityUser
                );
            return updatedSecurityUser;
        }

        public async Task<IEnumerable<SecurityGroup>> GetAllSecurityGroups()
        {
            var securityGroups = await Task.Run(() => _baseRepository.Context.Connection.Query<SecurityGroup>(
                StoredProcedureMap.GetAllSecurityGroups,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return securityGroups;

        }
        public async Task<SecurityGroup> UpsertBOSSSecurityGroup(SecurityGroup securityGroup)
        {
            var upsertedSecurityGroup = await _baseRepository.Context.Connection.QueryFirstAsync<SecurityGroup>(
               StoredProcedureMap.UpsertBOSSSecurityGroup,
               new
               {
                   securityGroup.Id,
                   securityGroup.GroupName,
                   securityGroup.RoleCodes,
                   securityGroup.FeatureCodes,
                   securityGroup.Notes,
                   securityGroup.LastUpdatedBy
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedSecurityGroup;
        }

        public async Task DeleteSecurityGroupById(string groupIdToDelete, string lastUpdatedBy)
        {
           await _baseRepository.DeleteAsync(new { groupIdToDelete, lastUpdatedBy }, StoredProcedureMap.DeleteSecurityGroup);
        }

        public async Task<IEnumerable<RevOffice>> GetAllRevOffices()
        {
            var revOffices = await Task.Run(() => _baseRepository.Context.Connection.Query<RevOffice>(
                StoredProcedureMap.GetAllRevOfficeList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return revOffices;

        }

        public async Task<IEnumerable<ServiceLineHierarchy>> GetServiceLineList()
        {
            var revOffices = await Task.Run(() => _baseRepository.Context.Connection.Query<ServiceLineHierarchy>(
                StoredProcedureMap.GetServiceLineList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return revOffices;

        }




        public async Task saveRevOfficeList(DataTable officeDataTable)
        {
            var upsertedOffices = await _baseRepository.Context.Connection.QueryAsync<RevOffice>(
               StoredProcedureMap.UpsertRevOffices,              
               new
               {
                   officeList =
                       officeDataTable.AsTableValuedParameter(
                           "[ccm].[officeTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return ;
        }

        public async Task saveServiceLineList(DataTable serviceLineDataTable)
        {
            var upsertedServiceLineList = await _baseRepository.Context.Connection.QueryAsync<RevOffice>(
               StoredProcedureMap.UpsertServiceLineList,
               new
               {
                   serviceLineList =
                       serviceLineDataTable.AsTableValuedParameter(
                           "[workday].[serviceLineTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return;
        }


        public async Task UpdateSecurityUserForWFPRole(DataTable officeDataTable, DataTable serviceLineDataTable)
        {
            // Upsert office hierarchy data
            var upsertedOffices = await _baseRepository.Context.Connection.QueryAsync<OfficeHierarchy>(
                StoredProcedureMap.UpdateSecurityUserForWFPRole,  // Stored procedure name
                new
                {
                    OfficeHierarchy = officeDataTable.AsTableValuedParameter("[ccm].[OfficesHierarchyTableType]"), // Office Hierarchy parameter
                    ServiceLineHierarchy = serviceLineDataTable.AsTableValuedParameter("[workday].[ServiceLineHierarchyTableType]") // Service Line Hierarchy parameter
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod
            );
            return;
        }

    }
}
