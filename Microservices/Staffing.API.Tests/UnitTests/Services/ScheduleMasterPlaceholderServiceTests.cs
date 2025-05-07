using FluentAssertions;
using Hangfire;
using Moq;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.API.Services")]
    public class ScheduleMasterPlaceholderServiceTests
    {

        [Theory]
        [InlineData("placeholder allocation cannot be null or empty")]
        public async Task UpsertPlaceholderAllocation_should_return_error(string errorMessage)
        {
            //Arrange
            var mockSchedulePlaceholdereRepo = new Mock<IScheduleMasterPlaceholderRepository>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();

            var placeholderAllocations = Enumerable.Empty<ScheduleMasterPlaceholder>();
            var sut = new ScheduleMasterPlaceholdeService(mockSchedulePlaceholdereRepo.Object, mockBackgroundJobClient.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.UpsertPlaceholderAllocation(placeholderAllocations));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Fact]
        public void UpsertPlaceholderAllocation_should_upsert_placeholder_allocations()
        {
            //Arrange
            var mockSchedulePlaceholdereRepo = new Mock<IScheduleMasterPlaceholderRepository>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();

            var sut = new ScheduleMasterPlaceholdeService(mockSchedulePlaceholdereRepo.Object, mockBackgroundJobClient.Object);


        }

        [Theory]
        [InlineData("02VID,38122,51030,51032", "02-15-2021", "04-15-2021")]
        public async Task GetPlanningCardAndPlaceholderAllocationsWithinDateRange(string employeeCodes, string allocationStartDate, string allocationEndDate)
        {
            var startDate = Convert.ToDateTime(allocationStartDate);
            var endDate = Convert.ToDateTime(allocationEndDate);
            //Arrange
            var mockSchedulePlaceholdereRepo = new Mock<IScheduleMasterPlaceholderRepository>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            mockSchedulePlaceholdereRepo.Setup(x => x.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate)).ReturnsAsync(GetFakePlaceholderAndPlanningCardAllocation());
            var sut = new ScheduleMasterPlaceholdeService(mockSchedulePlaceholdereRepo.Object, mockBackgroundJobClient.Object);
            

            var results = await sut.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate);
            results.Count().Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData("02VID,38122,51030,51032", null, null)]
        public async Task GetPlanningCardAndPlaceholderAllocationsWithinDateRange_should_return_results_with_no_startDate_no_endDate(string employeeCodes, string allocationStartDate, string allocationEndDate)
        {
            var startDate = Convert.ToDateTime(allocationStartDate);
            var endDate = Convert.ToDateTime(allocationEndDate);
            //Arrange
            var mockSchedulePlaceholdereRepo = new Mock<IScheduleMasterPlaceholderRepository>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            mockSchedulePlaceholdereRepo.Setup(x => x.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate)).ReturnsAsync(GetFakePlaceholderAndPlanningCardAllocationWithNoStartDateNoEndDate());
            var sut = new ScheduleMasterPlaceholdeService(mockSchedulePlaceholdereRepo.Object, mockBackgroundJobClient.Object);


            var results = await sut.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate);
            results.Count().Should().BeGreaterThan(0);
            if (results.Count() > 0)
            {
                Assert.True(results.All(allocation => allocation.StartDate == null && allocation.EndDate == null));
            }
        }

        [Theory]
        [InlineData("02VID,38122,51030,51032", "02-15-2021", null)]
        public async Task GetPlanningCardAndPlaceholderAllocationsWithinDateRange_should_return_results_with_no_endDate(string employeeCodes, string allocationStartDate, string allocationEndDate)
        {
            var startDate = Convert.ToDateTime(allocationStartDate);
            var endDate = Convert.ToDateTime(allocationEndDate);
            //Arrange
            var mockSchedulePlaceholdereRepo = new Mock<IScheduleMasterPlaceholderRepository>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            mockSchedulePlaceholdereRepo.Setup(x => x.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate)).ReturnsAsync(GetFakePlaceholderAndPlanningCardAllocationWithNoEndDate());
            var sut = new ScheduleMasterPlaceholdeService(mockSchedulePlaceholdereRepo.Object, mockBackgroundJobClient.Object);


            var results = await sut.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate);
            results.Count().Should().BeGreaterThan(0);
            if (results.Count() > 0)
            {
                Assert.True(results.All(allocation => allocation.StartDate != null && allocation.EndDate == null));
            }
        }

        [Theory]
        [InlineData("02VID,38122,51030,51032", null, "02-04-2015")]
        public async Task GetPlanningCardAndPlaceholderAllocationsWithinDateRange_should_return_results_with_no_startDate(string employeeCodes, string allocationStartDate, string allocationEndDate)
        {
            var startDate = Convert.ToDateTime(allocationStartDate);
            var endDate = Convert.ToDateTime(allocationEndDate);
            //Arrange
            var mockSchedulePlaceholdereRepo = new Mock<IScheduleMasterPlaceholderRepository>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            mockSchedulePlaceholdereRepo.Setup(x => x.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate)).ReturnsAsync(GetFakePlaceholderAndPlanningCardAllocationWithNoStartDate());
            var sut = new ScheduleMasterPlaceholdeService(mockSchedulePlaceholdereRepo.Object, mockBackgroundJobClient.Object);


            var results = await sut.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate);
            results.Count().Should().BeGreaterThan(0);
            if (results.Count() > 0)
            {
                Assert.True(results.All(allocation => allocation.StartDate == null && allocation.EndDate != null));
            }
        }

        private IList<ScheduleMasterPlaceholder> GetFakePlaceholderAndPlanningCardAllocation()
        {
            var placeholderAndPlanningCardAllocation = new List<ScheduleMasterPlaceholder>
            {
                new ScheduleMasterPlaceholder
                {
                    EmployeeCode = "02VID",
                    StartDate = new DateTime(2020, 02, 15),
                    EndDate = new DateTime(2020, 05, 14)
                },
                new ScheduleMasterPlaceholder
                {
                    EmployeeCode = "38122",
                    StartDate = new DateTime(2020, 02, 16),
                    EndDate = new DateTime(2020, 03, 15)
                }
            };

            return placeholderAndPlanningCardAllocation.ToList();
        }

        private IList<ScheduleMasterPlaceholder> GetFakePlaceholderAndPlanningCardAllocationWithNoEndDate()
        {
            var placeholderAndPlanningCardAllocation = new List<ScheduleMasterPlaceholder>
            {
                new ScheduleMasterPlaceholder
                {
                    EmployeeCode = "02VID",
                    StartDate = new DateTime(2020, 02, 15),
                    EndDate = null
                },
                new ScheduleMasterPlaceholder
                {
                    EmployeeCode = "38122",
                    StartDate = new DateTime(2020, 02, 16),
                    EndDate = null
                }
            };

            return placeholderAndPlanningCardAllocation.ToList();
        }

        private IList<ScheduleMasterPlaceholder> GetFakePlaceholderAndPlanningCardAllocationWithNoStartDate()
        {
            var placeholderAndPlanningCardAllocation = new List<ScheduleMasterPlaceholder>
            {
                new ScheduleMasterPlaceholder
                {
                    EmployeeCode = "02VID",
                    StartDate = null,
                    EndDate = new DateTime(2021, 04, 15)
                },
                new ScheduleMasterPlaceholder
                {
                    EmployeeCode = "38122",
                    StartDate = null,
                    EndDate = new DateTime(2021, 04, 15)
                }
            };

            return placeholderAndPlanningCardAllocation.ToList();
        }

        private IList<ScheduleMasterPlaceholder> GetFakePlaceholderAndPlanningCardAllocationWithNoStartDateNoEndDate()
        {
            var placeholderAndPlanningCardAllocation = new List<ScheduleMasterPlaceholder>
            {
                new ScheduleMasterPlaceholder
                {
                    EmployeeCode = "02VID",
                    StartDate = null,
                    EndDate = null
                },
                new ScheduleMasterPlaceholder
                {
                    EmployeeCode = "38122",
                    StartDate = null,
                    EndDate = null
                }
            };

            return placeholderAndPlanningCardAllocation.ToList();
        }
    }
}
