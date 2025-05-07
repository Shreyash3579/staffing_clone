using FluentAssertions;
using Hangfire;
using Moq;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Services;
using Staffing.API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.API.Services")]
    public class ResourceAllocationServiceTests
    {
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[]
                {
                    "", new DateTime(2019, 06, 10), new DateTime(2019, 07, 15), true, "45088",
                    "oldCaseCode cannot be empty or null"
                },
                new object[] {"H2ES", null, new DateTime(2019, 07, 15), true, "45088", "currentEndDate cannot be null"},
                new object[]
                    {"H2ES", new DateTime(2019, 06, 10), null, true, "45088", "expectedEndDate cannot be null"},
                new object[]
                {
                    "H2ES", new DateTime(2019, 07, 15), new DateTime(2019, 06, 10), true, "45088",
                    "expectedEndDate cannot be lesser than currentEndDate"
                },
                new object[]
                {
                    "H2ES", new DateTime(2019, 06, 10), new DateTime(2019, 07, 15), true, "",
                    "lastUpdatedBy cannot be null or empty"
                }
            };

        public static IEnumerable<object[]> DeleteAssignment =>
            new List<object[]>
            {
                new object[]
                {
                    Guid.Empty, "45088", "Id cannot be null or empty"
                },
                new object[]
                {
                    Guid.NewGuid(), null, "lastUpdatedBy cannot be null or empty"
                }
            };

        [Theory]
        [InlineData(null, null, null, null, null)]
        public async Task GetResourceAllocationsBySelectedValues_should_return_EmptyData(string oldCaseCodes, string employeeCodes,
            DateTime? lastUpdated, DateTime? startDate, DateTime? endDate)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

            //Act
            //var allocations = await sut.GetResourceAllocationsBySelectedValues(
            //    oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate);

            ////Assert
            //allocations.ToList().Count.Should().Be(0);

        }

        [Theory]
        [InlineData(null, null, null, null, "2020-03-01", "Error while getting resource data. Start Date should be provided with end date")]
        public async Task GetResourceAllocationsBySelectedValues_should_return_ArgumentException(string oldCaseCodes, string employeeCodes,
            DateTime? lastUpdated, DateTime? startDate, DateTime endDate, string errorMessage)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

            //Act
            //var exception = await Record.ExceptionAsync(() => sut.GetResourceAllocationsBySelectedValues(
            //    oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate));

            ////Assert
            //exception?.Message.Should().Be(errorMessage);
            //exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("")]
        public async Task GetResourceAllocationsByCaseCodes_should_return_ArgumentException(string oldCaseCodes)
        {
            var errorMessage = "Error while getting resource data. Case Codes cannot be null or empty.";
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetResourceAllocationsByCaseCodes(oldCaseCodes));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("")]
        public async Task GetResourceAllocationsByPipelineIds_should_return_ArgumentException(string pipelineIds)
        {
            var errorMessage = "Error while getting resource data. PipelineIds cannot be null or empty.";
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetResourceAllocationsByPipelineIds(pipelineIds));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("", "2019-03-01", "2019-03-15", "Office Code can not be null or empty")]
        [InlineData("110,332", "2019-03-15", "2019-03-01",
            "endDate should be greater than or equal to startDate")]
        public async Task GetResourceAllocationsByOfficeCodes_should_return_ArgumentException(string officeCodes,
            DateTime startDate,
            DateTime endDate, string errorMessage)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

            //Act
            var exception = await Record.ExceptionAsync(() =>
                sut.GetResourceAllocationsByOfficeCodes(officeCodes, startDate, endDate));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception?.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("", "employeeCodes cannot be null or empty")]
        public async Task GetResourceAllocationsByEmployeeCodes_should_return_ArgumentException(string employeeCodes,
            string errorMessage)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

            //Act
            var result = await sut.GetResourceAllocationsByEmployeeCodes(employeeCodes, null, null);

            //Assert
            result.Count().Should().Be(0);
        }

        /// below method 'AssignResourceToCase' no longer exist in ResourceAllocationService
        //[Theory]
        //[InlineData(null, "resourceAllocations cannot be null or empty")]
        //public async Task AssignResourceToCase_should_return_ArgumentException(
        //    IEnumerable<ResourceAllocation> resourceAllocation,
        //    string errorMessage)
        //{
        //    //Arrange
        //    var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
        //    var mockBackgroundJob = new Mock<IBackgroundJobClient>();

        //    var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

        //    //Act
        //    var exception = await Record.ExceptionAsync(() => sut.AssignResourceToCase(resourceAllocation));

        //    //Assert
        //    exception?.Message.Should().Be(errorMessage);
        //    exception.Should().BeOfType<ArgumentException>();
        //}

        [Theory]
        [MemberData(nameof(DeleteAssignment))]
        public async Task DeleteAssignedResourceById_should_return_ArgumentException(Guid id, string lastUpdatedBy,
            string errorMessage)
        {
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.DeleteResourceAllocationById(id, lastUpdatedBy));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        /// below method 'UpdateResourceAssignmentToCase' no longer exist in ResourceAllocationService
        //[Theory]
        //[ClassData(typeof(ResourceAllocationTestDataGenerator))]
        //public async Task UpdateResourceAssignmentToCase_should_return_ArgumentException(
        //    ResourceAllocation[] resourceData)
        //{
        //    var errorMessage = "Error while updating resource assignment. Id cannot be null";
        //    //Arrange
        //    var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
        //    var mockBackgroundJob = new Mock<IBackgroundJobClient>();

        //    var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

        //    //Act
        //    var exception = await Record.ExceptionAsync(() => sut.UpdateResourceAssignmentToCase(resourceData[0]));

        //    //Assert
        //    exception?.Message.Should().Be(errorMessage);
        //    exception.Should().BeOfType<ArgumentException>();
        //}

        //[Theory]
        //[MemberData(nameof(Data))]
        //public async Task UpdateResourcesEndDateOnCaseRoll_should_return_ArgumentException(string oldCaseCode,
        //    DateTime currentEndDate, DateTime expectedEndDate,
        //    bool isUpdateEndDateFromCCM, string lastUpdatedBy, string errorMessage)
        //{
        //    //Arrange
        //    var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
        //    var mockBackgroundJob = new Mock<IBackgroundJobClient>();

        //    var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

        //    //Act
        //    var exception = await Record.ExceptionAsync(() =>
        //        sut.UpdateResourcesEndDateOnCaseRoll(oldCaseCode, currentEndDate, expectedEndDate,
        //            isUpdateEndDateFromCCM, lastUpdatedBy));

        //    //Assert
        //    exception?.Message.Should().Be(errorMessage);
        //    exception.Should().BeOfType<ArgumentException>();
        //}

        [Theory]
        [InlineData(null)]
        public async Task UpsertResourceAllocations_should_return_ArgumentException(
       IEnumerable<ResourceAllocation> resourceData)
        {
            var errorMessage = "resourceAllocations cannot be null or empty";
            //Arrange
            var mockResourceAllocationRepo = new Mock<IResourceAllocationRepository>();
            var mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var sut = new ResourceAllocationService(mockResourceAllocationRepo.Object, mockBackgroundJob.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.UpsertResourceAllocations(resourceData));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }
    }

    public class ResourceAllocationTestDataGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new[]
                {
                    new ResourceAllocation
                    {
                        ClientCode = 7471,
                        CaseCode = 102,
                        OldCaseCode = "H2ES",
                        EmployeeCode = "38319",
                        EmployeeName = "Gupta, Praneet",
                        CurrentLevelGrade = "M9",
                        OperatingOfficeAbbreviation = "Praneet",
                        Allocation = 100,
                        StartDate = new DateTime(2019, 01, 02),
                        EndDate = new DateTime(2019, 01, 11),
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