using Microservices.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Staffing.Authentication.Contracts.RepositoryInterfaces;
using Staffing.Authentication.Contracts.Services;
using Staffing.Authentication.Core.Enums;
using Staffing.Authentication.Core.Helpers;
using Staffing.Authentication.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Authentication.Core.Services
{
    public class SecurityUserService : ISecurityUserService
    {
        private readonly IResourcesApiClient _resourcesApiClient;
        private readonly IHcpdApiClient _hcpdApiClient;
        private readonly IPegC2CApiClient _pegC2CApiClient;
        private readonly ISecurityUserRepository _securityUserRepository;
        private readonly AppSettingsConfiguration _appSettings;
        
        public SecurityUserService(ISecurityUserRepository securityUserRepository,
            IResourcesApiClient resourcesApiClient, IOptionsSnapshot<AppSettingsConfiguration> appSettings,
            IHcpdApiClient hcpdApiClient,
            IPegC2CApiClient pegC2CApiClient)
        {
            _securityUserRepository = securityUserRepository;
            _resourcesApiClient = resourcesApiClient;
            _appSettings = appSettings.Value;
            _hcpdApiClient = hcpdApiClient;
            _pegC2CApiClient = pegC2CApiClient;
        }

        public async Task<(SecurityUserViewModel securityUser, string token)> AuthenticateEmployee(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode)) throw new ArgumentException("User Code cannot be null or empty.");

            
            SecurityUserViewModel securityUserVM = null;
            string token = null;

            var securityUserTask = _securityUserRepository.Authenticate(employeeCode);
            var hcpdSecurityUserTask = _hcpdApiClient.GetSecurityUserDetails(employeeCode);
            var pegC2CSecurityUserTask = _pegC2CApiClient.GetSecurityUserAccess(employeeCode);

            await Task.WhenAll(securityUserTask, hcpdSecurityUserTask, pegC2CSecurityUserTask);

            var securityUser = securityUserTask.Result;
            var hcpdSecurityUsers = hcpdSecurityUserTask.Result;
            bool isUserHasAccessToPegC2C = pegC2CSecurityUserTask.Result;

            var hcpdSecurityUser = ConvertToViewModel(hcpdSecurityUsers);
            

            if (securityUser.Any())
            {
                
                securityUserVM = ConvertToViewModel(securityUser);
                var claimsData = GetClaims(securityUserVM, hcpdSecurityUser, isUserHasAccessToPegC2C);
                token = GenerateToken(claimsData);
            }

            else

            {
                List<Claim> claimsData = new List<Claim>();
                var claimsDataForNonBossUser = AddCaseIntakeClaimsForUsers(employeeCode, claimsData);
                
                token = GenerateToken(claimsDataForNonBossUser);

            }

            return (securityUserVM, token);


        }

        private HcpdSecurityUserViewModel ConvertToViewModel(IEnumerable<HcpdSecurityUser> hcpdSecurityUsers)
        {
            if (!hcpdSecurityUsers.Any())
            {
                return new HcpdSecurityUserViewModel();
            }

            var securityUser = new HcpdSecurityUserViewModel
            {
                EmployeeCode = hcpdSecurityUsers.First()?.EmployeeCode,
                SecurityAccessList = new List<HcpdSecurityAccess>()
            };

            // Agregate all Office & PD Grade mappings
            var securityAccessList = new List<HcpdSecurityAccess>();
            foreach (var item in hcpdSecurityUsers)
            {
                securityAccessList.AddRange(item.SecurityAccessList);
            }

            // Combine access of same office for different roles
            securityUser.SecurityAccessList = securityAccessList?.GroupBy(g => g.Office).Select(grp => new HcpdSecurityAccess
            {
                Office = grp.Key,
                PDGradeAccess = grp.SelectMany(x => x.PDGradeAccess).Distinct().ToArray()
            }).ToList();

            return securityUser;

        }

        public string AuthenticateApp(string appName, string appSecret)
        {
            var appSecretDecrypted = appSecret.Decrypt();
            if (_appSettings.AppToken[appName] != appSecretDecrypted) return null;
            var claimsData = GetClaims(appName, _appSettings.ClaimRoles[appName]?.Split(","));
            var token = GenerateToken(claimsData);
            return $"Bearer {token}";
        }

        public async Task<Employee> GetEmployeeByEmployeeCode(string employeeCode)
        {
            var employeeTask = _resourcesApiClient.GetEmployee(employeeCode);
            var tokenTask = AuthenticateEmployee(employeeCode);

            await Task.WhenAll(employeeTask, tokenTask);

            var employee = employeeTask.Result;
            var securityUserWithToken = tokenTask.Result;

            if (securityUserWithToken.token != null)
            {
                if (securityUserWithToken.securityUser != null)
                {
                    employee.IsAdmin = securityUserWithToken.securityUser.IsAdmin;
                    employee.Override = securityUserWithToken.securityUser.Override;
                    employee.HasAccessToAISearch = securityUserWithToken.securityUser.HasAccessToAISearch;
                    employee.HasAccessToStaffingInsightsTool = securityUserWithToken.securityUser.HasAccessToStaffingInsightsTool;
                    employee.HasAccessToRetiredStaffingTab = securityUserWithToken.securityUser.HasAccessToRetiredStaffingTab;

                }
                employee.Token = securityUserWithToken.token;
            }

            return employee;
        }
        public async Task<object> RefreshToken(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var appName = principal.Identity.Name;
            var savedRefreshToken = GetRefreshToken(appName); //retrieve the refresh token from a data store
            if (savedRefreshToken != refreshToken)
                throw new SecurityTokenException("Invalid refresh token");

            var newJwtToken = GenerateToken(principal.Claims);
            var newRefreshToken = GenerateRefreshToken();

            ConfigurationUtility.AddOrUpdateAppSetting($"AppRefreshToken:{appName}", newRefreshToken);

            return new
            {
                token = newJwtToken,
                refreshToken = newRefreshToken
            };
        }
        public async Task<object> RegisterApp(string employeeCode, string appName, AppClaim claim)
        {
            if (!_appSettings.AuthorizedToRegisterApp.Contains(employeeCode))
                return "Not authorized to register app";
            var clientId = String.Empty;
            var registeredClaims = new List<string>();
            if (_appSettings.AppClientId.ContainsKey(appName))
            {
                clientId = _appSettings.AppClientId[appName];
                registeredClaims = _appSettings.ClaimRoles[clientId].Split(',').ToList();
                if (registeredClaims.Contains(claim.ToString())) return null;
                registeredClaims.Add(claim.ToString());
            }

            clientId = string.IsNullOrEmpty(clientId) ? Guid.NewGuid().ToString() : clientId;
            var clientSecret = appName + '-' + clientId;
            var claims = !registeredClaims.Any() ? claim.ToString() : string.Join(",", registeredClaims);


            ConfigurationUtility.AddOrUpdateAppSetting($"AppClientId:{appName}", clientId);
            ConfigurationUtility.AddOrUpdateAppSetting($"AppToken:{clientId}", clientSecret);

            var refreshToken = GenerateRefreshToken();
            ConfigurationUtility.AddOrUpdateAppSetting($"AppRefreshToken:{clientId}", refreshToken);

            ConfigurationUtility.AddOrUpdateAppSetting($"ClaimRoles:{clientId}", claims);

            return new
            {
                clientId,
                clientSecret = clientSecret.Encrypt()
            };
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        #region Private Methods
        private SecurityUserViewModel ConvertToViewModel(IEnumerable<SecurityUser> securityUserWithFeatureAccess)
        {
            var securityUser = new SecurityUserViewModel
            {
                EmployeeCode = securityUserWithFeatureAccess.FirstOrDefault().EmployeeCode,
                IsAdmin = securityUserWithFeatureAccess.FirstOrDefault().IsAdmin,
                Override = securityUserWithFeatureAccess.FirstOrDefault().Override,
                RoleNames = securityUserWithFeatureAccess.FirstOrDefault()?.Roles?.Split(","),
                Features = securityUserWithFeatureAccess.Select(x => new FeatureAccess
                {
                    FeatureName = x.FeatureName,
                    AccessTypeName = x.AccessTypeName
                }).ToList().GroupBy(g => new { g.FeatureName, g.AccessTypeName }).Select(grp => grp.First()).ToList(),
                OfficeCodes = securityUserWithFeatureAccess.FirstOrDefault()?.OfficeCodes?.Split(",").Select(Int32.Parse).ToArray(),
                DemandTypes = securityUserWithFeatureAccess.FirstOrDefault()?.DemandTypes?.Split(","),
                HasAccessToAISearch = securityUserWithFeatureAccess.FirstOrDefault().HasAccessToAISearch,
                HasAccessToStaffingInsightsTool = securityUserWithFeatureAccess.FirstOrDefault().HasAccessToStaffingInsightsTool,
                HasAccessToRetiredStaffingTab = securityUserWithFeatureAccess.First().HasAccessToRetiredStaffingTab,
            };

            return securityUser;
        }
        private static Claim[] GetClaims(SecurityUserViewModel securityUser, HcpdSecurityUserViewModel hcpdSecurityUser, bool isUserHasAccessToPegC2C)
        {
            //TODO: Decide if security data should be available in token or on model
            var scopeForStaffingUsers = ConfigurationUtility.GetValue("Scope:StaffingUser");
            var claimsList =  new[]
            {
                new Claim("EmployeeCode", securityUser.EmployeeCode),
                new Claim("Roles", JsonConvert.SerializeObject(securityUser.RoleNames)),
                new Claim("HCPDAccess", JsonConvert.SerializeObject(hcpdSecurityUser)),
                new Claim("OfficeAccess", JsonConvert.SerializeObject(securityUser.OfficeCodes)),
                new Claim("Scope", scopeForStaffingUsers),
                new Claim("FeatureAccess", JsonConvert.SerializeObject(securityUser.Features)),
                new Claim("DemandTypesAccess", JsonConvert.SerializeObject(securityUser.DemandTypes)),
                new Claim("PegC2CAccess", JsonConvert.SerializeObject(isUserHasAccessToPegC2C))
             };

            if (securityUser.RoleNames == null || securityUser.RoleNames.Length == 0)
            {
                var data = AddCaseIntakeClaimsForUsers(securityUser.EmployeeCode, claimsList.ToList());
                claimsList = data.ToArray();
            }
            return claimsList;
        }

        private static List<Claim> AddCaseIntakeClaimsForUsers(string employeeCode, List<Claim> claimsData)
        {
            // Create the new feature access to be appended
            var features = new List<FeatureAccess>
            {
                new FeatureAccess
                {
                    FeatureName = "intakeForm",
                    AccessTypeName = "Write"
                }
            };

            // Check if the FeatureAccess claim already exists
            var existingFeatureAccessClaim = claimsData.FirstOrDefault(c => c.Type == "FeatureAccess");

            if (existingFeatureAccessClaim != null)
            {

                // If FeatureAccess exists, append the new feature data to the existing one
                var existingAccessibleFeatures = JsonConvert.DeserializeObject<List<FeatureAccess>>(existingFeatureAccessClaim.Value);
                existingAccessibleFeatures.AddRange(features); // Append the new features

                // Remove the old claim and add the updated one
                claimsData.Remove(existingFeatureAccessClaim);  // Remove the old claim
                claimsData.Add(new Claim("FeatureAccess", JsonConvert.SerializeObject(existingAccessibleFeatures))); // Add updated claim
            }
            else
            {
                var scopeForNonStaffingUsers = ConfigurationUtility.GetValue("Scope:NonStaffingUser");
                // If FeatureAccess does not exist, add the claim
                claimsData.AddRange(new[]
                {
                    new Claim("EmployeeCode", employeeCode),
                    new Claim("Scope", scopeForNonStaffingUsers),
                    new Claim("FeatureAccess", JsonConvert.SerializeObject(features)),
             });
            }

            return claimsData;

        }


        private static Claim[] GetClaims(string appName, string[] roles)
        {
            var claims = roles?.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
            var claimsData = new List<Claim>
            {
                new Claim(ClaimTypes.Name, appName)
            };
            if (claims != null)
                claimsData.AddRange(claims);
            return claimsData.ToArray();
        }

        private string GenerateToken(IEnumerable<Claim> claimsData)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(
                Environment.GetEnvironmentVariable("staffing_secretKey"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimsData),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddYears(10),
                Issuer = "Staffing Authentication API",
                Audience = "APIs accessed by Staffing App",
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes(
                Environment.GetEnvironmentVariable("staffing_secretKey"));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        private string GetRefreshToken(string appName)
        {
            return _appSettings.AppRefreshToken[appName];
        }
        #endregion
    }
}