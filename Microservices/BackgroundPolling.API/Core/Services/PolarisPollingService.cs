using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using static BackgroundPolling.API.Core.Helpers.Constants;

namespace BackgroundPolling.API.Core.Services
{
    public class PolarisPollingService : IPolarisPollingService
    {
        public IPolarisPollingRepository _polarisPollingRepository;
        public IPolarisApiClient _polarisApiClient;
        public IResourceApiClient _resourceApiClient;
        public IStaffingApiClient _staffingApiClient;

        public ICcmApiClient _ccmApiClient;
        public IADSecurityUserService _aDSecurityUserService;

        public PolarisPollingService(IPolarisPollingRepository polarisPollingRepository, IPolarisApiClient polarisApiClient,
            IResourceApiClient resourceApiClient, IStaffingApiClient staffingApiClient, ICcmApiClient ccmApiClient, IADSecurityUserService aDSecurityUserService)
        {
            _polarisPollingRepository = polarisPollingRepository;
            _polarisApiClient = polarisApiClient;
            _resourceApiClient = resourceApiClient;
            _staffingApiClient = staffingApiClient;

            _ccmApiClient = ccmApiClient;
            _aDSecurityUserService = aDSecurityUserService;
        }

        public async Task UpsertSecurityUsers()
        {
            var EMEARegionCode = Convert.ToInt32(ConfigurationUtility.GetValue("RegionCodes:EMEARegionCode"));

            var revSecurityUsersTask = _polarisApiClient.GetRevSecurityUsersWithGeography();
            var activeEmployeesTask = _resourceApiClient.GetEmployees();
            var bossSecurityUsersTask = _staffingApiClient.GetBossSecurityUsers();
            var officesTask = _ccmApiClient.GetOfficeList();

            await Task.WhenAll(revSecurityUsersTask, activeEmployeesTask, bossSecurityUsersTask, officesTask);

            var revSecurityUsers = GetAuthorizedRevSecurityUsers(revSecurityUsersTask.Result);

            var activeEmployees = activeEmployeesTask.Result;
            var bossSecurityUsers = bossSecurityUsersTask.Result;
            var offices = officesTask.Result;
            var securityUsersFromAD = await GetSecurityUsersFromAD(offices);
            var hrSecurityUsers = GetHRUsers(activeEmployees, securityUsersFromAD.SecurityUsers);

            if (activeEmployees == null || !activeEmployees.Any())
            {
                return;
            }

            if (revSecurityUsers == null || !revSecurityUsers.Any())
            {
                return;
            }

            var allSecurityUsersWithOfficeList = GetSecurityUsersForAnalyticsDB(revSecurityUsers, hrSecurityUsers, bossSecurityUsers, offices);

            if (allSecurityUsersWithOfficeList.Count > 0)
            {
                await UpsertSecurityUsersForAnalytics(allSecurityUsersWithOfficeList);

            }            

            var securityUsersForBoss = GetSecurityUsersForBoss(revSecurityUsers, securityUsersFromAD.SecurityUsers, hrSecurityUsers);

            if (securityUsersForBoss.Count() > 0)
            {
                await UpsertSecurityUsersForBOSS(securityUsersForBoss, securityUsersFromAD.SecurityUsersWithFeatureAccess, securityUsersFromAD.PolarisSecurityUsersWithGeography);

            }
            
        }

        public async Task<SecurityUserModel> GetSecurityUsersFromAD(IEnumerable<Office> offices)
        {
            var securityGroups = await _staffingApiClient.GetAllBOSSSecurityGroups();
            return await _aDSecurityUserService.GetBOSSSecurityUsersFromAD(securityGroups, offices);
        }

        private IEnumerable<SecurityUser> GetSecurityUsersForBoss(IEnumerable<PolarisSecurityUser> revSecurityUsers, IEnumerable<SecurityUser> securityUsersFromAD, IEnumerable<SecurityUser> hrSecurityUsers)
        {
            var revUsersForBOSS = GetUniqueRevSecurityUsers(revSecurityUsers);
            var allSecurityUsers = revUsersForBOSS.Concat(hrSecurityUsers).Concat(securityUsersFromAD.Where(x => x.RoleCode != Convert.ToInt32(RoleCodes.Hr)));
            var uniqueSecurityUsers = allSecurityUsers.GroupBy(x => new { x.EmployeeCode, x.RoleCode }).Select(y => y.FirstOrDefault());
            return uniqueSecurityUsers;
        }

