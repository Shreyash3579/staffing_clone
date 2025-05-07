using AutoFixture;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.AnalyticsService;
using Staffing.Analytics.API.Models;
using Staffing.Analytics.API.Models.Workday;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using EmployeeTransaction = Staffing.Analytics.API.Models.Workday.EmployeeTransaction;
using Office = Staffing.Analytics.API.Models.Office;

namespace Staffing.Analytics.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.API.Services")]
    public class AnalyticsServiceTests
    {
        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task
            GetResourcesAllocationsWithBillRate_should_return_billRatesForAllocationWithinCurrentBillRateDateRange(
                ResourceAllocation[] resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);
            var billRate = GetFakeBillRates().FirstOrDefault(x => x.Status == "Current" && x.LevelGrade == "M9");
            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object,mockResourceAvailabilityRepository.Object,mockAnalyticsRepository.Object,
                null,null,null,null,null,null,mockBackgroundJob.Object,null);

            //Act
            var allocationsWithBillRates = await sut.GetResourcesAllocationsWithBillRate(resourcesAllocations);

            //Assert
            allocationsWithBillRates.ToList().Count.Should().Be(2);

            allocationsWithBillRates.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * billRate.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            allocationsWithBillRates.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * billRate.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            allocationsWithBillRates.ToList().ForEach(x =>
                x.BillRate.Should().Be(billRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
            allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
            allocationsWithBillRates.ToList().ForEach(x => x.EffectiveCostReason.Should().BeNull());
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task
            GetResourcesAllocationsWithBillRate_should_return_billRatesForAllocationExceedsCurrentBillRateEndDate(
                ResourceAllocation[] resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 05, 31);
            var currentBillRate = GetFakeBillRates().FirstOrDefault(x => x.Status == "Current" && x.LevelGrade == "M9");
            var pendingBillRate = GetFakeBillRates().FirstOrDefault(x => x.Status == "Pending" && x.LevelGrade == "M9");
            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.GetResourcesAllocationsWithBillRate(resourcesAllocations);

            //Assert
            allocationsWithBillRates.ToList().Count.Should().Be(6);
            allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));

            var resourceBillRateForCurrentRate = allocationsWithBillRates.ToList().GetRange(0, 4);
            resourceBillRateForCurrentRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * currentBillRate.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForCurrentRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * currentBillRate.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForCurrentRate.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(currentBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));

            var resourceBillRateForPendingRate = allocationsWithBillRates.ToList().GetRange(4, 2);
            resourceBillRateForPendingRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * pendingBillRate.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForPendingRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * pendingBillRate.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForPendingRate.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(pendingBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task
            GetResourcesAllocationsWithBillRate_should_return_billRatesForAllocationPrecedesCurrentBillRateStartDate(
                ResourceAllocation[] resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2018, 08, 05);

            var currentBillRate = GetFakeBillRates().FirstOrDefault(x => x.Status == "Current" && x.LevelGrade == "M9");
            var historicalBillRatePre2019 =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Historical" && x.LevelGrade == "M9" && x.EndDate < new DateTime(2019, 01, 01));
            var historicalBillRatePost2019 =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Historical" && x.LevelGrade == "M9" && x.EndDate >= new DateTime(2019, 01, 01));

            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.GetResourcesAllocationsWithBillRate(resourcesAllocations);

            //Assert
            allocationsWithBillRates.ToList().Count.Should().Be(18);
            allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));

            var resourceBillRateForCurrentRate = allocationsWithBillRates.ToList().GetRange(0, 4);
            resourceBillRateForCurrentRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * currentBillRate.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForCurrentRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * currentBillRate.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForCurrentRate.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(currentBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));

            var resourceBillRateForHistoricalRate = allocationsWithBillRates.ToList().GetRange(4, 9);
            resourceBillRateForHistoricalRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * historicalBillRatePost2019.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * historicalBillRatePost2019.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRate.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(historicalBillRatePost2019.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));

            var resourceBillRateForHistoricalRatePre2019 = allocationsWithBillRates.ToList().GetRange(13, 5);
            resourceBillRateForHistoricalRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * historicalBillRatePre2019.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * historicalBillRatePre2019.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRate.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(historicalBillRatePre2019.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task
            GetResourcesAllocationsWithBillRate_should_return_billRatesForAllocationPrecedesCurrentBillRateDateRange(
                ResourceAllocation[] resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2018, 10, 05);
            resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2019, 05, 31);

            var historicalBillRatePre2019 =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Historical" && x.LevelGrade == "M9" && x.EndDate < new DateTime(2019, 01, 01));
            var historicalBillRatePost2019 =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Historical" && x.LevelGrade == "M9" && x.EndDate >= new DateTime(2019, 01, 01));

            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.GetResourcesAllocationsWithBillRate(resourcesAllocations);

            //Assert
            allocationsWithBillRates.ToList().Count.Should().Be(8);
            allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));

            var resourceBillRateForHistoricalRatePost2019 = allocationsWithBillRates.ToList().GetRange(0, 5);
            resourceBillRateForHistoricalRatePost2019.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * historicalBillRatePost2019.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRatePost2019.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * historicalBillRatePost2019.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRatePost2019.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(historicalBillRatePost2019.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));

            var resourceBillRateForHistoricalRatePre2019 = allocationsWithBillRates.ToList().GetRange(5, 3);
            resourceBillRateForHistoricalRatePre2019.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * historicalBillRatePre2019.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRatePre2019.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * historicalBillRatePre2019.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRatePre2019.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(historicalBillRatePre2019.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task
            GetResourcesAllocationsWithBillRate_should_return_billRatesForAllocationExceedsCurrentBillRateDateRange(
                ResourceAllocation[] resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2020, 04, 05);
            resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 08, 31);
            var pendingBillRate =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Pending" && x.LevelGrade == "M9");
            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.GetResourcesAllocationsWithBillRate(resourcesAllocations);

            //Assert
            allocationsWithBillRates.ToList().Count.Should().Be(5);

            allocationsWithBillRates.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * pendingBillRate.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            allocationsWithBillRates.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * pendingBillRate.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            allocationsWithBillRates.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(pendingBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
            allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task GetResourcesAllocationsWithBillRate_should_return_billRateWithinAllocationDateRange(
            ResourceAllocation[] resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2019, 06, 27);
            resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 06, 27);
            var pendingBillRate =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Pending" && x.LevelGrade == "M9");
            var currentBillRate =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Current" && x.LevelGrade == "M9");
            var HistoricalBillRate =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Historical" && x.LevelGrade == "M9" && x.EndDate >= new DateTime(2019, 01, 01));
            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.GetResourcesAllocationsWithBillRate(resourcesAllocations);

            //Assert
            allocationsWithBillRates.ToList().Count.Should().Be(13);
            allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));

            var resourceBillRateForPendingRate = allocationsWithBillRates.ToList().GetRange(0, 3);
            resourceBillRateForPendingRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * pendingBillRate.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForPendingRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * pendingBillRate.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForPendingRate.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(pendingBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));

            var resourceBillRateForCurrentRate = allocationsWithBillRates.ToList().GetRange(3, 6);
            resourceBillRateForCurrentRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * currentBillRate.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForCurrentRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * currentBillRate.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForCurrentRate.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(currentBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));

            var resourceBillRateForHistoricalRate = allocationsWithBillRates.ToList().GetRange(9, 4);
            resourceBillRateForHistoricalRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * HistoricalBillRate.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * HistoricalBillRate.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForHistoricalRate.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(HistoricalBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
        }

        //[Theory]
        //[ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        //public async Task
        //    GetResourcesAllocationsWithBillRate_should_return_billRateForBillCode1_ifBillRateNotAvailableForEmployeeBillCode(
        //        ResourceAllocation[] resourcesAllocations)
        //{
        //    //Arrange
        //    var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
        //    var mockCCMAPIClient = new Mock<ICCMApiClient>();
        //    mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
        //        .ReturnsAsync(GetFakeBillRates);

        //    resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2019, 06, 27);
        //    resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 06, 27);
        //    resourcesAllocations.FirstOrDefault().BillCode = 3;

        //    var pendingBillRate =
        //        GetFakeBillRates().FirstOrDefault(x => x.Status == "Pending" && x.LevelGrade == "M9");
        //    var currentBillRate =
        //        GetFakeBillRates().FirstOrDefault(x => x.Status == "Current" && x.LevelGrade == "M9");
        //    var HistoricalBillRate =
        //        GetFakeBillRates().FirstOrDefault(x => x.Status == "Historical" && x.LevelGrade == "M9" && x.EndDate >= new DateTime(2019, 01, 01));
        //    var mockResourcesApiClient = new Mock<IResourceApiClient>();
        //    var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
        //    var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
        //    var mockBackgroundJob = new Mock<IBackgroundJobClient>();

        //    var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
        //        mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
        //        mockBackgroundJob.Object);

        //    //Act
        //    var allocationsWithBillRates = await sut.GetResourcesAllocationsWithBillRate(resourcesAllocations);

        //    //Assert
        //    allocationsWithBillRates.ToList().Count.Should().Be(13);
        //    allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
        //    allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
        //    allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));

        //    var resourceBillRateForPendingRate = allocationsWithBillRates.ToList().GetRange(0, 3);
        //    resourceBillRateForPendingRate.ToList()
        //        .ForEach(x =>
        //            x.ActualCost.Should().Be(x.Allocation * pendingBillRate.Rate /
        //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
        //    resourceBillRateForPendingRate.ToList().ForEach(x =>
        //        x.EffectiveCost.Should().Be(x.Allocation * pendingBillRate.Rate /
        //                                    GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
        //    resourceBillRateForPendingRate.ToList().ForEach(x =>
        //        x.BillRate.Should()
        //            .Be(pendingBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));

        //    var resourceBillRateForCurrentRate = allocationsWithBillRates.ToList().GetRange(3, 6);
        //    resourceBillRateForCurrentRate.ToList()
        //        .ForEach(x =>
        //            x.ActualCost.Should().Be(x.Allocation * currentBillRate.Rate /
        //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
        //    resourceBillRateForCurrentRate.ToList().ForEach(x =>
        //        x.EffectiveCost.Should().Be(x.Allocation * currentBillRate.Rate /
        //                                    GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
        //    resourceBillRateForCurrentRate.ToList().ForEach(x =>
        //        x.BillRate.Should()
        //            .Be(currentBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));

        //    var resourceBillRateForHistoricalRate = allocationsWithBillRates.ToList().GetRange(9, 4);
        //    resourceBillRateForHistoricalRate.ToList()
        //        .ForEach(x =>
        //            x.ActualCost.Should().Be(x.Allocation * HistoricalBillRate.Rate /
        //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
        //    resourceBillRateForHistoricalRate.ToList().ForEach(x =>
        //        x.EffectiveCost.Should().Be(x.Allocation * HistoricalBillRate.Rate /
        //                                    GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
        //    resourceBillRateForHistoricalRate.ToList().ForEach(x =>
        //        x.BillRate.Should()
        //            .Be(HistoricalBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
        //}

        //[Theory]
        //[ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        //public async Task GetResourcesAllocationsWithBillRate_should_return_EffectiveCostZeroForInternalPD(
        //    ResourceAllocation[] resourcesAllocations)
        //{
        //    //Arrange
        //    var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
        //    var mockCCMAPIClient = new Mock<ICCMApiClient>();
        //    mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
        //        .ReturnsAsync(GetFakeBillRates);

        //    resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2020, 01, 01);
        //    resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 03, 27);
        //    resourcesAllocations.FirstOrDefault().BillCode = 3;
        //    resourcesAllocations.FirstOrDefault().InvestmentCode = 5;
        //    resourcesAllocations.FirstOrDefault().InvestmentName = "Internal PD";

        //    var currentBillRate =
        //        GetFakeBillRates().FirstOrDefault(x => x.Status == "Current" && x.LevelGrade == "M9");

        //    var mockResourcesApiClient = new Mock<IResourceApiClient>();
        //    var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
        //    var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
        //    var mockBackgroundJob = new Mock<IBackgroundJobClient>();

        //    var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
        //        mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
        //        mockBackgroundJob.Object);

        //    //Act
        //    var allocationsWithBillRates = await sut.GetResourcesAllocationsWithBillRate(resourcesAllocations);

        //    //Assert
        //    allocationsWithBillRates.ToList().Count.Should().Be(3);

        //    var resourceBillRateForCurrentRate = allocationsWithBillRates.ToList().GetRange(0, 3);
        //    resourceBillRateForCurrentRate.ToList()
        //        .ForEach(x =>
        //            x.ActualCost.Should().Be(x.Allocation * currentBillRate.Rate /
        //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
        //    resourceBillRateForCurrentRate.ToList().ForEach(x => x.EffectiveCost.Should().Be(0));
        //    resourceBillRateForCurrentRate.ToList().ForEach(x =>
        //        x.BillRate.Should()
        //            .Be(currentBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
        //    resourceBillRateForCurrentRate.ToList()
        //        .ForEach(x => x.EffectiveCostReason.Should().Be("Investment - Internal PD"));
        //}

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task
            GetResourcesAllocationsWithBillRate_should_return_billRatesForAllocationExceedsCurrentBillRateDateRange_havingMultiplePendingRate(
                ResourceAllocation[] resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2020, 04, 05);
            resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 12, 31);
            resourcesAllocations.FirstOrDefault().CurrentLevelGrade = "A7";
            var pendingBillRate =
                GetFakeBillRates()
                    .FirstOrDefault(x => x.Status == "Pending" && x.LevelGrade == "A7" && x.EndDate != null);
            var pendingBillRateWithEndDateNull =
                GetFakeBillRates()
                    .FirstOrDefault(x => x.Status == "Pending" && x.LevelGrade == "A7" && x.EndDate == null);
            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.GetResourcesAllocationsWithBillRate(resourcesAllocations);

            //Assert
            allocationsWithBillRates.ToList().Count.Should().Be(10);
            allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
            allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));

            var resourceBillRateForCurrentRate = allocationsWithBillRates.ToList().GetRange(0, 1);
            resourceBillRateForCurrentRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * pendingBillRate.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForCurrentRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * pendingBillRate.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForCurrentRate.ToList().ForEach(x =>
                x.BillRate.Should()
                    .Be(pendingBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));

            var resourceBillRateForPendingRate = allocationsWithBillRates.ToList().GetRange(1, 9);
            resourceBillRateForPendingRate.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * pendingBillRateWithEndDateNull.Rate /
                                             GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForPendingRate.ToList().ForEach(x =>
                x.EffectiveCost.Should().Be(x.Allocation * pendingBillRateWithEndDateNull.Rate /
                                            GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
            resourceBillRateForPendingRate.ToList().ForEach(x =>
                x.BillRate.Should().Be(pendingBillRateWithEndDateNull.Rate /
                                       GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task CreateAnaylticsReport_should_splitRows_per_pendingPromotions(
            string resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            var currentBillRateForA7 =
                GetFakeBillRates()
                    .FirstOrDefault(x => x.Status == "Current" && x.LevelGrade == "A7" && x.EndDate != null);
            var currentBillRateForM9 =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Current" && x.LevelGrade == "M9");
            var PendingBillRateForM9 =
                GetFakeBillRates().FirstOrDefault(x => x.Status == "Pending" && x.LevelGrade == "M9");

           // resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2020, 01, 01);
           // resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 07, 31);
           // resourcesAllocations.FirstOrDefault().CurrentLevelGrade = "A7";
           // resourcesAllocations.FirstOrDefault().EmployeeCode = "37810";

            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            mockResourcesApiClient.Setup(x => x.GetPendingTransactionsByEmployeeCodes(It.IsAny<string>()))
                .ReturnsAsync(GetFakePendingPromotion);

            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();
            mockBackgroundJob.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(Guid.NewGuid().ToString());

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null,null,null,null,null,null,mockBackgroundJob.Object,null);

            //Act
            var allocationsWithBillRates = await sut.CreateAnalyticsReport(resourcesAllocations);

            //Assert
           // allocationsWithBillRates.ToList().Count.Should().Be(7);
           // allocationsWithBillRates.ToList().Where(x => x.CurrentLevelGrade == "A7").Count().Should().Be(2);
           // allocationsWithBillRates.ToList().Where(x => x.CurrentLevelGrade == "M9").Count().Should().Be(5);
           // allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
           // allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
           // allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));
           //
           // allocationsWithBillRates.ToList().Where(x => x.CurrentLevelGrade == "A7").ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA7.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // allocationsWithBillRates.ToList().Where(x => x.CurrentLevelGrade == "A7").ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA7.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // allocationsWithBillRates.ToList().Where(x => x.CurrentLevelGrade == "A7").ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA7.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // allocationsWithBillRates.ToList().Where(x => x.CurrentLevelGrade == "A7").ToList()
           //     .ForEach(x => x.TransactionType.Should().BeNull());
           //
           // var resourceCurrentBillRateForM9 = allocationsWithBillRates.ToList().GetRange(2, 1);
           // resourceCurrentBillRateForM9.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForM9.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForM9.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForM9.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForM9.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForM9.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForM9.ForEach(x => x.EffectiveCostReason.Should().BeNull());
           //
           // var resourcePendingBillRateForM9 = allocationsWithBillRates.ToList().GetRange(3, 4);
           // resourcePendingBillRateForM9.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * PendingBillRateForM9.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateForM9.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * PendingBillRateForM9.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateForM9.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(PendingBillRateForM9.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourcePendingBillRateForM9.ForEach(x => x.EffectiveCostReason.Should().BeNull());
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task CreateAnaylticsReport_should_splitRows_per_pendingTransfers(
            string resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            var currentBillRateForBos =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A4" && x.OfficeCode == "110");
            var pendingBillRateWithEndDateForBos =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Pending" && x.LevelGrade == "A4" && x.OfficeCode == "110" && x.EndDate != null);
            var PendingBillRateWithEndDateNullForBos =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Pending" && x.LevelGrade == "A4" && x.OfficeCode == "110" && x.EndDate == null);

            var currentBillRateForAms =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A4" && x.OfficeCode == "265");
            var pendingBillRateWithEndDateForAms =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Pending" && x.LevelGrade == "A4" && x.OfficeCode == "265" && x.EndDate != null);
            var PendingBillRateWithEndDateNullForAms =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Pending" && x.LevelGrade == "A4" && x.OfficeCode == "265" && x.EndDate == null);

           // resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2020, 01, 01);
           // resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 04, 28);
           // resourcesAllocations.FirstOrDefault().CurrentLevelGrade = "A4";
           // resourcesAllocations.FirstOrDefault().EmployeeCode = "38333";

            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            mockResourcesApiClient.Setup(x => x.GetPendingTransactionsByEmployeeCodes(It.IsAny<string>()))
                .ReturnsAsync(GetFakePendingTransfers);

            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();
            mockBackgroundJob.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(Guid.NewGuid().ToString());

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.CreateAnalyticsReport(resourcesAllocations);

            //Assert
           // allocationsWithBillRates.ToList().Count.Should().Be(6);
           // allocationsWithBillRates.Where(x => x.OperatingOfficeCode == 110).Count().Should().Be(2);
           // allocationsWithBillRates.ToList().Where(x => x.OperatingOfficeCode == 265).Count().Should().Be(4);
           // allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
           // allocationsWithBillRates.Where(x => x.OperatingOfficeCode == 110).ToList()
           //     .ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
           // allocationsWithBillRates.Where(x => x.OperatingOfficeCode == 265).ToList()
           //     .ForEach(x => x.BillRateCurrency.Trim().Should().Be("EU"));
           // allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));
           //
           // var resourceCurrentBillRateForBos = allocationsWithBillRates.ToList().GetRange(0, 2);
           // resourceCurrentBillRateForBos.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForBos.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForBos.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForBos.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForBos.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForBos.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForBos.ForEach(x => x.EffectiveCostReason.Should().BeNull());
           //
           // var resourceCurrentBillRateForAms = allocationsWithBillRates.ToList().GetRange(2, 2);
           // resourceCurrentBillRateForAms.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForAms.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForAms.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForAms.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForAms.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForAms.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForAms.ForEach(x => x.EffectiveCostReason.Should().BeNull());
           // resourceCurrentBillRateForAms.ForEach(x => x.OperatingOfficeCode.Should().Be(265));

           // var resourcePendingBillRateWithEndDateForAms = allocationsWithBillRates.ToList().GetRange(4, 1);
           // resourcePendingBillRateWithEndDateForAms.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * pendingBillRateWithEndDateForAms.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateWithEndDateForAms.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * pendingBillRateWithEndDateForAms.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateWithEndDateForAms.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(pendingBillRateWithEndDateForAms.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourcePendingBillRateWithEndDateForAms.ForEach(x => x.EffectiveCostReason.Should().BeNull());
           // resourceCurrentBillRateForAms.ForEach(x => x.OperatingOfficeCode.Should().Be(265));
           //
           // var resourcePendingBillRateWithNullEndDateForAms = allocationsWithBillRates.ToList().GetRange(5, 1);
           // resourcePendingBillRateWithNullEndDateForAms.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * PendingBillRateWithEndDateNullForAms.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateWithNullEndDateForAms.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * PendingBillRateWithEndDateNullForAms.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateWithNullEndDateForAms.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(PendingBillRateWithEndDateNullForAms.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourcePendingBillRateWithNullEndDateForAms.ForEach(x => x.EffectiveCostReason.Should().BeNull());
           // resourceCurrentBillRateForAms.ForEach(x => x.OperatingOfficeCode.Should().Be(265));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task CreateAnaylticsReport_should_splitRows_per_pendingTransitions(
            string resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            var currentBillRate =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A4" && x.OfficeCode == "110");
            var pendingBillRateWithEndDate =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Pending" && x.LevelGrade == "A4" && x.OfficeCode == "110" && x.EndDate != null);
            var PendingBillRateWithEndDateNull =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Pending" && x.LevelGrade == "A4" && x.OfficeCode == "110" && x.EndDate == null);

           // resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2020, 01, 01);
           // resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 04, 28);
           // resourcesAllocations.FirstOrDefault().CurrentLevelGrade = "A4";
           // resourcesAllocations.FirstOrDefault().EmployeeCode = "36254";

            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            mockResourcesApiClient.Setup(x => x.GetPendingTransactionsByEmployeeCodes(It.IsAny<string>()))
                .ReturnsAsync(GetFakePendingTransitions);

            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();
            mockBackgroundJob.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(Guid.NewGuid().ToString());

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.CreateAnalyticsReport(resourcesAllocations);

            //Assert
           // allocationsWithBillRates.ToList().Count.Should().Be(6);
           // allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
           // allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Trim().Should().Be("US"));
           // allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));
           //
           // var resourceCurrentBillRate = allocationsWithBillRates.ToList().GetRange(0, 2);
           // resourceCurrentBillRate.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRate.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRate.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRate.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRate.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should()
           //             .Be(currentBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRate.ToList().ForEach(x => x.EffectiveCostReason.Should().BeNull());
           //
           // var resourceCurrentBillRate1 = allocationsWithBillRates.ToList().GetRange(2, 2);
           // resourceCurrentBillRate.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRate.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRate1.ToList().ForEach(x => x.EffectiveCost.Should().Be(0));
           // resourceCurrentBillRate1.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should()
           //             .Be(currentBillRate.Rate / GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRate1.ToList().ForEach(x => x.EffectiveCostReason.Should().Be("Transition"));
           //
           // var resourcePendingBillRateWithEndDate = allocationsWithBillRates.ToList().GetRange(4, 1);
           // resourcePendingBillRateWithEndDate.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * pendingBillRateWithEndDate.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateWithEndDate.ToList().ForEach(x => x.EffectiveCost.Should().Be(0));
           // resourcePendingBillRateWithEndDate.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(pendingBillRateWithEndDate.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourcePendingBillRateWithEndDate.ToList().ForEach(x => x.EffectiveCostReason.Should().Be("Transition"));
           //
           // var resourcePendingBillRateWithNullEndDate = allocationsWithBillRates.ToList().GetRange(5, 1);
           // resourcePendingBillRateWithNullEndDate.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * PendingBillRateWithEndDateNull.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateWithNullEndDate.ToList().ForEach(x => x.EffectiveCost.Should().Be(0));
           // resourcePendingBillRateWithNullEndDate.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(PendingBillRateWithEndDateNull.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourcePendingBillRateWithNullEndDate.ToList()
           //     .ForEach(x => x.EffectiveCostReason.Should().Be("Transition"));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task CreateAnaylticsReport_should_splitRows_per_pendingTransactions(
            string resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            var currentBillRateForA4Bos =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A4" && x.OfficeCode == "110");
            var currentBillRateForA4Ams =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A4" && x.OfficeCode == "265");
            var currentBillRateForM1Ams =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "M1" && x.OfficeCode == "265");
            var PendingBillRateForM1Ams =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Pending" && x.LevelGrade == "M1" && x.OfficeCode == "265");

            //resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2020, 01, 01);
            //resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 04, 28);
            //resourcesAllocations.FirstOrDefault().CurrentLevelGrade = "A4";
            //resourcesAllocations.FirstOrDefault().EmployeeCode = "36254";

            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            mockResourcesApiClient.Setup(x => x.GetPendingTransactionsByEmployeeCodes(It.IsAny<string>()))
                .ReturnsAsync(GetFakePendingTransactions);

            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();
            mockBackgroundJob.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(Guid.NewGuid().ToString());

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.CreateAnalyticsReport(resourcesAllocations);

            //Assert
           // allocationsWithBillRates.ToList().Count.Should().Be(6);
           // allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
           // allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));
           //
           // var resourceCurrentBillRateForA4Bos = allocationsWithBillRates.ToList().GetRange(0, 2);
           // resourceCurrentBillRateForA4Bos.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA4Bos.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4Bos.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA4Bos.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4Bos.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA4Bos.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForA4Bos.ToList().ForEach(x => x.EffectiveCostReason.Should().BeNull());
           // resourceCurrentBillRateForA4Bos.ToList().ForEach(x => x.BillRateCurrency.Should().Be("US"));
           //
           // var resourceCurrentBillRateForA4Ams = allocationsWithBillRates.ToList().GetRange(2, 1);
           // resourceCurrentBillRateForA4Ams.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA4Ams.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4Ams.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA4Ams.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4Ams.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA4Ams.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForA4Ams.ToList().ForEach(x => x.EffectiveCostReason.Should().BeNull());
           // resourceCurrentBillRateForA4Ams.ToList().ForEach(x => x.BillRateCurrency.Should().Be("EU"));
           //
           // var resourceCurrentBillRateForM1Ams = allocationsWithBillRates.ToList().GetRange(3, 1);
           // resourceCurrentBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForM1Ams.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForM1Ams.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForM1Ams.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForM1Ams.ToList().ForEach(x => x.EffectiveCostReason.Should().BeNull());
           // resourceCurrentBillRateForM1Ams.ToList().ForEach(x => x.BillRateCurrency.Should().Be("EU"));
           //
           // var resourcePendingBillRateForM1Ams = allocationsWithBillRates.ToList().GetRange(4, 1);
           // resourcePendingBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * PendingBillRateForM1Ams.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * PendingBillRateForM1Ams.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(PendingBillRateForM1Ams.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourcePendingBillRateForM1Ams.ToList().ForEach(x => x.EffectiveCostReason.Should().BeNull());
           // resourcePendingBillRateForM1Ams.ToList().ForEach(x => x.BillRateCurrency.Should().Be("EU"));
           //
           // var resourcePendingBillRateForM1 = allocationsWithBillRates.ToList().GetRange(5, 1);
           // resourcePendingBillRateForM1.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * PendingBillRateForM1Ams.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateForM1.ToList().ForEach(x => x.EffectiveCost.Should().Be(0));
           // resourcePendingBillRateForM1.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(PendingBillRateForM1Ams.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourcePendingBillRateForM1.ToList().ForEach(x => x.EffectiveCostReason.Should().Be("Transition"));
           // resourcePendingBillRateForM1.ToList().ForEach(x => x.BillRateCurrency.Should().Be("EU"));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task CreateAnalyticsReport_should_splitRows_per_pendingTransactionsAndHistoricalPromotions(
           string resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            var currentBillRateForA3Bos =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A3" && x.OfficeCode == "110");
            var currentBillRateForA4Bos =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A4" && x.OfficeCode == "110");
            var currentBillRateForA4Ams =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A4" && x.OfficeCode == "265");
            var currentBillRateForM1Ams =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "M1" && x.OfficeCode == "265");
            var PendingBillRateForM1Ams =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Pending" && x.LevelGrade == "M1" && x.OfficeCode == "265");

            //resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2019, 10, 01);
            //resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 04, 28);
            //resourcesAllocations.FirstOrDefault().CurrentLevelGrade = "A3";
            //resourcesAllocations.FirstOrDefault().EmployeeCode = "36254";

            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            mockResourcesApiClient.Setup(x => x.GetPendingTransactionsByEmployeeCodes(It.IsAny<string>()))
                .ReturnsAsync(GetFakePendingTransactions);
            mockResourcesApiClient.Setup(x => x.GetEmployeesIncludingTerminated()).ReturnsAsync(GetFakeResources);
            mockResourcesApiClient.Setup(x => x.GetEmployeesStaffingTransactions(It.IsAny<string>()))
                .ReturnsAsync(GetFakeEmployeeTransactions);

            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();
            mockBackgroundJob.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(Guid.NewGuid().ToString());

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null, null, null, null, null, null, mockBackgroundJob.Object, null);

            //Act
            var allocationsWithBillRates = await sut.CreateAnalyticsReport(resourcesAllocations);

            //Assert
           // allocationsWithBillRates.ToList().Count.Should().Be(9);
           // allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
           // allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));
           //
           // var resourceCurrentBillRateForA3Bos = allocationsWithBillRates.ToList().GetRange(0, 3);
           // resourceCurrentBillRateForA3Bos.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA3Bos.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA3Bos.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA3Bos.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA3Bos.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA3Bos.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForA3Bos.ToList().ForEach(x => x.BillRateCurrency.Should().Be("US"));
           //
           // var resourceCurrentBillRateForA4Bos = allocationsWithBillRates.ToList().GetRange(3, 2);
           // resourceCurrentBillRateForA4Bos.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA4Bos.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4Bos.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA4Bos.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4Bos.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA4Bos.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForA4Bos.ToList().ForEach(x => x.BillRateCurrency.Should().Be("US"));
           //
           // var resourceCurrentBillRateForA4Ams = allocationsWithBillRates.ToList().GetRange(5, 1);
           // resourceCurrentBillRateForA4Ams.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA4Ams.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4Ams.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA4Ams.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4Ams.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA4Ams.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForA4Ams.ToList().ForEach(x => x.BillRateCurrency.Should().Be("EU"));
           //
           // var resourceCurrentBillRateForM1Ams = allocationsWithBillRates.ToList().GetRange(6, 1);
           // resourceCurrentBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForM1Ams.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForM1Ams.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForM1Ams.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourceCurrentBillRateForM1Ams.ToList().ForEach(x => x.BillRateCurrency.Should().Be("EU"));
           //
           // var resourcePendingBillRateForM1Ams = allocationsWithBillRates.ToList().GetRange(7, 1);
           // resourcePendingBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * PendingBillRateForM1Ams.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * PendingBillRateForM1Ams.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateForM1Ams.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(PendingBillRateForM1Ams.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourcePendingBillRateForM1Ams.ToList().ForEach(x => x.BillRateCurrency.Should().Be("EU"));
           //
           // var resourcePendingBillRateForM1 = allocationsWithBillRates.ToList().GetRange(8, 1);
           // resourcePendingBillRateForM1.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * PendingBillRateForM1Ams.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourcePendingBillRateForM1.ToList().ForEach(x => x.EffectiveCost.Should().Be(0));
           // resourcePendingBillRateForM1.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(PendingBillRateForM1Ams.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           // resourcePendingBillRateForM1.ToList().ForEach(x => x.BillRateCurrency.Should().Be("EU"));
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        public async Task CreateAnalyticsReport_should_splitRows_per_multipleHistoricalPromotions(
           string resourcesAllocations)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockCCMAPIClient = new Mock<ICCMApiClient>();
            mockCCMAPIClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            var currentBillRateForA1 =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A1" && x.OfficeCode == "110");
            var currentBillRateForA2 =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A2" && x.OfficeCode == "110");
            var currentBillRateForA3 =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A3" && x.OfficeCode == "110");
            var currentBillRateForA4 =
                GetFakeBillRates().FirstOrDefault(x =>
                    x.Status == "Current" && x.LevelGrade == "A4" && x.OfficeCode == "110");

           // resourcesAllocations.FirstOrDefault().StartDate = new DateTime(2019, 01, 01);
           // resourcesAllocations.FirstOrDefault().EndDate = new DateTime(2020, 02, 16);
           // resourcesAllocations.FirstOrDefault().CurrentLevelGrade = "A1";
           // resourcesAllocations.FirstOrDefault().EmployeeCode = "36254";

            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            mockResourcesApiClient.Setup(x => x.GetPendingTransactionsByEmployeeCodes(It.IsAny<string>()))
                .ReturnsAsync(GetFakePendingTransactions);
            mockResourcesApiClient.Setup(x => x.GetEmployeesIncludingTerminated()).ReturnsAsync(GetFakeResources);
            mockResourcesApiClient.Setup(x => x.GetEmployeesStaffingTransactions(It.IsAny<string>()))
                .ReturnsAsync(GetFakeEmployeeHistoricalPromotions);

            var mockResourceAvailabilityRepository = new Mock<IResourceAvailabilityRepository>();
            var mockAnalyticsRepository = new Mock<IAnalyticsRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();
            mockBackgroundJob.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(Guid.NewGuid().ToString());

            var sut = new AnalyticsService(mockResourceAllocationRepo.Object,
                mockResourcesApiClient.Object, mockResourceAvailabilityRepository.Object, mockAnalyticsRepository.Object,
                null,null,null,null,null,null,mockBackgroundJob.Object,null);

            //Act
            var allocationsWithBillRates = await sut.CreateAnalyticsReport(resourcesAllocations);

            //Assert
           // allocationsWithBillRates.ToList().Count.Should().Be(15);
           // allocationsWithBillRates.ToList().ForEach(x => x.BillCode.Should().Be(1));
           // allocationsWithBillRates.ToList().ForEach(x => x.BillRateType.Should().Be("S"));
           // allocationsWithBillRates.ToList().ForEach(x => x.BillRateCurrency.Should().Be("US"));
           // allocationsWithBillRates.ToList().ForEach(x => x.EffectiveCostReason.Should().BeNull());
           //
           // var resourceCurrentBillRateForA1 = allocationsWithBillRates.ToList().GetRange(0, 3);
           // resourceCurrentBillRateForA1.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA1.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA1.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA1.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA1.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA1.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           //
           // var resourceCurrentBillRateForA2 = allocationsWithBillRates.ToList().GetRange(3, 6);
           // resourceCurrentBillRateForA2.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA2.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA2.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA2.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA2.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA2.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           //
           // var resourceCurrentBillRateForA3 = allocationsWithBillRates.ToList().GetRange(9, 5);
           // resourceCurrentBillRateForA3.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA3.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA3.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA3.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA3.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA3.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
           //
           // var resourceCurrentBillRateForA4 = allocationsWithBillRates.ToList().GetRange(14, 1);
           // resourceCurrentBillRateForA4.ToList()
           //     .ForEach(x =>
           //         x.ActualCost.Should().Be(x.Allocation * currentBillRateForA4.Rate /
           //                                  GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4.ToList()
           //     .ForEach(x =>
           //         x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA4.Rate /
           //                                     GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year) / 100));
           // resourceCurrentBillRateForA4.ToList()
           //     .ForEach(x =>
           //         x.BillRate.Should().Be(currentBillRateForA4.Rate /
           //                                GetWorkingDaysInMonth(x.StartDate.Month, x.StartDate.Year)));
        }

        private static int GetWorkingDaysInMonth(int month, int year)
        {
            var weekends = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var businessDaysInMonth = Enumerable.Range(1, daysInMonth)
                .Where(d => !weekends.Contains(new DateTime(year, month, d).DayOfWeek));

            return businessDaysInMonth.Count();
        }

        private IEnumerable<BillRate> GetFakeBillRates()
        {
            var fakeBillRates = new List<BillRate>
            {
                new BillRate
                {
                    Id = new Guid("30005420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "M9",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 70000,
                    Breakdown = "M",
                    StartDate = new DateTime(2018, 01, 01),
                    EndDate = new DateTime(2018, 12, 31),
                    Status = "Historical",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("30005420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "M9",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 70000,
                    Breakdown = "M",
                    StartDate = new DateTime(2019, 01, 01),
                    EndDate = new DateTime(2019, 09, 30),
                    Status = "Historical",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("ad025420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "M9",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 72000,
                    Breakdown = "M",
                    StartDate = new DateTime(2019, 10, 01),
                    EndDate = new DateTime(2020, 03, 31),
                    Status = "Current",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("ad025420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "M9",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 76000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 04, 01),
                    EndDate = null,
                    Status = "Pending",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("30005420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "265",
                    LevelGrade = "M1",
                    BillCode = 1,
                    Type = "S",
                    Currency = "EU",
                    Rate = 75000,
                    Breakdown = "M",
                    StartDate = new DateTime(2018, 01, 01),
                    EndDate = new DateTime(2019, 09, 30),
                    Status = "Historical",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("ad025420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "265",
                    LevelGrade = "M1",
                    BillCode = 1,
                    Type = "S",
                    Currency = "EU",
                    Rate = 79000,
                    Breakdown = "M",
                    StartDate = new DateTime(2019, 10, 01),
                    EndDate = new DateTime(2020, 03, 31),
                    Status = "Current",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("ad025420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "265",
                    LevelGrade = "M1",
                    BillCode = 1,
                    Type = "S",
                    Currency = "EU",
                    Rate = 86000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 04, 01),
                    EndDate = null,
                    Status = "Pending",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "A1",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 16000,
                    Breakdown = "M",
                    StartDate = new DateTime(2019, 01, 01),
                    EndDate = null,
                    Status = "Current",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "A2",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 46000,
                    Breakdown = "M",
                    StartDate = new DateTime(2019, 01, 01),
                    EndDate = null,
                    Status = "Current",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "A3",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 46000,
                    Breakdown = "M",
                    StartDate = new DateTime(2019, 09, 01),
                    EndDate = null,
                    Status = "Current",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "A7",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 56000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 01, 01),
                    EndDate = new DateTime(2020, 03, 31),
                    Status = "Current",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "A7",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 60000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 04, 01),
                    EndDate = new DateTime(2020, 04, 21),
                    Status = "Pending",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "A7",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 66000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 04, 22),
                    EndDate = null,
                    Status = "Pending",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "A4",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 56000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 01, 01),
                    EndDate = new DateTime(2020, 03, 31),
                    Status = "Current",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "A4",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 60000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 04, 01),
                    EndDate = new DateTime(2020, 04, 21),
                    Status = "Pending",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "110",
                    LevelGrade = "A4",
                    BillCode = 1,
                    Type = "S",
                    Currency = "US",
                    Rate = 66000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 04, 22),
                    EndDate = null,
                    Status = "Pending",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "265",
                    LevelGrade = "A4",
                    BillCode = 1,
                    Type = "S",
                    Currency = "EU",
                    Rate = 66000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 01, 01),
                    EndDate = new DateTime(2020, 03, 31),
                    Status = "Current",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "265",
                    LevelGrade = "A4",
                    BillCode = 1,
                    Type = "S",
                    Currency = "EU",
                    Rate = 80000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 04, 01),
                    EndDate = new DateTime(2020, 04, 21),
                    Status = "Pending",
                    LastUpdated = "2018-12-28T17:56:00"
                },
                new BillRate
                {
                    Id = new Guid("98045420-d4e8-e911-a996-005056acc703"),
                    OfficeCode = "265",
                    LevelGrade = "A4",
                    BillCode = 1,
                    Type = "S",
                    Currency = "EU",
                    Rate = 86000,
                    Breakdown = "M",
                    StartDate = new DateTime(2020, 04, 22),
                    EndDate = null,
                    Status = "Pending",
                    LastUpdated = "2018-12-28T17:56:00"
                }
            };
            return fakeBillRates;
        }

        private IEnumerable<ResourceTransaction> GetFakePendingPromotion()
        {
            var fakePromotions = new List<ResourceTransaction>
            {
                new ResourceTransaction
                {
                    EmployeeCode = "37810",
                    StartDate = new DateTime(2020, 03, 01),
                    EndDate = null,
                    LevelGrade = "M9",
                    BillCode = 1,
                    FTE = 1,
                    OperatingOffice = new Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "BOS"
                    },
                    Position = null,
                    Type = "Promotion"
                }
            };
            return fakePromotions;
        }

        private IEnumerable<ResourceTransaction> GetFakePendingTransfers()
        {
            var fakeTransfers = new List<ResourceTransaction>
            {
                new ResourceTransaction
                {
                    EmployeeCode = "38333",
                    StartDate = new DateTime(2020, 02, 17),
                    EndDate = null,
                    LevelGrade = "A4",
                    BillCode = 1,
                    FTE = 1,
                    OperatingOffice = new Office
                    {
                        OfficeCode = 265,
                        OfficeName = "Amsterdam",
                        OfficeAbbreviation = "AMS"
                    },
                    Position = null,
                    Type = "Transfer"
                }
            };
            return fakeTransfers;
        }

        private IEnumerable<ResourceTransaction> GetFakePendingTransitions()
        {
            var fakeTransfers = new List<ResourceTransaction>
            {
                new ResourceTransaction
                {
                    EmployeeCode = "36254",
                    StartDate = new DateTime(2020, 02, 03),
                    EndDate = null,
                    LevelGrade = "A4",
                    BillCode = 1,
                    FTE = 1,
                    OperatingOffice = new Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "Bos"
                    },
                    Position = null,
                    Type = "Transition"
                }
            };
            return fakeTransfers;
        }

        private IEnumerable<ResourceTransaction> GetFakePendingTransactions()
        {
            var fakeTransfers = new List<ResourceTransaction>
            {
                new ResourceTransaction
                {
                    EmployeeCode = "36254",
                    StartDate = new DateTime(2020, 03, 01),
                    EndDate = null,
                    LevelGrade = "M1",
                    BillCode = 1,
                    FTE = 1,
                    OperatingOffice = new Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "BOS"
                    },
                    Position = null,
                    Type = "Promotion"
                },
                new ResourceTransaction
                {
                    EmployeeCode = "36254",
                    StartDate = new DateTime(2020, 04, 03),
                    EndDate = new DateTime(2020, 05, 03),
                    LevelGrade = "A4",
                    BillCode = 1,
                    FTE = 1,
                    OperatingOffice = new Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "Bos"
                    },
                    Position = null,
                    Type = "Transition"
                },
                new ResourceTransaction
                {
                    EmployeeCode = "36254",
                    StartDate = new DateTime(2020, 02, 17),
                    EndDate = null,
                    LevelGrade = "A4",
                    BillCode = 1,
                    FTE = 1,
                    OperatingOffice = new Office
                    {
                        OfficeCode = 265,
                        OfficeName = "Amsterdam",
                        OfficeAbbreviation = "AMS"
                    },
                    Position = null,
                    Type = "Transfer"
                }
            };
            return fakeTransfers;
        }

        private IEnumerable<Resource> GetFakeResources()
        {
            var fixture = new Fixture();
            fixture.Customize<Resource>(r => r
                .With(x => x.EmployeeCode, "36254")
                .With(x => x.BillCode, 1)
                .With(x => x.LevelGrade, "A4")
                .With(x => x.Fte, 1)
                .With(x => x.OperatingOffice, new Office
                {
                    OfficeCode = 110,
                    OfficeName = "Boston",
                    OfficeAbbreviation = "BOS"
                }));

            var employees = fixture.CreateMany<Resource>(10).ToList();
            return employees;
        }

        private IEnumerable<EmployeeTransaction> GetFakeEmployeeTransactions()
        {
            var fixture = new Fixture();
            var fixtureTransaction = new Fixture();

            fixtureTransaction.Customize<EmployeeTransactionProcess>(e => e
                .With(x => x.PdGradeProposed, "A4")
                .With(x => x.PdGradeCurrent, "A3")
                .With(x => x.FteCurrent, 1)
                .With(x => x.FteProposed, 1)
                .With(x => x.BillCodeCurrent, 1)
                .With(x => x.BillCodeProposed, 1)
                .With(x => x.SchedulingOfficeCurrent,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" })
                .With(x => x.SchedulingOfficeProposed,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" })
                .Without(x => x.TransitionStartDate)
                .Without(x => x.TransitionEndDate));

            fixture.Customize<EmployeeTransaction>(e => e
                .With(x => x.BusinessProcessName, "Change Job")
                .With(x => x.TransactionStatus, "Successfully Completed")
                .With(x => x.EmployeeCode, "36254")
                .With(x => x.EffectiveDate, new DateTime(2020, 01, 01))
                .Without(x => x.TerminationEffectiveDate)
                .With(x => x.Transaction, fixtureTransaction.Create<EmployeeTransactionProcess>()));

            var employeeTransactions = fixture.CreateMany<EmployeeTransaction>(1).ToList();
            return employeeTransactions;
        }

        private IEnumerable<EmployeeTransaction> GetFakeEmployeeHistoricalPromotions()
        {
            var fixture = new Fixture();
            var fixtureTransaction = new Fixture();
            var employeeTransactions = new List<EmployeeTransaction>();

            // Promotion A2
            fixtureTransaction.Customize<EmployeeTransactionProcess>(e => e
                .With(x => x.PdGradeProposed, "A2")
                .With(x => x.PdGradeCurrent, "A1")
                .With(x => x.FteCurrent, 1)
                .With(x => x.FteProposed, 1)
                .With(x => x.BillCodeCurrent, 1)
                .With(x => x.BillCodeProposed, 1)
                .Without(x => x.TransitionStartDate)
                .Without(x => x.TransitionEndDate)
                .With(x => x.SchedulingOfficeCurrent,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" })
                .With(x => x.SchedulingOfficeProposed,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" }));

            fixture.Customize<EmployeeTransaction>(e => e
                .With(x => x.BusinessProcessName, "Change Job")
                .With(x => x.TransactionStatus, "Successfully Completed")
                .With(x => x.EmployeeCode, "36254")
                .With(x => x.EffectiveDate, new DateTime(2019, 04, 01))
                .Without(x => x.TerminationEffectiveDate)
                .With(x => x.Transaction, fixtureTransaction.Create<EmployeeTransactionProcess>()));

            employeeTransactions.Add(fixture.Create<EmployeeTransaction>());

            //Promotion A3
            fixtureTransaction.Customize<EmployeeTransactionProcess>(e => e
                .With(x => x.PdGradeProposed, "A3")
                .With(x => x.PdGradeCurrent, "A2")
                .With(x => x.FteCurrent, 1)
                .With(x => x.FteProposed, 1)
                .With(x => x.BillCodeCurrent, 1)
                .With(x => x.BillCodeProposed, 1)
                .Without(x => x.TransitionStartDate)
                .Without(x => x.TransitionEndDate)
                .With(x => x.SchedulingOfficeCurrent,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" })
                .With(x => x.SchedulingOfficeProposed,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" }));

            fixture.Customize<EmployeeTransaction>(e => e
                .With(x => x.BusinessProcessName, "Change Job")
                .With(x => x.TransactionStatus, "Successfully Completed")
                .With(x => x.EmployeeCode, "36254")
                .With(x => x.EffectiveDate, new DateTime(2019, 10, 01))
                .Without(x => x.TerminationEffectiveDate)
                .With(x => x.Transaction, fixtureTransaction.Create<EmployeeTransactionProcess>()));

            employeeTransactions.Add(fixture.Create<EmployeeTransaction>());

            //Promotion A4
            fixtureTransaction.Customize<EmployeeTransactionProcess>(e => e
                .With(x => x.PdGradeProposed, "A4")
                .With(x => x.PdGradeCurrent, "A3")
                .With(x => x.FteCurrent, 1)
                .With(x => x.FteProposed, 1)
                .With(x => x.BillCodeCurrent, 1)
                .With(x => x.BillCodeProposed, 1)
                .Without(x => x.TransitionStartDate)
                .Without(x => x.TransitionEndDate)
                .With(x => x.SchedulingOfficeCurrent,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" })
                .With(x => x.SchedulingOfficeProposed,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" }));

            fixture.Customize<EmployeeTransaction>(e => e
                .With(x => x.BusinessProcessName, "Change Job")
                .With(x => x.TransactionStatus, "Successfully Completed")
                .With(x => x.EmployeeCode, "36254")
                .With(x => x.EffectiveDate, new DateTime(2020, 02, 03))
                .Without(x => x.TerminationEffectiveDate)
                .With(x => x.Transaction, fixtureTransaction.Create<EmployeeTransactionProcess>()));

            employeeTransactions.Add(fixture.Create<EmployeeTransaction>());

            return employeeTransactions;
        }
    }

    public class ResourceAllocationUpdateTestDataGenerator : IEnumerable<object[]>
    {
        private readonly Guid _dummyGuid = Guid.NewGuid();

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new[]
                {
                    new ResourceAllocation
                    {
                        Id = _dummyGuid,
                        OldCaseCode = "H2ES",
                        CaseCode = 102,
                        CaseName = "Project Atlas",
                        ClientCode = 7471,
                        ClientName = "Goldman Sachs",
                        CaseTypeCode = 1,
                        CaseTypeName = "Billable",
                        PipelineId = null,
                        OpportunityName = null,
                        EmployeeCode = "36254",
                        EmployeeName = "Gupta, Praneet",
                        Fte = 0.8M,
                        ServiceLineName = "Traditional Consultant",
                        Position = "Manager",
                        CurrentLevelGrade = "M9",
                        BillCode = 1,
                        OperatingOfficeCode = 110,
                        OperatingOfficeAbbreviation = "BOS",
                        OperatingOfficeName = "Boston",
                        ManagingOfficeCode = 110,
                        ManagingOfficeAbbreviation = "BOS",
                        ManagingOfficeName = "Boston",
                        BillingOfficeCode = 110,
                        BillingOfficeAbbreviation = "BOS",
                        BillingOfficeName = "Boston",
                        Allocation = 80,
                        StartDate = new DateTime(2019, 12, 02),
                        EndDate = new DateTime(2020, 01, 11),
                        InvestmentCode = null,
                        InvestmentName = null,
                        CaseRoleCode = null,
                        CaseRoleName = null,
                        LastUpdatedBy = "39209"
                    }
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}