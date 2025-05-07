using FluentAssertions;
using Moq;
using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Core.Services;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.Analytics.API.Tests.UnitTests.Services
{
    public class PlaceholderAnalyticsServiceTests
    {
        [Theory]
        [InlineData("edcbae14-1986-eb11-a9a8-d7c2df109166")]
        public async Task DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds_should_execute_without_any_exception(string deleteAllocationIds)
        {
            //Arrange
            var mockPlaceholderAllocationRepo = new Mock<IPlaceholderAllocationRepository>();
            mockPlaceholderAllocationRepo.Setup(x => x.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(deleteAllocationIds));

            var sut = new PlaceholderAnalyticsService(mockPlaceholderAllocationRepo.Object);

            //Act
            await sut.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(deleteAllocationIds);
        }

        [Theory]
        [InlineData("", "scheduleMasterPlaceholderIds cannot be null or empty")]
        public async Task DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds_should_return_exception(string deleteAllocationIds, string errorMessage)
        {
            //Arrange
            var mockPlaceholderAllocationRepo = new Mock<IPlaceholderAllocationRepository>();
            mockPlaceholderAllocationRepo.Setup(x => x.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(deleteAllocationIds));

            var sut = new PlaceholderAnalyticsService(mockPlaceholderAllocationRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(deleteAllocationIds));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Fact]
        public async Task CreatePlaceholderAnalyticsReport_should_execute_without_any_exception()
        {
            //Arrange
            var mockPlaceholderAllocationRepo = new Mock<IPlaceholderAllocationRepository>();
            var scheduleMasterPlaceholderAllocations = GetFakeScheduleMasterAllocation();
            var scheduleMasterPlaceholderDataTable = GetFakeScheduleMasterPlaceholderAllocation(scheduleMasterPlaceholderAllocations);

            mockPlaceholderAllocationRepo.Setup(x => x.UpsertPlaceholderAnalyticsReportData(scheduleMasterPlaceholderDataTable))
                .ReturnsAsync(GetFakeScheduleMasterAllocation());

            var sut = new PlaceholderAnalyticsService(mockPlaceholderAllocationRepo.Object);

            //Act
            var result = await sut.CreatePlaceholderAnalyticsReport(scheduleMasterPlaceholderAllocations);

            //Assert
            result.ToList().Count().Should().Be(0);
        }

        private DataTable GetFakeScheduleMasterPlaceholderAllocation(IEnumerable<ScheduleMasterPlaceholder> scheduleMasterPlaceholderAllocations)
        {
            return GetScheuldeMasterPlaceholderDataTable(scheduleMasterPlaceholderAllocations);
        }

        private IEnumerable<ScheduleMasterPlaceholder> GetFakeScheduleMasterAllocation()
        {
            return new List<ScheduleMasterPlaceholder>
            {
                new ScheduleMasterPlaceholder
                {
                    Id = Guid.NewGuid(),
                    PlanningCardId = null,
                    OldCaseCode= "C5RU",
                    CaseName= "Retainer Team",
                    PipelineId= null,
                    OpportunityName= null,
                    ClientName= "Elliott Management",
                    CaseTypeCode= 1,
                    EmployeeCode= null,
                    EmployeeName= null,
                    CurrentLevelGrade= "A0",
                    OperatingOfficeCode= 115,
                    OperatingOfficeAbbreviation= "ATL",
                    Allocation= 100,
                    StartDate= Convert.ToDateTime("2016-07-25"),
                    EndDate= Convert.ToDateTime("2016-08-24"),
                    ServiceLineCode= "P",
                    ServiceLineName= "PEG",
                    InvestmentCode= null,
                    InvestmentName= null,
                    CaseRoleCode= null,
                    CaseRoleName= null,
                    LastUpdatedBy= "51030",
                    Notes= "",
                    IsPlaceholderAllocation= true,
                }
            };
        }

        private DataTable GetScheuldeMasterPlaceholderDataTable(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations)
        {
            var placeholderAllocationsDataTable = new DataTable();
            placeholderAllocationsDataTable.Columns.Add("id", typeof(Guid));
            placeholderAllocationsDataTable.Columns.Add("planningCardId", typeof(Guid));
            placeholderAllocationsDataTable.Columns.Add("clientCode", typeof(int));
            placeholderAllocationsDataTable.Columns.Add("caseCode", typeof(int));
            placeholderAllocationsDataTable.Columns.Add("oldCaseCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("caseName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("clientName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("opportunityName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("caseTypeCode", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("caseTypeName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("employeeCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("employeeName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("serviceLineCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("serviceLineName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("operatingOfficeCode", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("operatingOfficeName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("currentLevelGrade", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("allocation", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("startDate", typeof(DateTime));
            placeholderAllocationsDataTable.Columns.Add("endDate", typeof(DateTime));
            placeholderAllocationsDataTable.Columns.Add("pipelineId", typeof(Guid));
            placeholderAllocationsDataTable.Columns.Add("investmentCode", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("investmentName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("caseRoleCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("caseRoleName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("position", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("positionCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("positionName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("positionGroupName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            placeholderAllocationsDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("notes", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("isPlaceholderAllocation", typeof(bool));

            foreach (var record in placeholderAllocations)
            {
                var row = placeholderAllocationsDataTable.NewRow();
                row["id"] = (object)record.Id ?? DBNull.Value;
                row["planningCardId"] = (object)record.PlanningCardId ?? DBNull.Value;
                row["clientCode"] = (object)record.ClientCode ?? DBNull.Value;
                row["caseCode"] = (object)record.CaseCode ?? DBNull.Value;
                row["oldCaseCode"] = (object)record.OldCaseCode ?? DBNull.Value;
                row["caseName"] = (object)record.CaseName ?? DBNull.Value;
                row["clientName"] = (object)record.ClientName ?? DBNull.Value;
                row["opportunityName"] = (object)record.OpportunityName ?? DBNull.Value;
                row["caseTypeCode"] = (object)record.CaseTypeCode ?? DBNull.Value;
                row["caseTypeName"] = (object)record.CaseTypeName ?? DBNull.Value;
                row["employeeCode"] = (object)record.EmployeeCode ?? DBNull.Value;
                row["employeeName"] = (object)record.EmployeeName ?? DBNull.Value;
                row["serviceLineCode"] = (object)record.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)record.ServiceLineName ?? DBNull.Value;
                row["operatingOfficeCode"] = (object)record.OperatingOfficeCode ?? DBNull.Value;
                row["operatingOfficeName"] = (object)record.OperatingOfficeName ?? DBNull.Value;
                row["operatingOfficeAbbreviation"] = (object)record.OperatingOfficeAbbreviation ?? DBNull.Value;
                row["currentLevelGrade"] = (object)record.CurrentLevelGrade ?? DBNull.Value;
                row["allocation"] = (object)record.Allocation ?? DBNull.Value;
                row["startDate"] = (object)record.StartDate ?? DBNull.Value;
                row["endDate"] = (object)record.EndDate ?? DBNull.Value;
                row["pipelineId"] = (object)record.PipelineId ?? DBNull.Value;
                row["investmentCode"] = (object)record.InvestmentCode ?? DBNull.Value;
                row["investmentName"] = (object)record.InvestmentName ?? DBNull.Value;
                row["caseRoleCode"] = (object)record.CaseRoleCode ?? DBNull.Value;
                row["caseRoleName"] = (object)record.CaseRoleName ?? DBNull.Value;
                row["lastUpdated"] = (object)record.LastUpdated ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)record.LastUpdatedBy ?? DBNull.Value;
                row["notes"] = (object)record.Notes ?? DBNull.Value;
                row["isPlaceholderAllocation"] = (object)record.IsPlaceholderAllocation ?? DBNull.Value;
                placeholderAllocationsDataTable.Rows.Add(row);
            }
            return placeholderAllocationsDataTable;
        }
    }
}
