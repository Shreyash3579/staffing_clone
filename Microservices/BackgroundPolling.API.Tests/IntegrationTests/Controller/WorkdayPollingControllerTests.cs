using Xunit;

namespace BackgroundPolling.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "BackgroundPolling.API.CaseController")]
    public class WorkdayPollingControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public WorkdayPollingControllerTests(TestServerHost testServerHost)
        {
            _testServer = testServerHost;
        }

        //[Fact]
        //public async Task UpdateCostForPendingPromotionsTransitionsAndTransfers_should_not_return_error()
        //{
        //    var response =
        //        await _testServer.Client.PutAsync($"/api/workdaypolling/pendingTransactions", null);

        //    response.EnsureSuccessStatusCode();
        //}

        //[Fact]
        //public async Task UpdateCostForFutureLOAs_should_not_return_error()
        //{
        //    var response =
        //        await _testServer.Client.PutAsync($"/api/workdaypolling/futureLOAs", null);

        //    response.EnsureSuccessStatusCode();
        //}

        //[Fact]
        //public async Task UpdateCostForAnalytics_should_not_return_error()
        //{
        //    var response =
        //        await _testServer.Client.PutAsync($"/api/workdaypolling/costForAnalytics", null);

        //    response.EnsureSuccessStatusCode();
        //}

        //[Fact]
        //public async Task UpdateAnalyticsDataForDeletedTransactions_should_not_return_error()
        //{
        //    var response =
        //        await _testServer.Client.PutAsync($"/api/workdaypolling/analyticsDataUpdateForDeletedTransactions", null);

        //    response.EnsureSuccessStatusCode();
        //}

        //[Fact]
        //public async Task UpdateAvailabilityDataForEmployeesWithNoAllocations_should_not_return_error()
        //{
        //    var response =
        //        await _testServer.Client.PutAsync($"/api/workdaypolling/availabilityDataForEmployeesWithNoAllocations", null);

        //    response.EnsureSuccessStatusCode();
        //}

        //[Fact]
        //public async Task UpdateServiceLineInScheduleMaster_should_not_return_error()
        //{
        //    var response =
        //        await _testServer.Client.PutAsync($"/api/workdaypolling/updateServiceLineInScheduleMaster", null);

        //    response.EnsureSuccessStatusCode();
        //}
    }
}
