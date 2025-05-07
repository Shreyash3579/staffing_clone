using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Collection("Staffing.API.Integration")]
    [Trait("IntegrationTest", "Staffing.API.NotificationController")]
    public class NotificationControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public NotificationControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData("37995", "110,112,115")]
        public async Task GetUserNotifications_should_return_notificationsForUserSelectedOfficesInDemandView(string employeeCode, string officeCodes)
        {
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/notification?employeeCode={employeeCode}&officeCodes={officeCodes}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var userNotifications = JsonConvert.DeserializeObject<IEnumerable<UserNotification>>(responseString).ToList();
            var expectedOrderedUserNotifications = userNotifications.OrderByDescending(x => x.EndDate);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            userNotifications.Count().Should().BeGreaterOrEqualTo(0);
            userNotifications.ForEach(un => un.NotificationId.ToString().Should().NotBeNullOrEmpty());
            userNotifications.ForEach(un => un.NotificationStatus.ToString().Should().NotBeNullOrEmpty());
            expectedOrderedUserNotifications.SequenceEqual(userNotifications).Should().BeTrue(); //test ordering of elements
        }

        [Theory]
        [InlineData("37995", "110,112,115", 'R')]
        public async Task UpdateUserNotificationStatus_should_updateNotificationStatus(string employeeCode, string officeCodes, char notificationStatus)
        {
            //Arrange
            var responseNotifications =
                await _testServer.Client.GetAsync(
                    $"/api/notification?employeeCode={employeeCode}&officeCodes={officeCodes}");
            var responseNotificationsString = await responseNotifications.Content.ReadAsStringAsync();
            var userNotifications = JsonConvert.DeserializeObject<IEnumerable<UserNotification>>(responseNotificationsString).ToList();
            if (userNotifications.Count > 0)
            {
                var userNotificationToUpdate = userNotifications.FirstOrDefault();

                var payloadObject = new
                {
                    notificationId = userNotificationToUpdate.NotificationId.ToString(),
                    employeeCode,
                    notificationStatus
                };

                var payload = JsonConvert.SerializeObject(payloadObject);
                var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

                //Act
                await _testServer.Client.PutAsync($"/api/notification", httpContent);

                var response = await _testServer.Client.GetAsync(
                    $"/api/notification?employeeCode={employeeCode}&officeCodes={officeCodes}");

                response.EnsureSuccessStatusCode();
                var updatedUserNotifications = JsonConvert
                    .DeserializeObject<IEnumerable<UserNotification>>(responseNotificationsString).ToList();
                var updatedUserNotification =
                    updatedUserNotifications.FirstOrDefault(x =>
                        x.NotificationId == userNotificationToUpdate.NotificationId);

                //Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                updatedUserNotification.NotificationStatus.Should().Be(notificationStatus);
            }
        }

        [Theory]
        [InlineData("37995", "39209", "110,112,115", 'R')]
        public async Task UpdateUserNotificationStatus_should_notUpdateNotificationStatusForOtherUser(string employeeCode, string otherUserEmployeeCode, string officeCodes, char notificationStatus)
        {
            //Arrange
            var responseNotifications =
                await _testServer.Client.GetAsync(
                    $"/api/notification?employeeCode={employeeCode}&officeCodes={officeCodes}");
            var responseNotificationsString = await responseNotifications.Content.ReadAsStringAsync();
            var userNotifications = JsonConvert.DeserializeObject<IEnumerable<UserNotification>>(responseNotificationsString).ToList();
            var userNotificationToUpdate = userNotifications.FirstOrDefault(x => x.NotificationStatus == 'U');
            if (userNotificationToUpdate != null)
            {
                var payloadObject = new
                {
                    notificationId = userNotificationToUpdate?.NotificationId.ToString(),
                    employeeCode,
                    notificationStatus
                };

                var payload = JsonConvert.SerializeObject(payloadObject);
                var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

                //Act
                await _testServer.Client.PutAsync($"/api/notification", httpContent);

                var response = await _testServer.Client.GetAsync(
                    $"/api/notification?employeeCode={otherUserEmployeeCode}&officeCodes={officeCodes}");

                response.EnsureSuccessStatusCode();
                var otherUserNotifications = JsonConvert
                    .DeserializeObject<IEnumerable<UserNotification>>(responseNotificationsString).ToList();
                var otherUserNotification =
                    otherUserNotifications.FirstOrDefault(x =>
                        x.NotificationId == userNotificationToUpdate.NotificationId);

                //Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                otherUserNotification.NotificationStatus.Should().Be('U');
            }
        }
    }
}
