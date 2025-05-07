using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using Microservices.Common.Core.Helpers;
using Microsoft.Extensions.Options;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static BackgroundPolling.API.Core.Helpers.Constants;

namespace BackgroundPolling.API.Core.Services
{
    public class ADSecurityUserService : IADSecurityUserService
    {
        private readonly AppSettingsConfiguration _appSettings;
        private readonly AzureADHelper _azureADHelper;

        public ADSecurityUserService(IOptionsSnapshot<AppSettingsConfiguration> appSettings)
        {
            _appSettings = appSettings.Value;

            var _clientId = _appSettings.AzureADCredentials["ClientId"];
            var _tenantId = _appSettings.AzureADCredentials["TenantId"];
            var _clientSecret = _appSettings.AzureADCredentials["ClientSecret"];

            _azureADHelper = new AzureADHelper(_clientId, _tenantId, _clientSecret);
        }

        public async Task<SecurityUserModel> GetBOSSSecurityUsersFromAD(IEnumerable<SecurityGroup> securityGroups, IEnumerable<Office> offices)
        {
            var securityUsers = new List<SecurityUser>();
            var securityUsersWithFeatureAccess = new List<SecurityUser>();
            var polarisSecurityUsersWithGeography = new List<PolarisSecurityUser>();

            IEnumerable<string> groupNames = securityGroups.Select(x => x.GroupName).Distinct();

            var groupNamesWithIds = await _azureADHelper.GetGroupByName(groupNames);
            foreach (var item in groupNamesWithIds)
            {
                var grp = securityGroups.FirstOrDefault(x => string.Equals(x.GroupName, item.Key, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(item.Key) && grp != null)
                {
                    var members = await GetADMembers(item.Value);

                    foreach (var member in members)
                    {
                        if (grp.RoleCodes != null && grp.RoleCodes != "") { 
                            foreach (var y in grp.RoleCodes.Split(","))
                            {
                                var roleCode = Convert.ToInt16(y);
                                securityUsers.Add(new SecurityUser
                                {
                                    EmployeeCode = member,
                                    RoleCode = roleCode,
                                    LastUpdatedBy = "Auto-AD",
                                    IsBossSystemUser = grp.IsBossSystemUser
                                });
                            }
                        }
                        if(grp.FeatureCodes != null && grp.FeatureCodes != "")
                        {
                            foreach (var z in grp.FeatureCodes.Split(","))
                            {
                                var featureCode = Convert.ToInt16(z);
                                securityUsersWithFeatureAccess.Add(new SecurityUser
                                {
                                    EmployeeCode = member,
                                    FeatureCode = featureCode,
                                    LastUpdatedBy = "Auto-AD",
                                    IsBossSystemUser = grp.IsBossSystemUser
                                });
                            }
                        }

                        if (grp.OfficeRegionCodes != null && grp.OfficeRegionCodes != "")
                        {
                            foreach (var z in grp.OfficeRegionCodes.Split(","))
                            {
                                // Convert the region code from string to integer
                                var regionCode = Convert.ToInt16(z);

                                // Filter offices based on the region code
                                var officesWithinTheRegion = offices.Where(o => o.OfficeRegionCode == regionCode);

                                foreach (var office in officesWithinTheRegion)
                                {
                                    polarisSecurityUsersWithGeography.Add(new PolarisSecurityUser
                                    {
                                        EmployeeCode = member,
                                        OfficeCode = office.OfficeCode,  
                                        Source = Source.PracticeStaffing,
                                        IsBossSystemUser = grp.IsBossSystemUser,
                                        LastUpdated = DateTime.Now.Date,
                                    });
                                }
                            }
                        }

                    }

                }
            }

            securityUsers = securityUsers.GroupBy(g => new { g.EmployeeCode, g.RoleCode }).Select(grp => new SecurityUser
            {
                EmployeeCode = grp.Key.EmployeeCode,
                RoleCode = grp.Key.RoleCode,
                LastUpdatedBy = "Auto-AD",
                IsBossSystemUser = grp.First().IsBossSystemUser
            }).ToList();

            securityUsersWithFeatureAccess = securityUsersWithFeatureAccess.GroupBy(g => new { g.EmployeeCode, g.FeatureCode }).Select(grp => new SecurityUser
            {
                EmployeeCode = grp.Key.EmployeeCode,
                FeatureCode = grp.Key.FeatureCode,
                LastUpdatedBy = "Auto-AD",
                IsBossSystemUser = grp.First().IsBossSystemUser
            }).ToList();

            polarisSecurityUsersWithGeography = polarisSecurityUsersWithGeography.GroupBy(g => new { g.EmployeeCode, g.OfficeCode }).Select(grp => new PolarisSecurityUser
            {
                EmployeeCode = grp.Key.EmployeeCode,
                OfficeCode = grp.Key.OfficeCode,
                Source = grp.First().Source,
                IsBossSystemUser = grp.First().IsBossSystemUser,
                LastUpdated = grp.First().LastUpdated,
            }).ToList();

            var users = new SecurityUserModel
            {
                SecurityUsers = securityUsers,
                SecurityUsersWithFeatureAccess = securityUsersWithFeatureAccess,
                PolarisSecurityUsersWithGeography = polarisSecurityUsersWithGeography
            };

            return users;
        }

        private async Task<IEnumerable<SecurityUser>> GetADMembers(IDictionary<string, string> item, List<SecurityUser> securityUsers)
        {
            var accountName = item["AD"];
            var roleCode = Convert.ToInt32(item["Role"]);
            var isBossSystemUser = Convert.ToBoolean(item["IsBossSystemUser"]);
            //var members = await ActiveDirectory.RetrieveGroupMembers(accountName);
            var members = await _azureADHelper.GetGroupMembersEmployeeCodes(item["GroupId"]);

            securityUsers.AddRange(members.Select(employeeCode => new SecurityUser
            {
                EmployeeCode = employeeCode,
                RoleCode = roleCode,
                IsBossSystemUser = isBossSystemUser
            }));

            return securityUsers;
        }

        private async Task<IEnumerable<string>> GetADMembers(string groupId)
        {
            var members = await _azureADHelper.GetGroupMembersEmployeeCodes(groupId);

            return members;
        }
    }
}
