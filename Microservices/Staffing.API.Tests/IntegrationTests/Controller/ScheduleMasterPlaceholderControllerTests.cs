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
    [Trait("IntegrationTest", "Staffing.API.ScheduleMasterPlaceholderController")]
    public class ScheduleMasterPlaceholderControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public ScheduleMasterPlaceholderControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Fact]
        public async Task UpsertPlaceholderAllocation_should_return_exception()
        {
            // Arrange
            var payload = Enumerable.Empty<ScheduleMasterPlaceholder>();

            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/scheduleMasterPlaceholder/upsertPlaceholderResourceAllocation", payload);

            //Assert
            response.IsSuccessStatusCode.Should().Be(false);
        }

        [Fact]
        public async Task UpsertPlaceholderAllocation_should_save_placeholder_allocations_in_staffingDB_and_in_analyticsDB()
        {
            // Arrange
            var payload = GetPlaceholderAllocationRequestForAnalytics();

            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/scheduleMasterPlaceholder/upsertPlaceholderResourceAllocation", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var placeholderAllocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseString);
            var savedPlaceholderAllocations = placeholderAllocations;
            var saveAnalyticsPlaceholderAllocations = placeholderAllocations;

            var placeholderAllocationsForAnalytics = savedPlaceholderAllocations.Where(x => !string.IsNullOrEmpty(x.ServiceLineCode)
                                                                                    && !string.IsNullOrEmpty(x.CurrentLevelGrade)
                                                                                    && x.OperatingOfficeCode != null
                                                                                    && (!string.IsNullOrEmpty(x.OldCaseCode) || x.PipelineId != null));

            //Assert
            placeholderAllocationsForAnalytics.Count().Should().Be(saveAnalyticsPlaceholderAllocations.Count());
        }

        [Fact]
        public async Task UpsertPlaceholderAllocation_should_save_placeholder_allocations_in_staffingDB_only()
        {
            // Arrange
            var payload = GetPlaceholderAllocationRequestForStaffing();

            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/scheduleMasterPlaceholder/upsertPlaceholderResourceAllocation", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var placeholderAllocations = JsonConvert.DeserializeObject<(IEnumerable<ScheduleMasterPlaceholder>, IEnumerable<ScheduleMasterPlaceholder>)>(responseString);
            var saveAnalyticsPlaceholderAllocations = placeholderAllocations.Item2;

            //Assert
            saveAnalyticsPlaceholderAllocations.Count().Should().Be(0);
        }

        [Theory]
        [InlineData("02VID,38122","02-15-2021", "04-15-2021")]
        public async Task GetPlacedholdersAllocationsWithinDateRange_should_return_placeholderPlanningCardsAllocations_if_exist(string employeeCodes, string startDateOfAllocations, string endDateOfAllocations)
        {
            var startDate = Convert.ToDateTime(startDateOfAllocations);
            var endDate = Convert.ToDateTime(endDateOfAllocations);
            // Arrange
            var payload = new { employeeCodes, startDate, endDate };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/scheduleMasterPlaceholder/placeholderPlanningCardAllocationsWithinDateRange", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var placeholderAllocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            placeholderAllocations.Count().Should().BeGreaterOrEqualTo(0);
            if (placeholderAllocations.Count() > 0)
            {
                Assert.NotNull(placeholderAllocations.Find(x => employeeCodes.Contains(x.EmployeeCode)));
                Assert.True(placeholderAllocations.All(allocation => 
                                                        allocation?.StartDate <= endDate && 
                                                        allocation?.EndDate >= startDate));
            }
        }

        [Theory]
        [InlineData("", "02-15-2021", "04-15-2021")]
        public async Task GetPlacedholdersAllocationsWithinDateRange_should_return_no_result_when_no_employeeCodes_passed(string employeeCodes, string startDateOfAllocations, string endDate)
        {
            var startDate = Convert.ToDateTime(startDateOfAllocations);
            // Arrange
            var payload = new { employeeCodes, startDate, endDate};

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/scheduleMasterPlaceholder/placeholderPlanningCardAllocationsWithinDateRange", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var placeholderAllocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            placeholderAllocations.Count().Should().Be(0);
        }

        [Theory]
        [InlineData("02VID,38122", null, null)]
        public async Task GetPlacedholdersAllocationsWithinDateRange_should_either_return_no_result_or_result_with_no_startDate_endDate(string employeeCodes, string startDate, string endDate)
        {
            // Arrange
            var payload = new { employeeCodes, startDate, endDate };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/scheduleMasterPlaceholder/placeholderPlanningCardAllocationsWithinDateRange", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var placeholderAllocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            if (placeholderAllocations.Count() > 0)
            {
                Assert.True(placeholderAllocations.All(allocation => employeeCodes.Split(",").Contains(allocation.EmployeeCode)));
            }
        }

        [Theory]
        [InlineData("02VID,38122", "02-17-2021", null)]
        public async Task GetPlacedholdersAllocationsWithinDateRange_should_return_result_with_endDate_as_null(string employeeCodes, string startDate, string endDate)
        {
            // Arrange
            var payload = new { employeeCodes, startDate, endDate };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/scheduleMasterPlaceholder/placeholderPlanningCardAllocationsWithinDateRange", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var placeholderAllocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            if (placeholderAllocations.Count() > 0)
            {
                Assert.True(placeholderAllocations.All(allocation => 
                                                       allocation.StartDate != null && 
                                                       (allocation.EndDate == null || allocation.EndDate >= allocation.StartDate)
                                                       ));
            }
        }

        [Theory]
        [InlineData("41413,49270", null, "04-15-2021")]
        public async Task GetPlacedholdersAllocationsWithinDateRange_should_return_result_with_startDate_as_null(string employeeCodes, string startDate, string endDate)
        {
            // Arrange
            var payload = new { employeeCodes, startDate, endDate };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/scheduleMasterPlaceholder/placeholderPlanningCardAllocationsWithinDateRange", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var placeholderAllocations = JsonConvert.DeserializeObject<IEnumerable<ScheduleMasterPlaceholder>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            if (placeholderAllocations.Count() > 0)
            {
                Assert.True(placeholderAllocations.All(allocation => 
                                                       (allocation.StartDate == null || allocation.StartDate <= allocation.EndDate) && 
                                                       allocation.EndDate != null
                                                       ));
            }
        }

        private IEnumerable<ScheduleMasterPlaceholder> GetPlaceholderAllocationRequestForStaffing()
        {
            return new List<ScheduleMasterPlaceholder>
            {
                new ScheduleMasterPlaceholder
                {
                    Id = Guid.NewGuid(),
                    PlanningCardId = null,
                    ClientCode = 18892,
                    CaseCode = 50,
                    OldCaseCode = "",
                    EmployeeCode = null,
                    ServiceLineCode = "",
                    ServiceLineName = "",
                    OperatingOfficeCode = null,
                    CurrentLevelGrade = "",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2016-07-25"),
                    EndDate = Convert.ToDateTime("2016-08-24"),
                    PipelineId = null,
                    InvestmentCode = null,
                    CaseRoleCode = null,
                    LastUpdated = null,
                    LastUpdatedBy = "51030",
                    Notes = "",
                    IsPlaceholderAllocation = true,
                    CaseName = "Retainer Team",
                    ClientName = "Elliott Management",
                    OpportunityName = null,
                    CaseTypeCode = 1,
                    CaseTypeName = "Billable",
                    EmployeeName = null,
                    OperatingOfficeName = "San Francisco",
                    OperatingOfficeAbbreviation = "SFR",
                    InvestmentName = null,
                    CaseRoleName = null
                },
                new ScheduleMasterPlaceholder
                {
                    Id = Guid.NewGuid(),
                    PlanningCardId = null,
                    ClientCode = 18892,
                    CaseCode = 50,
                    OldCaseCode = "",
                    EmployeeCode = null,
                    ServiceLineCode = "",
                    ServiceLineName = "",
                    OperatingOfficeCode = null,
                    CurrentLevelGrade = "",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2016-07-25"),
                    EndDate = Convert.ToDateTime("2016-08-24"),
                    PipelineId = null,
                    InvestmentCode = null,
                    CaseRoleCode = null,
                    LastUpdated = null,
                    LastUpdatedBy = "51030",
                    Notes = "",
                    IsPlaceholderAllocation = true,
                    CaseName = "Retainer Team",
                    ClientName = "Elliott Management",
                    OpportunityName = null,
                    CaseTypeCode = 1,
                    CaseTypeName = "Billable",
                    EmployeeName = null,
                    OperatingOfficeName = "San Francisco",
                    OperatingOfficeAbbreviation = "SFR",
                    InvestmentName = null,
                    CaseRoleName = null
                },
                new ScheduleMasterPlaceholder
                {
                    Id = Guid.NewGuid(),
                    PlanningCardId = null,
                    ClientCode = 18892,
                    CaseCode = 50,
                    OldCaseCode = "",
                    EmployeeCode = null,
                    ServiceLineCode = "",
                    ServiceLineName = "",
                    OperatingOfficeCode = null,
                    CurrentLevelGrade = "",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2016-07-25"),
                    EndDate = Convert.ToDateTime("2016-08-24"),
                    PipelineId = null,
                    InvestmentCode = null,
                    CaseRoleCode = null,
                    LastUpdated = null,
                    LastUpdatedBy = "51030",
                    Notes = "",
                    IsPlaceholderAllocation = true,
                    CaseName = "Retainer Team",
                    ClientName = "Elliott Management",
                    OpportunityName = null,
                    CaseTypeCode = 1,
                    CaseTypeName = "Billable",
                    EmployeeName = null,
                    OperatingOfficeName = "San Francisco",
                    OperatingOfficeAbbreviation = "SFR",
                    InvestmentName = null,
                    CaseRoleName = null
                }
            };
        }
        private IEnumerable<ScheduleMasterPlaceholder> GetPlaceholderAllocationRequestForAnalytics()
        {
            return new List<ScheduleMasterPlaceholder>
            {
                new ScheduleMasterPlaceholder
                {
                    Id = Guid.NewGuid(),
                    PlanningCardId = null,
                    ClientCode = 18892,
                    CaseCode = 50,
                    OldCaseCode = "C5RU",
                    EmployeeCode = null,
                    ServiceLineCode = "SL0022",
                    ServiceLineName = "AAG",
                    OperatingOfficeCode = 125,
                    CurrentLevelGrade = "A1",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2016-07-25"),
                    EndDate = Convert.ToDateTime("2016-08-24"),
                    PipelineId = null,
                    InvestmentCode = null,
                    CaseRoleCode = null,
                    LastUpdated = null,
                    LastUpdatedBy = "51030",
                    Notes = "",
                    IsPlaceholderAllocation = true,
                    CaseName = "Retainer Team",
                    ClientName = "Elliott Management",
                    OpportunityName = null,
                    CaseTypeCode = 1,
                    CaseTypeName = "Billable",
                    EmployeeName = null,
                    OperatingOfficeName = "San Francisco",
                    OperatingOfficeAbbreviation = "SFR",
                    InvestmentName = null,
                    CaseRoleName = null
                },
                new ScheduleMasterPlaceholder
                {
                    Id = Guid.NewGuid(),
                    PlanningCardId = null,
                    ClientCode = 18892,
                    CaseCode = 50,
                    OldCaseCode = "",
                    EmployeeCode = null,
                    ServiceLineCode = "",
                    ServiceLineName = "AAG",
                    OperatingOfficeCode = null,
                    CurrentLevelGrade = "",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2016-07-25"),
                    EndDate = Convert.ToDateTime("2016-08-24"),
                    PipelineId = null,
                    InvestmentCode = null,
                    CaseRoleCode = null,
                    LastUpdated = null,
                    LastUpdatedBy = "51030",
                    Notes = "",
                    IsPlaceholderAllocation = true,
                    CaseName = "Retainer Team",
                    ClientName = "Elliott Management",
                    OpportunityName = null,
                    CaseTypeCode = 1,
                    CaseTypeName = "Billable",
                    EmployeeName = null,
                    OperatingOfficeName = "San Francisco",
                    OperatingOfficeAbbreviation = "SFR",
                    InvestmentName = null,
                    CaseRoleName = null
                },
                new ScheduleMasterPlaceholder
                {
                    Id = Guid.NewGuid(),
                    PlanningCardId = null,
                    ClientCode = 18892,
                    CaseCode = 50,
                    OldCaseCode = "C5RU",
                    EmployeeCode = null,
                    ServiceLineCode = "SL0022",
                    ServiceLineName = "AAG",
                    OperatingOfficeCode = 125,
                    CurrentLevelGrade = "A1",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2016-07-25"),
                    EndDate = Convert.ToDateTime("2016-08-24"),
                    PipelineId = null,
                    InvestmentCode = null,
                    CaseRoleCode = null,
                    LastUpdated = null,
                    LastUpdatedBy = "51030",
                    Notes = "",
                    IsPlaceholderAllocation = true,
                    CaseName = "Retainer Team",
                    ClientName = "Elliott Management",
                    OpportunityName = null,
                    CaseTypeCode = 1,
                    CaseTypeName = "Billable",
                    EmployeeName = null,
                    OperatingOfficeName = "San Francisco",
                    OperatingOfficeAbbreviation = "SFR",
                    InvestmentName = null,
                    CaseRoleName = null
                }
            };
        }
    }
}
