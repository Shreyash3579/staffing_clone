using Microservices.Common.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class SecurityUserService: ISecurityUserService
    {
        private readonly ISecurityUserRepository _securityUserRepository;
        private readonly AppSettingsConfiguration _appSettings;
        private readonly AzureADHelper _azureADHelper;

        public SecurityUserService(ISecurityUserRepository securityUserRepository, IOptionsSnapshot<AppSettingsConfiguration> appSettings)
        {
            //_appSettings = appSettings.Value;

            //var _clientId = _appSettings.AzureADCredentials["ClientId"];
            //var _tenantId = _appSettings.AzureADCredentials["TenantId"];
            //var _clientSecret = _appSettings.AzureADCredentials["ClientSecret"];

            //_azureADHelper = new AzureADHelper(_clientId, _tenantId, _clientSecret);
            _securityUserRepository = securityUserRepository;
        }

        public async Task<IEnumerable<SecurityUser>> GetAllSecurityUsers()
        {
            return await _securityUserRepository.GetAllSecurityUsers();
        }

        public async Task DeleteSecurityUserByEmployeeCode(string employeeCode, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(employeeCode)) throw new ArgumentException("EmployeeCode cannot be null or empty");
            await _securityUserRepository.DeleteSecurityUserByEmployeeCode(employeeCode, lastUpdatedBy);
            return;
        }

        public async Task<SecurityUser> UpsertBOSSSecurityUser(SecurityUser securityUser)
        {
            if (string.IsNullOrEmpty(securityUser.EmployeeCode))
                throw new ArgumentException("EmployeeCode cannot be null or empty");
            var upsertedSecurityUser = await _securityUserRepository.UpsertBOSSSecurityUser(securityUser);
            return upsertedSecurityUser;
        }

        public async Task<IEnumerable<SecurityGroup>> GetAllSecurityGroups()
        {
            return await _securityUserRepository.GetAllSecurityGroups();
        }

        public async Task<List<string>> GetGroupNamesBySearchString(string searchString)
        {
            return await _azureADHelper.GetGroupNamesBySearchString(searchString);
        }

        public async Task<SecurityGroup> UpsertBOSSSecurityGroup(SecurityGroup securityGroup)
        {
            if (string.IsNullOrEmpty(securityGroup.GroupName))
                throw new ArgumentException("Group name cannot be null or empty");
            var upsertedSecurityGroup = await _securityUserRepository.UpsertBOSSSecurityGroup(securityGroup);
            return upsertedSecurityGroup;
        }

        public async Task DeleteSecurityGroupById(string groupIdToDelete, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(groupIdToDelete)) 
                throw new ArgumentException("Group Id cannot be null or empty");
            
            await _securityUserRepository.DeleteSecurityGroupById(groupIdToDelete, lastUpdatedBy);
            return;
        }

        public async Task<IEnumerable<RevOffice>> GetRevOfficeList()
        {

            return await _securityUserRepository.GetAllRevOffices();
        }

        public async Task<IEnumerable<ServiceLineHierarchy>> GetServiceLineList()
        {

            return await _securityUserRepository.GetServiceLineList();
        }

        public async Task SaveServiceLineList( IEnumerable<ServiceLineHierarchy> serviceLineList)
        {
            DataTable serviceLineDataTable = ConvertServiceLineListToDataTable(serviceLineList);
             await _securityUserRepository.saveServiceLineList(serviceLineDataTable);

            return;
        }



        public async Task saveRevOfficeList(IEnumerable<RevOffice> officeList)
        {
            DataTable officeDataTable = ConvertRevOfficeListToDataTable(officeList);
            await _securityUserRepository.saveRevOfficeList(officeDataTable);

            return;
        }


        public async Task UpdateSecurityUserForWFPRole(IEnumerable<OfficeHierarchy> officeList, IEnumerable<ServiceLineHierarchy> newServiceLines)
        {
            DataTable officeDataTable = ConvertRevOfficeHierarchyToDataTable(officeList);
            DataTable serviceLineDataTable = ConvertServiceLineHierarchyToDataTable(newServiceLines);
            await _securityUserRepository.UpdateSecurityUserForWFPRole(officeDataTable, serviceLineDataTable);
            return;
        }

        public DataTable ConvertRevOfficeHierarchyToDataTable(IEnumerable<OfficeHierarchy> offices)
        {
            // Create a new DataTable instance
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("OfficeCode", typeof(string));
            dataTable.Columns.Add("ParentOfficeCodes", typeof(string));

            // Loop through each RevOffice object in the input list and populate the rows of the DataTable
            foreach (var office in offices)
            {
                DataRow row = dataTable.NewRow();
                row["OfficeCode"] = office.OfficeCode;
                row["ParentOfficeCodes"] = office.ParentOfficeCodes;

                // Add the row to the DataTable
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
        public DataTable ConvertServiceLineHierarchyToDataTable(IEnumerable<ServiceLineHierarchy> serviceLines)
        {
            // Create a new DataTable instance
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("ServiceLineCode", typeof(string));
            dataTable.Columns.Add("ParentServiceLineCodes", typeof(string));

            // Loop through each RevOffice object in the input list and populate the rows of the DataTable
            foreach (var serviceLine in serviceLines)
            {
                DataRow row = dataTable.NewRow();
                row["ServiceLineCode"] = serviceLine.ServiceLineCode;
                row["ParentServiceLineCodes"] = serviceLine.ServiceLineHierarchyCode;

                // Add the row to the DataTable
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }



        public DataTable ConvertRevOfficeListToDataTable(IEnumerable<RevOffice> revOffices)
        {
            // Create a new DataTable instance
            DataTable dataTable = new DataTable();

            // Define columns in the DataTable that correspond to the properties of the RevOffice class
            dataTable.Columns.Add("OfficeLevel", typeof(int));
            dataTable.Columns.Add("OfficeCode", typeof(int));
            dataTable.Columns.Add("OfficeName", typeof(string));
            dataTable.Columns.Add("ParentOfficeCode", typeof(int));
            dataTable.Columns.Add("CurrencyCode", typeof(string));
            dataTable.Columns.Add("OfficeHead", typeof(string));
            dataTable.Columns.Add("EntityTypeCode", typeof(string));
            dataTable.Columns.Add("OfficeAbbreviation", typeof(string));
            dataTable.Columns.Add("OfficeStatus", typeof(string));

            // Loop through each RevOffice object in the input list and populate the rows of the DataTable
            foreach (var office in revOffices)
            {
                DataRow row = dataTable.NewRow();
                row["OfficeLevel"] = office.OfficeLevel;
                row["OfficeCode"] = office.OfficeCode;
                row["OfficeName"] = office.OfficeName;
                row["ParentOfficeCode"] = office.ParentOfficeCode;
                row["CurrencyCode"] = office.CurrencyCode;
                row["OfficeHead"] = office.OfficeHead;
                row["EntityTypeCode"] = office.EntityTypeCode;
                row["OfficeAbbreviation"] = office.OfficeAbbreviation;
                row["OfficeStatus"] = office.OfficeStatus;

                // Add the row to the DataTable
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public DataTable ConvertServiceLineListToDataTable(IEnumerable<ServiceLineHierarchy> serviceLineList)
        {
            // Create a new DataTable
            DataTable dataTable = new DataTable();

            // Define the columns in the DataTable based on the ServiceLineHierarchy properties
            dataTable.Columns.Add("ServiceLineId", typeof(Guid));
            dataTable.Columns.Add("ServiceLineHierarchyCode", typeof(string));
            dataTable.Columns.Add("ServiceLineHierarchyName", typeof(string));
            dataTable.Columns.Add("ServiceLineCode", typeof(string));
            dataTable.Columns.Add("ServiceLineName", typeof(string));
            dataTable.Columns.Add("EmployeeCount", typeof(int));
            dataTable.Columns.Add("InActive", typeof(bool));

            // Iterate over each ServiceLineHierarchy object and add the corresponding row to the DataTable
            foreach (var serviceLine in serviceLineList)
            {
                DataRow row = dataTable.NewRow();
                row["ServiceLineId"] = serviceLine.ServiceLineId;
                row["ServiceLineHierarchyCode"] = serviceLine.ServiceLineHierarchyCode;
                row["ServiceLineHierarchyName"] = serviceLine.ServiceLineHierarchyName;
                row["ServiceLineCode"] = serviceLine.ServiceLineCode;
                row["ServiceLineName"] = serviceLine.ServiceLineName;
                row["EmployeeCount"] = serviceLine.EmployeeCount;
                row["InActive"] = serviceLine.InActive;

                // Add the row to the DataTable
                dataTable.Rows.Add(row);
            }

            // Return the populated DataTable
            return dataTable;
        }


    }
}
