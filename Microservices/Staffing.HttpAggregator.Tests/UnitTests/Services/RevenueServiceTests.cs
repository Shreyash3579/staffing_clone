using FluentAssertions;
using Moq;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Services;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.HttpAggregator.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.HttpAggregator.Services")]
    public class RevenueServiceTests
    {
        [Theory]
        [InlineData(null, null, null, "Client Code can not be null or 0")]
        [InlineData(18507, null, null, "Opportunity/Pipeline Id can not be empty/null")]
        [InlineData(18507, 0, null, "Case code can not be or less than 0")]
        [InlineData(18507, null, "", "Opportunity/Pipeline Id can not be empty/null")]
        public async Task GetRevenueByCaseCodeAndClientCode_should_return_exceptions(int? clientCode, int? caseCode,
            string pipelineId, string errorMessage)
        {
            //Arrange
            var mockRevenueAPIClient = new Mock<IRevenueApiClient>();
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            var sut = new RevenueService(mockRevenueAPIClient.Object, mockStaffingAPIClient.Object);

            //Act
            var exception = await Record.ExceptionAsync(
                () => sut.GetRevenueByCaseCodeAndClientCode(clientCode, caseCode, pipelineId, "US")
            );

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData(18507, 171, null)]
        public async Task GetRevenueByCaseCodeAndClientCode_should_return_case_revenue_for_three_months(int? clientCode, int? caseCode, string pipelineId)
        {
            //Arrange

            //since we just require 3 months of revenue data i.e. previous, current, next month
            //we're initializing initialMonth and finalMonth with the difference of 1 and -1 resp.
            //so if we updated the value in config settings from '3' to '5' we should update these values 
            //to 2 as well example DateTime.Now.AddMonths(-2).Month and DateTime.Now.AddMonths(2).Month
            var initialMonth = DateTime.Now.AddMonths(-1).Month;
            var finalMonth = DateTime.Now.AddMonths(1).Month;
            DateTime startDate = new DateTime(DateTime.Now.Year, initialMonth, 1);
            DateTime endDate = new DateTime(DateTime.Now.Year, finalMonth, 1).AddMonths(1).AddDays(-1);

            var mockRevenueAPIClient = new Mock<IRevenueApiClient>();
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            mockRevenueAPIClient.Setup(x => x.GetRevenueByClientCodeAndCaseCode(clientCode, caseCode, startDate, endDate)).ReturnsAsync(GetFakeCaseRevenue());
            mockStaffingAPIClient.Setup(x => x.GetCurrencyRatesByCurrencyCodesAndDate("US", "B", startDate, endDate)).ReturnsAsync(FakeCurrencyResponse());

            var sut = new RevenueService(mockRevenueAPIClient.Object, mockStaffingAPIClient.Object);

            var results = await sut.GetRevenueByCaseCodeAndClientCode(clientCode, caseCode, pipelineId, "US");
            results.serviceLinesData.Count.Should().BeGreaterThan(0);
            results.aggregatedRevenueByMonths.Count.Should().BeGreaterThan(0);
            results.aggregatedRevenueByMonths.Count.Should().Equals(3);
        }

        [Theory]
        [InlineData(31928, 1, null)]
        public async Task GetRevenueByCaseCodeAndClientCode_should_return_case_revenue_for_adjacent_months(int? clientCode, int? caseCode, string pipelineId)
        {
            //Arrange

            //since we just require 3 months of revenue data i.e. previous, current, next month
            //we're initializing initialMonth and finalMonth with the difference of 1 and -1 resp.
            //so if we updated the value in config settings from '3' to '5' we should update these values 
            //to 2 as well example DateTime.Now.AddMonths(-2).Month and DateTime.Now.AddMonths(2).Month
            var initialMonth = DateTime.Now.AddMonths(-1).Month;
            var finalMonth = DateTime.Now.AddMonths(1).Month;
            DateTime startDate = new DateTime(DateTime.Now.Year, initialMonth, 1);
            DateTime endDate = new DateTime(DateTime.Now.Year, finalMonth, 1).AddMonths(1).AddDays(-1);

            var mockRevenueAPIClient = new Mock<IRevenueApiClient>();
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            mockRevenueAPIClient.Setup(x => x.GetRevenueByClientCodeAndCaseCode(clientCode, caseCode, startDate, endDate)).ReturnsAsync(GetFakeAdjacentCaseRevenue());
            mockStaffingAPIClient.Setup(x => x.GetCurrencyRatesByCurrencyCodesAndDate("NK,US", "B", startDate, endDate)).ReturnsAsync(FakeMultipleCurrencyResponse());

            var sut = new RevenueService(mockRevenueAPIClient.Object, mockStaffingAPIClient.Object);

            var results = await sut.GetRevenueByCaseCodeAndClientCode(clientCode, caseCode, pipelineId, "US");
            results.serviceLinesData.Count.Should().BeGreaterThan(0);
            results.aggregatedRevenueByMonths.Count.Should().BeGreaterThan(0);
            results.aggregatedRevenueByMonths.Count.Should().Equals(3);
        }

        [Theory]
        [InlineData(18507, null, "3953c785-3535-4b18-a549-dc7217b1afaf")]
        public async Task GetRevenueByCaseCodeAndClientCode_should_return_opp_revenue_for_three_months(int? clientCode, int? caseCode, string pipelineId)
        {
            //Arrange

            //since we just require 3 months of revenue data i.e. previous, current, next month
            //we're initializing initialMonth and finalMonth with the difference of 1 and -1 resp.
            //so if we updated the value in config settings from '3' to '5' we should update these values 
            //to 2 as well example DateTime.Now.AddMonths(-2).Month and DateTime.Now.AddMonths(2).Month
            var initialMonth = DateTime.Now.AddMonths(-1).Month;
            var finalMonth = DateTime.Now.AddMonths(1).Month;
            DateTime startDate = new DateTime(DateTime.Now.Year, initialMonth, 1);
            DateTime endDate = new DateTime(DateTime.Now.Year, finalMonth, 1).AddMonths(1).AddDays(-1);

            var mockRevenueAPIClient = new Mock<IRevenueApiClient>();
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            mockRevenueAPIClient.Setup(x => x.GetRevenueByClientCodeAndCaseCode(clientCode, caseCode, startDate, endDate)).ReturnsAsync(GetFakeOpportunityRevenue());
            mockStaffingAPIClient.Setup(x => x.GetCurrencyRatesByCurrencyCodesAndDate("US", "B", startDate, endDate)).ReturnsAsync(FakeCurrencyResponse());

            var sut = new RevenueService(mockRevenueAPIClient.Object, mockStaffingAPIClient.Object);

            var results = await sut.GetRevenueByCaseCodeAndClientCode(clientCode, caseCode, pipelineId, "US");
            results.serviceLinesData.Count.Should().BeGreaterThan(0);
            results.aggregatedRevenueByMonths.Count.Should().BeGreaterThan(0);
            results.aggregatedRevenueByMonths.Count.Should().Equals(3);
        }

        private IEnumerable<CurrencyRate> FakeCurrencyResponse()
        {
            var currencyApiResponse = new List<CurrencyRate>
            {
                new CurrencyRate
                {
                    CurrencyCode = "US",
                    CurrencyRateTypeCode = "B",
                    UsdRate = 1,
                    EffectiveDate = Convert.ToDateTime("2020-01-01T03:00:00-04:00")
                }
            };
            return currencyApiResponse;
        }

        private IEnumerable<CurrencyRate> FakeMultipleCurrencyResponse()
        {
            var currencyApiResponse = new List<CurrencyRate>
            {
                new CurrencyRate
                {
                    CurrencyCode = "NK",
                    CurrencyRateTypeCode = "B",
                    UsdRate = 0.1138952164,
                    EffectiveDate = Convert.ToDateTime("2020-01-01T03:00:00-04:00")
                },
                new CurrencyRate
                {
                    CurrencyCode = "US",
                    CurrencyRateTypeCode = "B",
                    UsdRate = 1,
                    EffectiveDate = Convert.ToDateTime("2020-01-01T03:00:00-04:00")
                }
            };
            return currencyApiResponse;
        }

        private IList<Revenue> GetFakeCaseRevenue()
        {
            var rawFakeCaseRevenue = new List<Revenue>
            {
                new Revenue
                {
                    CaseCode = 171,
                    ClientCode = 18507,
                    CurrencyCode = "US",
                    ManagementActivity = -64550,
                    OfficeCode = 110,
                    OpportunityId = null,
                    ServiceLineCode = "SL0001",
                    StartDate = Convert.ToDateTime("2020-06-01T00:00:00"),
                    EndDate = Convert.ToDateTime("2020-06-30T00:00:00"),
                }
            };

            return rawFakeCaseRevenue;
        }

        private IList<Revenue> GetFakeAdjacentCaseRevenue()
        {
            var rawFakeCaseRevenue = new List<Revenue>
            {
                new Revenue
                {
                    CaseCode = 1,
                    ClientCode = 31928,
                    CurrencyCode = "NK",
                    ManagementActivity = 931251,
                    OfficeCode = 525,
                    OpportunityId = null,
                    ServiceLineCode = "SL0020",
                    StartDate = Convert.ToDateTime("2020-08-01T00:00:00"),
                    EndDate = Convert.ToDateTime("2020-08-31T00:00:00"),
                },
                new Revenue
                {
                    CaseCode = 1,
                    ClientCode = 31928,
                    CurrencyCode = "US",
                    ManagementActivity = 123302,
                    OfficeCode = 503,
                    OpportunityId = null,
                    ServiceLineCode = "SL0006",
                    StartDate = Convert.ToDateTime("2020-06-01T00:00:00"),
                    EndDate = Convert.ToDateTime("2020-06-30T00:00:00"),
                },
                new Revenue
                {
                    CaseCode = 1,
                    ClientCode = 31928,
                    CurrencyCode = "US",
                    ManagementActivity = -123302,
                    OfficeCode = 525,
                    OpportunityId = null,
                    ServiceLineCode = "SL0006",
                    StartDate = Convert.ToDateTime("2020-06-01T00:00:00"),
                    EndDate = Convert.ToDateTime("2020-06-30T00:00:00"),
                }
            };

            return rawFakeCaseRevenue;
        }

        private List<Revenue> GetFakeOpportunityRevenue()
        {
            var rawFakeOppRevenue = new List<Revenue>
            {
                new Revenue
                {
                    OfficeCode=171,
                    CurrencyCode="US",
                    CaseCode=1,
                    ClientCode=28738,
                    StartDate=Convert.ToDateTime("2020-06-01T00:00:00"),
                    EndDate=Convert.ToDateTime("2020-06-30T00:00:00"),
                    ManagementActivity=608333,
                    ServiceLineCode="SL0001",
                    OpportunityId="3953c785-3535-4b18-a549-dc7217b1afaf"
                },
                new Revenue
                {
                    OfficeCode=171,
                    CurrencyCode="US",
                    CaseCode=1,
                    ClientCode=28738,
                    StartDate=Convert.ToDateTime("2020-07-01T00:00:00"),
                    EndDate=Convert.ToDateTime("2020-07-13T00:00:00"),
                    ManagementActivity=152085,
                    ServiceLineCode="SL0001",
                    OpportunityId="3953c785-3535-4b18-a549-dc7217b1afaf"
                }
            };

            return rawFakeOppRevenue;
        }

    }

}