        private List<PolarisSecurityUser> GetSecurityUsersForAnalyticsDB(IEnumerable<PolarisSecurityUser> revSecurityUsers, IEnumerable<SecurityUser> hrSecurityUsers, IEnumerable<SecurityUserViewModel> bossSecurityUsers, IEnumerable<Office> offices)
        {
            var hrSecurityUserListwithOfficeCodes = GetUniqueSecurityUsersWithOfficeCodes(hrSecurityUsers, offices, Source.HR);

            var bossSecurityUserListwithOfficeCodes = GetUniqueSecurityUsersWithOfficeCodes(bossSecurityUsers, offices, Source.Boss);

            var allSecurityUsersWithOfficeList = revSecurityUsers.Concat(bossSecurityUserListwithOfficeCodes).Concat(hrSecurityUserListwithOfficeCodes).ToList();
            return allSecurityUsersWithOfficeList;
        }

        private IEnumerable<PolarisSecurityUser> GetAuthorizedRevSecurityUsers(IList<PolarisSecurityUser> revSecurityUsers) 
        {
            var authorizedRevUsers= revSecurityUsers.Where(x => x.CapacityBreakdownFlag && x.PriceRealizationFlag);
            authorizedRevUsers = authorizedRevUsers.GroupBy(x => new { x.EmployeeCode, x.OfficeCode, x.Source }).Select(x => x.ToList().First());
            return authorizedRevUsers;

        }

        public async Task UpsertSecurityUsersForBOSS(IEnumerable<SecurityUser> securityUsersForBoss, IEnumerable<SecurityUser> securityUsersWithFeatureAccess, IEnumerable<PolarisSecurityUser> SecurityUsersWithGeographyForBoss)
        {            
            var securityUsersDataTable = ConvertToSecurityUserDataTable(securityUsersForBoss);
            var securityUsersWithFeatureAccessDataTable = ConvertToSecurityUserWithFeatureAccessDataTable(securityUsersWithFeatureAccess);
          
            var securityUserGeographyDataTable = ConvertToSecurityUserGeographyDataTable(SecurityUsersWithGeographyForBoss);

            await _polarisPollingRepository.UpsertSecurityUsersDataForBOSS(securityUsersDataTable, securityUsersWithFeatureAccessDataTable, securityUserGeographyDataTable);
        }

        public async Task UpsertSecurityUsersForAnalytics(IList<PolarisSecurityUser> securityUsersWithGeographyData)
        {
            var geoSecurityUserDataTable = ConvertToSecurityUserDataTable(securityUsersWithGeographyData);
            await _polarisPollingRepository.UpsertSecurityUsersForAnalytics(geoSecurityUserDataTable);
        }

        private IList<int> GetAccessibleOfficesAndHierarchyTopDown(IEnumerable<RevOffice> offices, List<int> accessibleOfficesAndHierarchy)
        {
            var accessibleOffices = new List<int>();
            if (accessibleOffices.Count() == 0)
                accessibleOffices.Add(7); //EMEA region code

            if (!accessibleOfficesAndHierarchy.Any())
            {
                accessibleOfficesAndHierarchy = accessibleOffices;
                var childOffices = offices
                    .Where(x => accessibleOffices.Contains(x.ParentOfficeCode))
                    .Select(x => x.OfficeCode).ToList().Distinct();
                accessibleOfficesAndHierarchy = accessibleOfficesAndHierarchy.Concat(childOffices).ToList();
                return GetAccessibleOfficesAndHierarchyTopDown(offices, accessibleOfficesAndHierarchy);
            }
            var accessibleChildOffices = offices
                .Where(x => accessibleOfficesAndHierarchy.Contains(x.ParentOfficeCode))
                .Select(x => x.OfficeCode).ToList().Distinct();
            var newAccessibleChildOffices = accessibleChildOffices.Except(accessibleOfficesAndHierarchy).ToList();
            if (newAccessibleChildOffices.Any())
            {
                accessibleOfficesAndHierarchy = accessibleOfficesAndHierarchy.Concat(newAccessibleChildOffices).ToList();
                return GetAccessibleOfficesAndHierarchyTopDown(offices, accessibleOfficesAndHierarchy);

            }
            return accessibleOfficesAndHierarchy.Distinct().ToList();
        }
        private DataTable ConvertToSecurityUserDataTable(IList<PolarisSecurityUser> securityUsers)
        {
            var securityUserDataTable = new DataTable();
            securityUserDataTable.Columns.Add("employeeCode", typeof(string));
            securityUserDataTable.Columns.Add("officeCode", typeof(short));
            securityUserDataTable.Columns.Add("source", typeof(string));
            securityUserDataTable.Columns.Add("roleType", typeof(string));
            securityUserDataTable.Columns.Add("isBossSystemUser", typeof(bool));
            securityUserDataTable.Columns.Add("capacityBreakdownFlag", typeof(bool));
            securityUserDataTable.Columns.Add("priceRealizationFlag", typeof(bool));
            securityUserDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            securityUserDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var securityUser in securityUsers)
            {
                var row = securityUserDataTable.NewRow();

                row["employeeCode"] = securityUser.EmployeeCode;
                row["officeCode"] = securityUser.OfficeCode;
                row["source"] = (object)securityUser.Source ?? DBNull.Value;
                row["roleType"] = (object)securityUser.RoleType ?? DBNull.Value;
                row["isBossSystemUser"] = (object)securityUser.IsBossSystemUser ?? DBNull.Value;
                row["capacityBreakdownFlag"] = (object)securityUser.CapacityBreakdownFlag ?? DBNull.Value;
                row["priceRealizationFlag"] = (object)securityUser.PriceRealizationFlag ?? DBNull.Value;
                row["lastUpdated"] = (object)securityUser.LastUpdated ?? DateTime.Today.Date;
                row["lastUpdatedBy"] = "Auto-SecurityPolling";

                securityUserDataTable.Rows.Add(row);
            }

