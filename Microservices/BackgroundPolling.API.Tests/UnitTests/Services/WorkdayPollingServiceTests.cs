using AutoFixture;
using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Services;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.Models.Workday;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BackgroundPolling.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "CCM.API.Services")]
    public class WorkdayPollingServiceTests
    {
        [Fact]
        public async Task UpdateCostForPendingPromotionsTransitionsAndTransfers_should_return_pendingTransactionsWithBillRate()
        {
            //Arrange
            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            mockResourcesApiClient.Setup(x => x.GetFutureTransitions()).ReturnsAsync(GetFakePendingTransitions);
            mockResourcesApiClient.Setup(x => x.GetFuturePromotions()).ReturnsAsync(GetFakePendingPromotion);
            mockResourcesApiClient.Setup(x => x.GetFutureTransfers()).ReturnsAsync(GetFakePendingTransfers);

            var mockWorkdayPollingRepo = new Mock<IWorkdayPollingRepository>();

            var mockCCMApiClient = new Mock<ICcmApiClient>();
            mockCCMApiClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
                .ReturnsAsync(GetFakeBillRates);

            var mockWorkdayRedisConnectorAPIClient = new Mock<IWorkdayRedisConnectorAPIClient>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            mockBackgroundJobClient.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(Guid.NewGuid().ToString());

            var mockPollMasterRepostory = new Mock<IPollMasterRepository>();
            var mockMemoryCache = new Mock<IMemoryCache>();

            var sut = new WorkdayPollingService(mockResourcesApiClient.Object,mockWorkdayPollingRepo.Object,mockMemoryCache.Object,
                mockCCMApiClient.Object,mockWorkdayRedisConnectorAPIClient.Object,null,
                mockBackgroundJobClient.Object,mockPollMasterRepostory.Object,null);

            //Act
            var pendingTransactions = await sut.UpdateAnalyitcsDataForPendingTransactions(null);

            //Assert
            pendingTransactions.ToList().Count.Should().Be(3);
            pendingTransactions.ToList().ForEach(x => x.BillCode.Should().Be(1));
            pendingTransactions.ToList().ForEach(x => x.CurrentLevelGrade.Should().NotBeNullOrEmpty());
            pendingTransactions.ToList().ForEach(x => x.EffectiveDate.Should().BeAfter(DateTime.MinValue));
            pendingTransactions.ToList().ForEach(x => x.EmployeeCode.Should().BeOneOf("36254", "37810", "38333"));
            pendingTransactions.ToList().ForEach(x => x.EndDate?.Should().BeOnOrAfter(x.EffectiveDate));
            pendingTransactions.ToList().ForEach(x => x.FTE.Should().BeInRange(0, 1));
            pendingTransactions.ToList().ForEach(x => x.LastUpdatedBy.ToUpper().Should().Be("AUTO-PENDINGTRANS"));
            pendingTransactions.ToList().ForEach(x =>
            {
                if (x.EmployeeCode == "36254")
                {
                    x.EffectiveCostReason.ToUpper().Should().Be("TRANSITION");
                    x.OperatingOfficeAbbreviation.ToUpper().Should().Be("BOS");
                    x.OperatingOfficeCode.Should().Be(110);
                    x.OperatingOfficeName.ToUpper().Should().Be("BOSTON");
                    x.Position.ToUpper().Should().Be("SENIOR ASSOC. CONSULTANT");
                }
                else if (x.EmployeeCode == "37810")
                {
                    x.EffectiveCostReason.Should().BeNullOrEmpty();
                    x.OperatingOfficeAbbreviation.ToUpper().Should().Be("BOS");
                    x.OperatingOfficeCode.Should().Be(110);
                    x.OperatingOfficeName.ToUpper().Should().Be("BOSTON");
                    x.Position.ToUpper().Should().Be("MANAGER");
                }
                else if (x.EmployeeCode == "38333")
                {
                    x.EffectiveCostReason.Should().BeNullOrEmpty();
                    x.OperatingOfficeAbbreviation.ToUpper().Should().Be("AMS");
                    x.OperatingOfficeCode.Should().Be(265);
                    x.OperatingOfficeName.ToUpper().Should().Be("AMSTERDAM");
                    x.Position.ToUpper().Should().Be("SENIOR ASSOC. CONSULTANT");
                }
            });
        }

        [Fact]
        public async Task UpdateCostForAnalytics_should_return_recordsWithNullCost()
        {
            //Arrange
            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            mockResourcesApiClient.Setup(x => x.GetFutureLOAs()).ReturnsAsync(GetFakeFutureLOAs);
            mockResourcesApiClient.Setup(x => x.GetEmployeesIncludingTerminated()).ReturnsAsync(GetFakeResourcesIncludedTerminated);
            mockResourcesApiClient.Setup(x =>
            x.GetLOAsWithinDateRangeByEmployeeCodes(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(GetFakeLOAs);
            mockResourcesApiClient.Setup(x => x.GetEmployeesStaffingTransactions(It.IsAny<string>()))
                .ReturnsAsync(GetFakeEmployeeHistoricalPromotions);

            var mockWorkdayPollingRepo = new Mock<IWorkdayPollingRepository>();
            mockWorkdayPollingRepo.Setup(x => x.GetRecordsWithoutCost()).ReturnsAsync(GetFakeRecordsWithoutCost);

            var mockCCMApiClient = new Mock<ICcmApiClient>();
            mockCCMApiClient.Setup(x => x.GetBillRateByOffices(It.IsAny<string>()))
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

            var mockWorkdayRedisConnectorAPIClient = new Mock<IWorkdayRedisConnectorAPIClient>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            var mockPollMasterRepostory = new Mock<IPollMasterRepository>();

            var mockMemoryCache = new Mock<IMemoryCache>();

            var sut = new WorkdayPollingService(mockResourcesApiClient.Object, mockWorkdayPollingRepo.Object, mockMemoryCache.Object,
                mockCCMApiClient.Object, mockWorkdayRedisConnectorAPIClient.Object, null,
                mockBackgroundJobClient.Object, mockPollMasterRepostory.Object, null);

            //Act
            var recordsWithoutCost = await sut.UpdateCostForAnalytics();

            //Assert
            recordsWithoutCost.ToList().Count.Should().Be(4);
            recordsWithoutCost.ToList().ForEach(x => x.Id.Should().NotBeEmpty());
            recordsWithoutCost.ToList().ForEach(x => x.EmployeeCode.Should().NotBeNullOrEmpty());
            recordsWithoutCost.ToList().ForEach(x => x.Fte.Should().Be(Convert.ToDecimal(1.00)));
            recordsWithoutCost.ToList().ForEach(x => x.OperatingOfficeCode.Should().Be(110));
            recordsWithoutCost.ToList().ForEach(x => x.CurrentLevelGrade.Should().BeOneOf("A1", "A2", "A3", "A4"));
            recordsWithoutCost.ToList().ForEach(x => x.Allocation.Should().BeInRange(0, 999));
            recordsWithoutCost.ToList().ForEach(x => x.Date.Should().BeAfter(DateTime.MinValue));
            recordsWithoutCost.ToList().ForEach(x => x.Position.Should().Be("Associate Consultant"));
            recordsWithoutCost.ToList().ForEach(x => x.BillCode.Should().Be(Convert.ToDecimal(1.00)));

            var resourceCurrentBillRateForA1 = recordsWithoutCost.ToList().GetRange(0, 1);
            resourceCurrentBillRateForA1.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * currentBillRateForA1.Rate /
                                             GetWorkingDaysInMonth(x.Date.Month, x.Date.Year) / 100));
            resourceCurrentBillRateForA1.ToList()
                .ForEach(x =>
                    x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA1.Rate /
                                                GetWorkingDaysInMonth(x.Date.Month, x.Date.Year) / 100));
            resourceCurrentBillRateForA1.ToList()
                .ForEach(x =>
                    x.BillRate.Should().Be(currentBillRateForA1.Rate /
                                           GetWorkingDaysInMonth(x.Date.Month, x.Date.Year)));

            var resourceCurrentBillRateForA2 = recordsWithoutCost.ToList().GetRange(1, 1);
            resourceCurrentBillRateForA2.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * currentBillRateForA2.Rate /
                                             GetWorkingDaysInMonth(x.Date.Month, x.Date.Year) / 100));
            resourceCurrentBillRateForA2.ToList()
                .ForEach(x =>
                    x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA2.Rate /
                                                GetWorkingDaysInMonth(x.Date.Month, x.Date.Year) / 100));
            resourceCurrentBillRateForA2.ToList()
                .ForEach(x =>
                    x.BillRate.Should().Be(currentBillRateForA2.Rate /
                                           GetWorkingDaysInMonth(x.Date.Month, x.Date.Year)));

            var resourceCurrentBillRateForA3 = recordsWithoutCost.ToList().GetRange(2, 1);
            resourceCurrentBillRateForA3.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * currentBillRateForA3.Rate /
                                             GetWorkingDaysInMonth(x.Date.Month, x.Date.Year) / 100));
            resourceCurrentBillRateForA3.ToList()
                .ForEach(x =>
                    x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA3.Rate /
                                                GetWorkingDaysInMonth(x.Date.Month, x.Date.Year) / 100));
            resourceCurrentBillRateForA3.ToList()
                .ForEach(x =>
                    x.BillRate.Should().Be(currentBillRateForA3.Rate /
                                           GetWorkingDaysInMonth(x.Date.Month, x.Date.Year)));

            var resourceCurrentBillRateForA4 = recordsWithoutCost.ToList().GetRange(3, 1);
            resourceCurrentBillRateForA4.ToList()
                .ForEach(x =>
                    x.ActualCost.Should().Be(x.Allocation * currentBillRateForA4.Rate /
                                             GetWorkingDaysInMonth(x.Date.Month, x.Date.Year) / 100));
            resourceCurrentBillRateForA4.ToList()
                .ForEach(x =>
                    x.EffectiveCost.Should().Be(x.Allocation * currentBillRateForA4.Rate /
                                                GetWorkingDaysInMonth(x.Date.Month, x.Date.Year) / 100));
            resourceCurrentBillRateForA4.ToList()
                .ForEach(x =>
                    x.BillRate.Should().Be(currentBillRateForA4.Rate /
                                           GetWorkingDaysInMonth(x.Date.Month, x.Date.Year)));
        }

        [Fact]
        public async Task UpdateAnalyticsDataForDeletedTransactions_should_return_deletedTransactions()
        {
            //Arrange
            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            var mockWorkdayPollingRepo = new Mock<IWorkdayPollingRepository>();
            var mockCCMApiClient = new Mock<ICcmApiClient>();

            var mockWorkdayRedisConnectorAPIClient = new Mock<IWorkdayRedisConnectorAPIClient>();
            mockWorkdayRedisConnectorAPIClient.Setup(x =>
            x.GetEmployeesLOATransactionsByModifiedDate(It.IsAny<DateTime>().Date))
                .ReturnsAsync(GetFakeEmployeesLOATransactionsModifiedRecently);

            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            var mockPollMasterRepostory = new Mock<IPollMasterRepository>();
            var mockMemoryCache = new Mock<IMemoryCache>();

            var sut = new WorkdayPollingService(mockResourcesApiClient.Object, mockWorkdayPollingRepo.Object, mockMemoryCache.Object,
                mockCCMApiClient.Object, mockWorkdayRedisConnectorAPIClient.Object, null,
                mockBackgroundJobClient.Object, mockPollMasterRepostory.Object, null);

            //Act
            var deletedTransactions = await sut.UpdateAnalyticsDataForLoAUpdatedRecently(null, null);

            //Assert
            //deletedTransactions.ToList().ForEach(x =>
            //x.BusinessProcessEvent.Should()
            //.Be("Leave Return for Meryem Benamour (06MER) last day of leave on 10/03/2019, first day back at work on 10/04/2019"));
            //deletedTransactions.ToList().ForEach(x => x.BusinessProcessType.Should().Be("Request Return from Leave of Absence"));
            //deletedTransactions.ToList().ForEach(x => x.CompletedDate.Should().Be(new DateTime(2020, 10, 07)));
            //deletedTransactions.ToList().ForEach(x => x.EffectiveDate.Should().Be(new DateTime(2020, 10, 03)));
            //deletedTransactions.ToList().ForEach(x => x.LastModifiedDate.Should().Be(new DateTime(2020, 10, 03)));
            //deletedTransactions.ToList().ForEach(x => x.EmployeeStatus.Should().Be("Active"));
            //deletedTransactions.ToList().ForEach(x => x.EmployeeCode.Should().Be("06MER"));
            //deletedTransactions.ToList().ForEach(x => x.EmployeeName.Should().Be("Meryem Benamour (06MER)"));
            //deletedTransactions.ToList().ForEach(x => x.TransactionStatus.Should().Be("Rescinded"));
        }
        [Fact]
        public async Task SaveWorkdayLoaAsShortTermCommitment_should_return_shortTermCommitments()
        {
            //Arrange
            var mockResourcesApiClient = new Mock<IResourceApiClient>();
            var mockWorkdayPollingRepo = new Mock<IWorkdayPollingRepository>();
            var mockCCMApiClient = new Mock<ICcmApiClient>();

            var mockWorkdayRedisConnectorAPIClient = new Mock<IWorkdayRedisConnectorAPIClient>();
            mockWorkdayRedisConnectorAPIClient.Setup(x =>
            x.GetEmployeesLOATransactionsPendingFromRedis())
                .ReturnsAsync(GetFakeEmployeesLOATransactionsPending);

            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            var mockPollMasterRepostory = new Mock<IPollMasterRepository>();

            var mockMemoryCache = new Mock<IMemoryCache>();

            var sut = new WorkdayPollingService(mockResourcesApiClient.Object, mockWorkdayPollingRepo.Object, mockMemoryCache.Object,
                mockCCMApiClient.Object, mockWorkdayRedisConnectorAPIClient.Object, null,
                mockBackgroundJobClient.Object, mockPollMasterRepostory.Object, null);

            //Act
            var shortTermCommitments = await sut.SaveWorkdayLoaAndTransitionAsShortTermCommitment();

            //Assert
            if (shortTermCommitments.Any())
            {
                shortTermCommitments.ToList().Count.Should().Be(3);
                shortTermCommitments.ToList().ForEach(x => x.Id.Should().NotBeEmpty());
                shortTermCommitments.ToList().ForEach(x => x.EmployeeCode.Should().BeOneOf("46033", "35783", "03AKN"));
                shortTermCommitments.ToList().ForEach(x => x.CommitmentType.CommitmentTypeCode.Should().Be("ST"));
                shortTermCommitments.ToList().ForEach(x => x.EndDate.Should().NotBeBefore(x.StartDate));
                

                //shortTermCommitments.ToList().ForEach(x =>
                //{
                //    if (x.Id == Guid.Parse("f7bb5e21d6de0178262747749401997d"))
                //    {
                //        x.StartDate.Should().Be(DateTime.Now.AddDays(3).Date);
                //        x.EndDate.Should().Be(DateTime.Now.AddDays(24).Date);
                //    }
                //    else if (x.Id == Guid.Parse("792349a4d3e80154db5493c148302138"))
                //    {
                //        x.StartDate.Should().Be(DateTime.Now.Date);
                //        x.EndDate.Should().Be(DateTime.Now.AddDays(10).Date);
                //    }
                //    else if (x.Id == Guid.Parse("deda479fc316013695e2b7ddee002b3a"))
                //    {
                //        x.StartDate.Should().Be(DateTime.Now.AddDays(9).Date);
                //        x.EndDate.Should().Be(DateTime.Now.AddDays(30).Date);
                //    }

                //});

            }
        }

        //[Fact]
        //public async Task UpdateAvailabilityDataForEmployeesWithNoAllocations_should_return_deletedTransactions()
        //{
        //    //Arrange
        //    var mockResourcesApiClient = new Mock<IResourceApiClient>();
        //    mockResourcesApiClient.Setup(x => x.GetEmployees()).ReturnsAsync(GetFakeResourcesIncludedTerminated);

        //    var mockWorkdayPollingRepo = new Mock<IWorkdayPollingRepository>();
        //    var mockCCMApiClient = new Mock<ICcmApiClient>();
        //    var mockWorkdayRedisConnectorAPIClient = new Mock<IWorkdayRedisConnectorAPIClient>();

        //    var mockStaffingApiClient = new Mock<IStaffingApiClient>();
        //    mockStaffingApiClient.Setup(x => x.getEmployeesWithNoAllocations(It.IsAny<string>()))
        //        .ReturnsAsync(GetFakeEmployeeCodesWithNoAllocations);

        //    var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();

        //    var sut = new WorkdayPollingService(mockResourcesApiClient.Object, mockWorkdayPollingRepo.Object, mockCCMApiClient.Object,
        //        mockWorkdayRedisConnectorAPIClient.Object, mockStaffingApiClient.Object, mockBackgroundJobClient.Object);

        //    //Act
        //    var employeesWithNoAllocations = await sut.UpdateAvailabilityDataForEmployeesWithNoAllocations();

        //    //Assert
        //    employeesWithNoAllocations.ToList().Count.Should().Be(5);
        //    employeesWithNoAllocations.ToList().ForEach(x => x.EmployeeCode.Should().BeOneOf("43381,42536,38435,44281,36254".Split(",")));
        //    employeesWithNoAllocations.ToList().ForEach(x => x.EmployeeName.Should().NotBeNullOrEmpty());
        //    employeesWithNoAllocations.ToList().ForEach(x => x.Fte.Should().BeInRange(0,1));
        //    employeesWithNoAllocations.ToList().ForEach(x => x.OperatingOfficeCode.Should().NotBe(0));
        //    employeesWithNoAllocations.ToList().ForEach(x => x.OperatingOfficeName.Should().NotBeNullOrEmpty());
        //    employeesWithNoAllocations.ToList().ForEach(x => x.OperatingOfficeAbbreviation.Should().NotBeNullOrEmpty());
        //    employeesWithNoAllocations.ToList().ForEach(x => x.CurrentLevelGrade.Should().NotBeNullOrEmpty());
        //    employeesWithNoAllocations.ToList().ForEach(x => x.ServiceLineName.Should().NotBeNullOrEmpty());
        //    employeesWithNoAllocations.ToList().ForEach(x => x.Position.Should().NotBeNullOrEmpty());
        //    employeesWithNoAllocations.ToList().ForEach(x => x.BillCode.Should().NotBe(0));
        //    employeesWithNoAllocations.ToList().ForEach(x => x.StartDate.Should().BeOnOrAfter(Convert.ToDateTime("01-01-" + DateTime.Today.Year)));
        //    employeesWithNoAllocations.ToList().ForEach(x => x.EndDate.Should().Be(DateTime.Today.AddDays(30).Date));
        //    employeesWithNoAllocations.ToList().ForEach(x => x.Availability.Should().Be(Convert.ToInt32(x.Fte * 100)));
        //    employeesWithNoAllocations.ToList().ForEach(x => x.EffectiveAvailability.Should().Be(Convert.ToInt32(x.Fte * 100)));
        //    employeesWithNoAllocations.ToList().ForEach(x => x.LastUpdatedBy.Should().Be("PollingAPI ResWithNoAlloc"));
        //}

        #region Private Methods

        private IEnumerable<string> GetFakeEmployeeCodesWithNoAllocations()
        {
            var employeesCodes = "43381,42536,38435,44281,36254".Split(",");
            return employeesCodes;
        }

        private IEnumerable<ScheduleMasterDetail> GetFakeRecordsWithoutCost()
        {
            var fakeRecords = new List<ScheduleMasterDetail>
            {
                new ScheduleMasterDetail
                {
                    Id=new Guid("7D742623-7942-EA11-A99D-A2EDE821CAF8"),
                    EmployeeCode = "43381",
                    Fte=Convert.ToDecimal(1.00),
                    OperatingOfficeCode=110,
                    CurrentLevelGrade="A1",
                    Allocation=100,
                    Date=new DateTime(2022, 12, 30),
                    InvestmentCode=null,
                    CaseRoleCode=null,
                    ServiceLineName=null,
                    Position="Associate Consultant",
                    BillCode=Convert.ToDecimal(1.00),
                    BillRate=null,
                    BillRateType ="S",
                    BillRateCurrency="US",
                    ActualCost=null,
                    EffectiveCost=null,
                    EffectiveCostReason=null,
                    OperatingOfficeAbbreviation="BOS",
                    OperatingOfficeName="Boston",
                    TransactionType=null,
                },
                new ScheduleMasterDetail
                {
                    Id=new Guid("E2FA54BD-7842-EA11-A99D-A2EDE821CAF8"),
                    EmployeeCode = "42536",
                    Fte=Convert.ToDecimal(1.00),
                    OperatingOfficeCode=110,
                    CurrentLevelGrade="A2",
                    Allocation=100,
                    Date=new DateTime(2020, 07, 23),
                    InvestmentCode=null,
                    CaseRoleCode=null,
                    ServiceLineName=null,
                    Position="Associate Consultant",
                    BillCode=Convert.ToDecimal(1.00),
                    BillRate=null,
                    BillRateType="S",
                    BillRateCurrency="EU",
                    ActualCost=null,
                    EffectiveCost=null,
                    EffectiveCostReason=null,
                    OperatingOfficeAbbreviation="BOS",
                    OperatingOfficeName="Boston",
                    TransactionType=null,
                },
                new ScheduleMasterDetail
                {
                    Id=new Guid("E91C74EF-7842-EA11-A99D-A2EDE821CAF8"),
                    EmployeeCode = "38435",
                    Fte=Convert.ToDecimal(1.00),
                    OperatingOfficeCode=110,
                    CurrentLevelGrade="A3",
                    Allocation=0,
                    Date=new DateTime(2020, 12, 31),
                    InvestmentCode=null,
                    CaseRoleCode=null,
                    ServiceLineName=null,
                    Position="Associate Consultant",
                    BillCode=Convert.ToDecimal(1.00),
                    BillRate=null,
                    BillRateType="S",
                    BillRateCurrency="US",
                    ActualCost=null,
                    EffectiveCost=null,
                    EffectiveCostReason=null,
                    OperatingOfficeAbbreviation="BOS",
                    OperatingOfficeName="Boston",
                    TransactionType=null,
                },
                new ScheduleMasterDetail
                {
                    Id=new Guid("55D5B93B-0B48-EA11-A99D-A2EDE821CAF8"),
                    EmployeeCode = "44281",
                    Fte=Convert.ToDecimal(1.00),
                    OperatingOfficeCode=110,
                    CurrentLevelGrade="A4",
                    Allocation=80,
                    Date=new DateTime(2020, 03, 30),
                    InvestmentCode=null,
                    CaseRoleCode=null,
                    ServiceLineName="Traditional Consultant",
                    Position="Associate Consultant",
                    BillCode=Convert.ToDecimal(1.00),
                    BillRate=null,
                    BillRateType="S",
                    BillRateCurrency="US",
                    ActualCost=null,
                    EffectiveCost=null,
                    EffectiveCostReason=null,
                    OperatingOfficeAbbreviation="BOS",
                    OperatingOfficeName="Boston",
                    TransactionType=null,
                }
            };
            return fakeRecords;
        }

        private IEnumerable<ResourceTransaction> GetFakePendingTransitions()
        {
            var fakeTransfers = new List<ResourceTransaction>
            {
                new ResourceTransaction
                {
                    EmployeeCode = "36254",
                    StartDate = DateTime.Now.AddMonths(2),
                    EndDate = null,
                    LevelGrade = "A4",
                    BillCode = 1,
                    FTE = 1,
                    OperatingOffice = new Models.Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "Bos"
                    },
                    Position = new Position
                    {
                        PositionCode = "1010101",
                        PositionName = "Senior Assoc. Consultant",
                        PositionGroupName = "Senior Assoc. Consultant"
                    },
                    LastUpdated = DateTime.Now,
                    Type = "Transition"
                }
            };
            return fakeTransfers;
        }

        private IEnumerable<ResourceTransaction> GetFakePendingPromotion()
        {
            var fakePromotions = new List<ResourceTransaction>
            {
                new ResourceTransaction
                {
                    EmployeeCode = "37810",
                    StartDate = DateTime.Now.AddMonths(1),
                    EndDate = null,
                    LevelGrade = "M9",
                    BillCode = 1,
                    FTE = 1,
                    OperatingOffice = new Models.Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "BOS"
                    },
                    Position = new Position
                    {
                        PositionCode = "1010101",
                        PositionName = "Manager",
                        PositionGroupName = "Manager"
                    },
                    LastUpdated = DateTime.Now,
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
                    StartDate = DateTime.Now.AddDays(100),
                    EndDate = null,
                    LevelGrade = "A4",
                    BillCode = 1,
                    FTE = 1,
                    OperatingOffice = new Models.Office
                    {
                        OfficeCode = 265,
                        OfficeName = "Amsterdam",
                        OfficeAbbreviation = "AMS"
                    },
                    Position = new Position
                    {
                        PositionCode = "1010101",
                        PositionName = "Senior Assoc. Consultant",
                        PositionGroupName = "Senior Assoc. Consultant"
                    },
                    LastUpdated = DateTime.Now,
                    Type = "Transfer"
                }
            };
            return fakeTransfers;
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

        private IEnumerable<ResourceLOA> GetFakeLOAs()
        {
            var fakeLOAs = new List<ResourceLOA>
            {
                new ResourceLOA
                {
                    EmployeeCode = "43381",
                    StartDate = new DateTime(2018, 07, 10),
                    EndDate = new DateTime(2020, 09, 01),
                    Description = "Unpaid Educational",
                    Status = null,
                    Type = "LOA"
                }
            };
            return fakeLOAs;
        }

        private IEnumerable<ResourceLOA> GetFakeFutureLOAs()
        {
            var fakeLOAs = new List<ResourceLOA>
            {
                new ResourceLOA
                {
                    EmployeeCode = "01ABX",
                    StartDate = new DateTime(2020, 03, 12),
                    EndDate = new DateTime(2020, 04, 10),
                    Description = "Unpaid Parental",
                    Status = null,
                    Type = "LOA"
                },
                new ResourceLOA
                {
                    EmployeeCode = "02LEY",
                    StartDate = new DateTime(2020, 04, 06),
                    EndDate = new DateTime(2020, 04, 27),
                    Description = "Unpaid Personal",
                    Status = null,
                    Type = "LOA"
                },
                new ResourceLOA
                {
                    EmployeeCode = "02JPS",
                    StartDate = new DateTime(2020, 04, 16),
                    EndDate = new DateTime(2020, 06, 14),
                    Description = "Unpaid Educational",
                    Status = null,
                    Type = "LOA"
                },
                new ResourceLOA
                {
                    EmployeeCode = "01RBY",
                    StartDate = new DateTime(2018, 12, 05),
                    EndDate = new DateTime(2020, 04, 10),
                    Description = "Unpaid Medical",
                    Status = null,
                    Type = "LOA"
                },
                new ResourceLOA
                {
                    EmployeeCode = "03CRI",
                    StartDate = new DateTime(2020, 01, 22),
                    EndDate = new DateTime(2020, 03, 31),
                    Description = "Paid Parental",
                    Status = null,
                    Type = "LOA"
                },
                new ResourceLOA
                {
                    EmployeeCode = "03HTA",
                    StartDate = new DateTime(2021, 03, 22),
                    EndDate = new DateTime(2021, 06, 20),
                    Description = "Unpaid Parental",
                    Status = null,
                    Type = "LOA"
                },
                new ResourceLOA
                {
                    EmployeeCode = "56AKR",
                    StartDate = new DateTime(2019, 09, 08),
                    EndDate = new DateTime(2020, 09, 05),
                    Description = "Paid Maternity",
                    Status = null,
                    Type = "LOA"
                }
            };
            return fakeLOAs;
        }

        //private List<Resource> GetFakeActiveResources()
        //{
        //    var fixture = new Fixture();
        //    fixture.Customize<Resource>(r => r
        //        .With(x => x.EmployeeCode, "36254")
        //        .With(x => x.BillCode, 1)
        //        .With(x => x.LevelGrade, "A4")
        //        .With(x => x.Fte, 1)
        //        .With(x => x.OperatingOffice, new Models.Office
        //        {
        //            OfficeCode = 110,
        //            OfficeName = "Boston",
        //            OfficeAbbreviation = "BOS"
        //        }));

        //    var employees = fixture.CreateMany<Resource>(10).ToList();
        //    return employees;
        //}

        private List<Resource> GetFakeResourcesIncludedTerminated()
        {
            var fixture = new Fixture();
            fixture.Customize<Resource>(r => r
                .With(x => x.EmployeeCode, "36254")
                .With(x => x.BillCode, 1)
                .With(x => x.LevelGrade, "A4")
                .With(x => x.Fte, 1)
                .With(x => x.SchedulingOffice, new Models.Office
                {
                    OfficeCode = 110,
                    OfficeName = "Boston",
                    OfficeAbbreviation = "BOS"
                }));

            var employees = fixture.CreateMany<Resource>(10).ToList();
            return employees;
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
                .With(x => x.SchedulingOfficeCurrent,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" })
                .With(x => x.SchedulingOfficeProposed,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" }));

            fixture.Customize<EmployeeTransaction>(e => e
                .With(x => x.BusinessProcessType, "Promote Employee")
                .With(x => x.TransactionStatus, "Successfully Completed")
                .With(x => x.EmployeeCode, "36254")
                .With(x => x.EffectiveDate, new DateTime(2019, 04, 01))
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
                .With(x => x.SchedulingOfficeCurrent,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" })
                .With(x => x.SchedulingOfficeProposed,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" }));

            fixture.Customize<EmployeeTransaction>(e => e
                .With(x => x.BusinessProcessType, "Promote Employee")
                .With(x => x.TransactionStatus, "Successfully Completed")
                .With(x => x.EmployeeCode, "36254")
                .With(x => x.EffectiveDate, new DateTime(2019, 10, 01))
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
                .With(x => x.SchedulingOfficeCurrent,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" })
                .With(x => x.SchedulingOfficeProposed,
                    new Models.Workday.Office { OfficeCode = "110", OfficeName = "Boston", OfficeAbbreviation = "BOS" }));

            fixture.Customize<EmployeeTransaction>(e => e
                .With(x => x.BusinessProcessType, "Promote Employee")
                .With(x => x.TransactionStatus, "Successfully Completed")
                .With(x => x.EmployeeCode, "36254")
                .With(x => x.EffectiveDate, new DateTime(2020, 02, 03))
                .With(x => x.Transaction, fixtureTransaction.Create<EmployeeTransactionProcess>()));

            employeeTransactions.Add(fixture.Create<EmployeeTransaction>());

            return employeeTransactions;
        }

        private IEnumerable<LOATransaction> GetFakeEmployeesLOATransactionsModifiedRecently()
        {
            var fakeLOATransaction = new List<LOATransaction>
            {
                new LOATransaction
                {
                    BusinessProcessEvent = "Leave Return for Akito Miyakawa （宮川 亜希人 - ミヤカワ アキト） (37497) last day of leave on 12/31/2020",
                    BusinessProcessReason = null,
                    BusinessProcessType = "Request Return from Leave of Absence",
                    CompletedDate = new DateTime(2020, 12, 24),
                    EffectiveDate = new DateTime(2020, 12, 31),
                    LastModifiedDate = new DateTime(2020, 12, 19),
                    EmployeeStatus = "On Leave",
                    MostRecentCorrectionDate = null,
                    TerminationStatusEffectiveDate = null,
                    EmployeeCode = "37497",
                    EmployeeName = "Akito Miyakawa （宮川 亜希人 - ミヤカワ アキト） (37497)",
                    TransactionStatus = "Successfully Completed",
                    Transaction = null
                },
                new LOATransaction
                {
                    BusinessProcessEvent = "Leave Request: Alexandra Duffy (42734)",
                    BusinessProcessReason = "Parental > Paid Parental",
                    BusinessProcessType = "Request Leave of Absence",
                    CompletedDate = new DateTime(2020, 09, 04),
                    EffectiveDate = new DateTime(2020, 09, 09),
                    LastModifiedDate = new DateTime(2020, 09, 04),
                    EmployeeStatus = "On Leave",
                    MostRecentCorrectionDate = null,
                    TerminationStatusEffectiveDate = null,
                    EmployeeCode = "42734",
                    EmployeeName = "Alexandra Duffy (42734)",
                    TransactionStatus = "Successfully Completed",
                    Transaction = new LOATransactionProcess
                    {
                     LastDayOfWork = new DateTime(2020, 09, 06),
                     FirstDayOfLeave = new DateTime(2020, 09, 09),
                     EstimatedLastDayOfLeave = new DateTime(2020, 09, 15),
                     ActualLastDayOfLeave=new DateTime(2020, 09, 15),
                     FirstDayBackAtWork = new DateTime(2020, 09, 16),
                     FirstDayAtWork = null
                    }
                },
                new LOATransaction
                {
                    BusinessProcessEvent = "Leave Return for Meryem Benamour (06MER) last day of leave on 10/03/2019, first day back at work on 10/04/2019",
                    BusinessProcessReason = null,
                    BusinessProcessType = "Request Return from Leave of Absence",
                    CompletedDate = new DateTime(2020, 10, 07),
                    EffectiveDate = new DateTime(2020, 10, 03),
                    LastModifiedDate = new DateTime(2020, 10, 03),
                    EmployeeStatus = "Active",
                    MostRecentCorrectionDate = null,
                    TerminationStatusEffectiveDate = null,
                    EmployeeCode = "06MER",
                    EmployeeName = "Meryem Benamour (06MER)",
                    TransactionStatus = "Rescinded",
                    Transaction = null
                }
            };
            return fakeLOATransaction;
        }

        private static int GetWorkingDaysInMonth(int month, int year)
        {
            var weekends = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var businessDaysInMonth = Enumerable.Range(1, daysInMonth)
                .Where(d => !weekends.Contains(new DateTime(year, month, d).DayOfWeek));

            return businessDaysInMonth.Count();
        }

        private IEnumerable<LOATransaction> GetFakeEmployeesLOATransactionsPending()
        {
            var fakeLOATransactions = new List<LOATransaction>
            {
                new LOATransaction
                {
                    Id = "45ba57294d3501517823bc0e6a565aba",
                    BusinessProcessEvent = "Leave Return for Aakash Bakrania (40454) last day of leave on 12/31/2020, first day back at work on 01/01/2021",
                    BusinessProcessReason = null,
                    BusinessProcessType = "Request Return from Leave of Absence",
                    CompletedDate = new DateTime(2020, 07, 17),
                    EffectiveDate = new DateTime(2020, 12, 31),
                    LastModifiedDate = null,
                    EmployeeStatus = "On Leave",
                    MostRecentCorrectionDate = null,
                    TerminationStatusEffectiveDate = null,
                    EmployeeCode = "40454",
                    EmployeeName = "Aakash Bakrania (40454)",
                    TransactionStatus = "Rescinded",
                    Transaction = null
                },
                new LOATransaction
                {
                    Id = "deda479fc316013695e2b7ddee002b3a",
                    BusinessProcessEvent = "Leave Request: Abdulla Janahi (46033)",
                    BusinessProcessReason = "Personal > Unpaid Personal",
                    BusinessProcessType = "Request Leave of Absence",
                    CompletedDate = new DateTime(2020, 03, 17),
                    EffectiveDate = new DateTime(2020, 05, 27),
                    LastModifiedDate = null,
                    EmployeeStatus = "On Leave",
                    MostRecentCorrectionDate = null,
                    TerminationStatusEffectiveDate = null,
                    EmployeeCode = "46033",
                    EmployeeName = "Abdulla Janahi (46033)",
                    TransactionStatus = "Successfully Completed",
                    Transaction = new LOATransactionProcess
                    {
                        LastDayOfWork = DateTime.Now.AddDays(30).Date,
                        FirstDayOfLeave = DateTime.Now.AddDays(31).Date,
                        EstimatedLastDayOfLeave = DateTime.Now.AddDays(90).Date,
                        ActualLastDayOfLeave = null,
                        FirstDayBackAtWork = null,
                        FirstDayAtWork = null
                    }
                },
                new LOATransaction
                {
                    Id = "89cb9865afda01bed709da4198181e2c",
                    BusinessProcessEvent = "Leave Request: Akansha Arya (35783) (Rescinded)",
                    BusinessProcessReason = "Parental > Paid Parental",
                    BusinessProcessType = "Request Leave of Absence",
                    CompletedDate = new DateTime(2019, 08, 15),
                    EffectiveDate = new DateTime(2020, 05, 21),
                    LastModifiedDate = null,
                    EmployeeStatus = "On Leave",
                    MostRecentCorrectionDate = null,
                    TerminationStatusEffectiveDate = null,
                    EmployeeCode = "35783",
                    EmployeeName = "Akansha Arya (35783)",
                    TransactionStatus = "Rescinded",
                    Transaction = new LOATransactionProcess
                    {
                        LastDayOfWork = new DateTime(2020,05,20),
                        FirstDayOfLeave = new DateTime(2020,05,21),
                        EstimatedLastDayOfLeave = new DateTime(2020,07,29),
                        ActualLastDayOfLeave = null,
                        FirstDayBackAtWork = null,
                        FirstDayAtWork = null
                    }
                },
                new LOATransaction
                {
                    Id = "792349a4d3e80154db5493c148302138",
                    BusinessProcessEvent = "Leave Request: Akansha Arya (35783)",
                    BusinessProcessReason = "Parental > Unpaid Parental",
                    BusinessProcessType = "Request Leave of Absence",
                    CompletedDate = new DateTime(2019, 10, 23),
                    EffectiveDate = new DateTime(2020, 07, 08),
                    LastModifiedDate = null,
                    EmployeeStatus = "On Leave",
                    MostRecentCorrectionDate = null,
                    TerminationStatusEffectiveDate = null,
                    EmployeeCode = "35783",
                    EmployeeName = "Akansha Arya (35783)",
                    TransactionStatus = "Successfully Completed",
                    Transaction = new LOATransactionProcess
                    {
                        LastDayOfWork = DateTime.Now.AddDays(10).Date,
                        FirstDayOfLeave = DateTime.Now.AddDays(11).Date,
                        EstimatedLastDayOfLeave = DateTime.Now.AddDays(60).Date,
                        ActualLastDayOfLeave = DateTime.Now.AddDays(65).Date,
                        FirstDayBackAtWork = DateTime.Now.AddDays(66).Date,
                        FirstDayAtWork = null
                    }
                },
                new LOATransaction
                {
                    Id = "f7bb5e21d6de0178262747749401997d",
                    BusinessProcessEvent = "Leave Request: Alastair Kendall (03AKN)",
                    BusinessProcessReason = "Personal > Unpaid Personal",
                    BusinessProcessType = "Request Leave of Absence",
                    CompletedDate = new DateTime(2020, 02, 06),
                    EffectiveDate = new DateTime(2020, 08, 12),
                    LastModifiedDate = null,
                    EmployeeStatus = "On Leave",
                    MostRecentCorrectionDate = null,
                    TerminationStatusEffectiveDate = null,
                    EmployeeCode = "03AKN",
                    EmployeeName = "Alastair Kendall (03AKN)",
                    TransactionStatus = "Successfully Completed",
                    Transaction = new LOATransactionProcess
                    {
                        LastDayOfWork = null,
                        FirstDayOfLeave = DateTime.Now.AddDays(25).Date,
                        EstimatedLastDayOfLeave = null,
                        ActualLastDayOfLeave = null,
                        FirstDayBackAtWork = null,
                        FirstDayAtWork = null
                    }
                }
            };
            return fakeLOATransactions;
        }
        #endregion
    }
}
