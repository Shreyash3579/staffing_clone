using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Staffing.Authentication.Contracts.RepositoryInterfaces;
using Staffing.Authentication.Contracts.Services;
using Staffing.Authentication.Core.Services;
using Staffing.Authentication.Models;
using Staffing.Authentication.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.Authentication.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.Authentication.SecurityUserServices")]
    public class SecurityUserServiceTests
    {
        [Theory]
        [InlineData("", "User Code cannot be null or empty.")]
        public async Task AuthenticateEmployee_should_return_ArgumentException(string employeeCode, string errorMessage)
        {
            //Arrange
            var mockSecurityUserRepo = new Mock<ISecurityUserRepository>();
            var mockResourcesApiClient = new Mock<IResourcesApiClient>();
            var mockADService = new Mock<IADservice>();
            var optionsAccessor = new Mock<IOptionsSnapshot<AppSettingsConfiguration>>();

            var sut = new SecurityUserService(mockSecurityUserRepo.Object, mockResourcesApiClient.Object,
                optionsAccessor.Object, mockADService.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.AuthenticateEmployee(employeeCode));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("39209", true)]
        [InlineData("42AKS", false)]
        public async Task AuthenticateEmployee_should_return_token(string employeeCode, bool isAdmin)
        {
            //Arrange
            var mockSecurityUserRepo = new Mock<ISecurityUserRepository>();
            var mockResourcesApiClient = new Mock<IResourcesApiClient>();
            var mockADService = new Mock<IADservice>();
            var optionsAccessor = new Mock<IOptionsSnapshot<AppSettingsConfiguration>>();

            mockSecurityUserRepo.Setup(x => x.Authenticate(employeeCode))
                .ReturnsAsync(GetAuthorizedUser(employeeCode, isAdmin));

            var expectedUserType = isAdmin ? "BossAdminUser" : "BossUser";

            var sut = new SecurityUserService(mockSecurityUserRepo.Object, mockResourcesApiClient.Object,
                optionsAccessor.Object, mockADService.Object);

            //Act
            var result = await sut.AuthenticateEmployee(employeeCode);
            var decodedToken = JwtTokenHelper.GetDecodedJwtToken(result.token);

            //Assert
            result.token.Should().NotBeNullOrEmpty();
            decodedToken.Claims.First(claim => claim.Type == "EmployeeCode").Value.Should().Be(employeeCode);
            Convert.ToBoolean(decodedToken.Claims.First(claim => claim.Type == "IsBossUser").Value).Should().Be(true);
            decodedToken.Claims.First(claim => claim.Type == "UserType").Value.Should().Be(expectedUserType);
        }

        [Theory]
        [InlineData("TEST1")]
        public async Task AuthenticateEmployee_should_NOT_return_token(string employeeCode)
        {
            //Arrange
            var mockSecurityUserRepo = new Mock<ISecurityUserRepository>();
            var mockResourcesApiClient = new Mock<IResourcesApiClient>();
            var mockADService = new Mock<IADservice>();
            var optionsAccessor = new Mock<IOptionsSnapshot<AppSettingsConfiguration>>();

            mockSecurityUserRepo.Setup(x => x.Authenticate(employeeCode))
                .ReturnsAsync(GetUnAuthorizedUser());

            var sut = new SecurityUserService(mockSecurityUserRepo.Object, mockResourcesApiClient.Object,
               optionsAccessor.Object, mockADService.Object);

            //Act
            var result = await sut.AuthenticateEmployee(employeeCode);

            //Assert
            result.token.Should().BeNullOrEmpty();
        }

        [Theory]
        [InlineData("52045", "Global Staffing Data Access", false, "HRUser")]
        [InlineData("37995", "Global Staffing Development Team", true, "SuperAdminUser")]
        public async Task AuthenticateEmployee_should_return_token_ForADMember(string employeeCode, string adAccount,
            bool isBossUser, string userType)
        {
            //Arrange
            var mockSecurityUserRepo = new Mock<ISecurityUserRepository>();
            var mockResourcesApiClient = new Mock<IResourcesApiClient>();
            var mockADService = new Mock<IADservice>();
            var optionsAccessor = new Mock<IOptionsSnapshot<AppSettingsConfiguration>>();

            mockADService.Setup(x => x.isGroupMember(adAccount, employeeCode))
                .ReturnsAsync(IsMemberOfADGroup(adAccount, userType));
            mockSecurityUserRepo.Setup(x => x.Authenticate(employeeCode))
               .ReturnsAsync(GetUnAuthorizedUser());

            var sut = new SecurityUserService(mockSecurityUserRepo.Object, mockResourcesApiClient.Object,
               optionsAccessor.Object, mockADService.Object);

            //Act
            var result = await sut.AuthenticateEmployee(employeeCode);
            var decodedToken = JwtTokenHelper.GetDecodedJwtToken(result.token);

            //Assert
            result.token.Should().NotBeNullOrEmpty();
            decodedToken.Claims.First(claim => claim.Type == "EmployeeCode").Value.Should().Be(employeeCode);
            Convert.ToBoolean(decodedToken.Claims.First(claim => claim.Type == "IsBossUser").Value).Should().Be(isBossUser);
            decodedToken.Claims.First(claim => claim.Type == "UserType").Value.Should().Be(userType);
        }

        [Theory]
        [InlineData("Staffing", "ImOivLnEiP/jaTt+LlMj5ef0KgogNLiSPqqHPOb5doM=")]
        public void AuthenticateApp_should_return_BearerToken(string appName, string appSecret)
        {
            //Arrange
            var mockSecurityUserRepo = new Mock<ISecurityUserRepository>();
            var mockResourcesApiClient = new Mock<IResourcesApiClient>();
            var mockADService = new Mock<IADservice>();
            var optionsAccessor = new Mock<IOptionsSnapshot<AppSettingsConfiguration>>();

            optionsAccessor.Setup(o => o.Value).Returns(new AppSettingsConfiguration
            {
                AppToken = new Dictionary<string, string> { { appName, "BackgroundPollingApi" } },
                ClaimRoles = new Dictionary<string, string> { { appName, "BackgroundPollingApi,EmployeeBasicInfoReadAccess" } }
            });

            var sut = new SecurityUserService(mockSecurityUserRepo.Object, mockResourcesApiClient.Object,
               optionsAccessor.Object, mockADService.Object);

            //Act
            var token = sut.AuthenticateApp(appName, appSecret);

            //Assert
            token.Should().NotBeNullOrEmpty();
            token.Should().StartWith("Bearer");
        }

        [Theory]
        [InlineData("Staffing", "ImOivLnEiP/jaTt+LlMj5ef0KgogNLiSPqqHPOb5doM=")]
        public void AuthenticateApp_should_NotReturn_BearerToken(string appName, string appSecret)
        {
            //Arrange
            var mockSecurityUserRepo = new Mock<ISecurityUserRepository>();
            var mockResourcesApiClient = new Mock<IResourcesApiClient>();
            var mockADService = new Mock<IADservice>();
            var optionsAccessor = new Mock<IOptionsSnapshot<AppSettingsConfiguration>>();

            optionsAccessor.Setup(o => o.Value).Returns(new AppSettingsConfiguration
            {
                AppToken = new Dictionary<string, string> { { appName, "" } }
            });

            var sut = new SecurityUserService(mockSecurityUserRepo.Object, mockResourcesApiClient.Object,
              optionsAccessor.Object, mockADService.Object);

            //Act
            var token = sut.AuthenticateApp(appName, appSecret);

            //Assert
            token.Should().BeNullOrEmpty();
        }

        [Theory]
        [InlineData("37995")]
        public async Task GetEmployeeByEmployeeCode_should_return_employee_withToken(string employeeCode)
        {
            //Arrange
            var mockSecurityUserRepo = new Mock<ISecurityUserRepository>();
            var mockResourcesApiClient = new Mock<IResourcesApiClient>();
            var mockADService = new Mock<IADservice>();
            var optionsAccessor = new Mock<IOptionsSnapshot<AppSettingsConfiguration>>();

            mockSecurityUserRepo.Setup(x => x.Authenticate(employeeCode))
                .ReturnsAsync(GetAuthorizedUser(employeeCode, false));
            mockResourcesApiClient.Setup(x => x.GetEmployee(It.IsAny<string>())).ReturnsAsync(GetFakeEmployee);

            var sut = new SecurityUserService(mockSecurityUserRepo.Object, mockResourcesApiClient.Object,
              optionsAccessor.Object, mockADService.Object);

            //Act
            var employee = await sut.GetEmployeeByEmployeeCode(employeeCode);

            //Assert
            employee.Should().BeOfType<Employee>();
            employee.Token.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("37995")]
        public async Task GetEmployeeByEmployeeCode_should_return_employee_withoutToken(string employeeCode)
        {
            //Arrange
            var mockSecurityUserRepo = new Mock<ISecurityUserRepository>();
            var mockResourcesApiClient = new Mock<IResourcesApiClient>();
            var mockADService = new Mock<IADservice>();
            var optionsAccessor = new Mock<IOptionsSnapshot<AppSettingsConfiguration>>();

            mockSecurityUserRepo.Setup(x => x.Authenticate(employeeCode))
                .ReturnsAsync(GetUnAuthorizedUser());
            mockResourcesApiClient.Setup(x => x.GetEmployee(It.IsAny<string>())).ReturnsAsync(GetFakeEmployee);

            var sut = new SecurityUserService(mockSecurityUserRepo.Object, mockResourcesApiClient.Object,
               optionsAccessor.Object, mockADService.Object);

            //Act
            var employee = await sut.GetEmployeeByEmployeeCode(employeeCode);

            //Assert
            employee.Should().BeOfType<Employee>();
            employee.Token.Should().BeNullOrEmpty();
        }

        private bool IsMemberOfADGroup(string accountName, string userType)
        {
            return (accountName == "Global Staffing Data Access" && userType == "HRUser") ||
                (accountName == "Global Staffing Development Team" && userType == "SuperAdminUser");
        }

        private SecurityUser GetAuthorizedUser(string employeeCode, bool isAdmin)
        {
            return new SecurityUser { EmployeeCode = employeeCode, IsAdmin = isAdmin };
        }

        private SecurityUser GetUnAuthorizedUser()
        {
            return null;
        }

        private Employee GetFakeEmployee()
        {
            return new Employee
            {
                EmployeeCode = "37995",
                FirstName = "Nitin",
                LastName = "Jain",
                FullName = "Jain, Nitin",
                LevelGrade = "N6",
                LevelName = "Administrator",

                Office = new Office
                {
                    OfficeCode = 332,
                    OfficeAbbreviation = "NDS",
                    OfficeName = "New Delhi - BCC"
                },
                OperatingOffice = new Office
                {
                    OfficeCode = 332,
                    OfficeAbbreviation = "NDS",
                    OfficeName = "New Delhi - BCC"
                },
                InternetAddress = "nitin.jain@bain.com",
                ProfileImageUrl = string.Empty,
                FTE = 1
            };
        }
    }
}