            return securityUserDataTable;
        }
        private DataTable ConvertToSecurityUserDataTable(IEnumerable<SecurityUser> securityUsers)
        {
            var securityUserDataTable = new DataTable();
            securityUserDataTable.Columns.Add("employeeCode", typeof(string));
            securityUserDataTable.Columns.Add("isBossSystemUser", typeof(bool));
            securityUserDataTable.Columns.Add("roleCode", typeof(int));
            securityUserDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            securityUserDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var securityUser in securityUsers)
            {
                var row = securityUserDataTable.NewRow();

                row["employeeCode"] = securityUser.EmployeeCode;
                row["isBossSystemUser"] = securityUser.IsBossSystemUser;
                row["roleCode"] = (object)securityUser.RoleCode ?? DBNull.Value;
                row["lastUpdated"] = DateTime.Today.Date;
                row["lastUpdatedBy"] = "Auto-Security";

                securityUserDataTable.Rows.Add(row);
            }

            return securityUserDataTable;
        }

        private DataTable ConvertToSecurityUserWithFeatureAccessDataTable(IEnumerable<SecurityUser> securityUsersWithFeatureAccess)
        {
            var securityUserWithFeatureAccessDataTable = new DataTable();
            securityUserWithFeatureAccessDataTable.Columns.Add("employeeCode", typeof(string));
            securityUserWithFeatureAccessDataTable.Columns.Add("isBossSystemUser", typeof(bool));
            securityUserWithFeatureAccessDataTable.Columns.Add("featureCode", typeof(int));
            securityUserWithFeatureAccessDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            securityUserWithFeatureAccessDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var securityUser in securityUsersWithFeatureAccess)
            {
                var row = securityUserWithFeatureAccessDataTable.NewRow();

                row["employeeCode"] = securityUser.EmployeeCode;
                row["isBossSystemUser"] = securityUser.IsBossSystemUser;
                row["featureCode"] = (object)securityUser.FeatureCode ?? DBNull.Value;
                row["lastUpdated"] = DateTime.Today.Date;
                row["lastUpdatedBy"] = "Auto-Security";

                securityUserWithFeatureAccessDataTable.Rows.Add(row);
            }

            return securityUserWithFeatureAccessDataTable;
        }

        private DataTable ConvertToSecurityUserGeographyDataTable(IEnumerable<PolarisSecurityUser> securityUsers)
        {
            var securityUserDataTable = new DataTable();
            securityUserDataTable.Columns.Add("employeeCode", typeof(string));
            securityUserDataTable.Columns.Add("officeCode", typeof(short));
            securityUserDataTable.Columns.Add("source", typeof(string));
            securityUserDataTable.Columns.Add("roleType", typeof(string));
            securityUserDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            securityUserDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var securityUser in securityUsers)
            {

                var row = securityUserDataTable.NewRow();

                row["employeeCode"] = securityUser.EmployeeCode;
                row["officeCode"] = securityUser.OfficeCode;
                row["source"] = (object)securityUser.Source ?? DBNull.Value;
                row["roleType"] = (object)securityUser.RoleType ?? DBNull.Value;
                row["lastUpdated"] = (object)securityUser.LastUpdated ?? DateTime.Today.Date;
                row["lastUpdatedBy"] = "Auto-Polaris";

                securityUserDataTable.Rows.Add(row);
            }

            return securityUserDataTable;
        }
        
        private IEnumerable<SecurityUser> GetUniqueRevSecurityUsers(IEnumerable<PolarisSecurityUser> revSecurityUsers) 
        {
            var uniqueRevSecurityUsers = revSecurityUsers.GroupBy(g => g.EmployeeCode).Select(grp => grp.FirstOrDefault()).Select(x => new SecurityUser()
            {
                EmployeeCode = x.EmployeeCode,
                IsBossSystemUser = false,
                RoleCode = (int)RoleCodes.Rev
            }).ToList();

            return uniqueRevSecurityUsers;
        }

        private IEnumerable<SecurityUser> GetHRUsers(List<Resource> activeEmployees, IEnumerable<SecurityUser> securityUsersFromAD)
        {
            List<SecurityUser> hrEmployees = GetHRUsersByDepartmentCodes(activeEmployees);
            List<SecurityUser> hrExceptionsFromAd = GetHRExceptionsFromAD(securityUsersFromAD);

            var uniqueHrUsers = hrExceptionsFromAd.Concat(hrEmployees).GroupBy(x => x.EmployeeCode).Select(x => x.First());

            return uniqueHrUsers;

        }

        private static List<SecurityUser> GetHRExceptionsFromAD(IEnumerable<SecurityUser> securityUsersFromAD)
        {
            return securityUsersFromAD.Where(x => x.RoleCode == Convert.ToInt32(RoleCodes.Hr)).Select(x => new SecurityUser()
            {
                EmployeeCode = x.EmployeeCode,
                IsBossSystemUser = false,
                RoleCode = (int)RoleCodes.Hr,
            }).ToList();
        }

        private static List<SecurityUser> GetHRUsersByDepartmentCodes(List<Resource> activeEmployees)
        {
            var hrDepartmentcodes = ConfigurationUtility.GetValue("HrDepartments:DepartmentCodes");
            var HrUsersFromWorkday = activeEmployees.Where(fl => fl.Department?.DepartmentCode != null && hrDepartmentcodes.Contains(fl.Department.DepartmentCode));
            var hrEmployees = HrUsersFromWorkday.Select(x => new SecurityUser()
            {
                EmployeeCode = x.EmployeeCode,
                IsBossSystemUser = false,
                RoleCode = (int)RoleCodes.Hr
            }).ToList();
            return hrEmployees;
        }

        private List<PolarisSecurityUser> GetUniqueSecurityUsersWithOfficeCodes(IEnumerable<SecurityUser> securityUsers, IEnumerable<Office> offices, string source)
        {
            return CreatePolarisSecurityUsers(securityUsers, offices, source);
        }

        private List<PolarisSecurityUser> GetUniqueSecurityUsersWithOfficeCodes(IEnumerable<SecurityUserViewModel> securityUsers, IEnumerable<Office> offices, string source)
        {
            return CreatePolarisSecurityUsers(securityUsers, offices, source);
        }


        private List<PolarisSecurityUser> CreatePolarisSecurityUsers(IEnumerable<dynamic> securityUsers, IEnumerable<Office> offices, string source)
        {
            List<PolarisSecurityUser> securityUsersWithOfficeCodes = new List<PolarisSecurityUser>();

            foreach (var securityUser in securityUsers.GroupBy(g => g.EmployeeCode).Select(grp => grp.FirstOrDefault()))
            {
                foreach (var office in offices)
                {
                    PolarisSecurityUser user = new PolarisSecurityUser
                    {
                        EmployeeCode = securityUser.EmployeeCode,
                        Source = source,
                        IsBossSystemUser = securityUser.IsBossSystemUser,
                        OfficeCode = office.OfficeCode,
                        LastUpdated = DateTime.Now.Date,
                    };
                    securityUsersWithOfficeCodes.Add(user);
                }
            }

            return securityUsersWithOfficeCodes;
        }



    }
}
