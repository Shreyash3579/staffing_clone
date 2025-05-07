using FluentAssertions;
using Newtonsoft.Json;
using Staffing.Authentication.Models;
using Staffing.Authentication.Tests.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace Staffing.Authentication.Tests.IntegrationsTests.Controller
{
    [Trait("IntegrationTest", "SecurityUser.API.SecurityUserController")]
    public class SecurityUserControllerTests : IClassFixture<TestServerHost>
    {
        public SecurityUserControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        private readonly TestServerHost _testServer;

        [InlineData("Staffing", "ImOivLnEiP/jaTt+LlMj5ef0KgogNLiSPqqHPOb5doM=")]
        public async Task AuthenticateApp_should_return_bearerToken_for_authorizedApp(string appName, string appSecret)
        {
            //Arrange
            var appSecretEncoded = HttpUtility.UrlEncode(appSecret);

            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/securityUser/authenticate?appName={appName}&appSecret={appSecretEncoded}");

            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadAsStringAsync();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            token.Should().NotBeNullOrEmpty();
            token.Should().StartWith("Bearer");
        }

        [InlineData("37995","SuperAdminUser",true)]
        [InlineData("52045", "HRUser", false)]
        [InlineData("1588", "BossUser", true)]
        public async Task impersonatedUser_should_return_userWithToken(string userCode,
            string expectedUserType, bool expextedIsBossUser)
        {
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/securityUser/impersonatedUser?userCode={userCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var impersonatedUser = JsonConvert.DeserializeObject<Employee>(responseString);
            var decodedToken = JwtTokenHelper.GetDecodedJwtToken(impersonatedUser.Token);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            impersonatedUser.Should().BeAssignableTo<Employee>();
            impersonatedUser.EmployeeCode.Should().Be("37995");
            impersonatedUser.FirstName.Should().Be("Nitin");
            impersonatedUser.LastName.Should().Be("Jain");
            impersonatedUser.FullName.Should().Be("Jain, Nitin");
            impersonatedUser.Token.Should().NotBeNullOrEmpty(); 
            decodedToken.Claims.First(claim => claim.Type == "EmployeeCode").Value.Should().Be(userCode);
            Convert.ToBoolean(decodedToken.Claims.First(claim => claim.Type == "IsBossUser").Value).Should().Be(expextedIsBossUser);
            decodedToken.Claims.First(claim => claim.Type == "UserType").Value.Should().Be(expectedUserType);
        }

        [Fact]
        public async Task loggedInUser_should_return_UnAuthorizedStatus()
        {
            //Act
            var response =
                await _testServer.Client.GetAsync("/api/securityUser/loggedInUser");
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}