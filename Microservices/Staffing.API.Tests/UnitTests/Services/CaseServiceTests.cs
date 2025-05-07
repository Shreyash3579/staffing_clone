//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using AutoFixture;
//using FluentAssertions;
//using Moq;
//using Staffing.API.Contracts.RepositoryInterfaces;
//using Staffing.API.Core.Services;
//using Staffing.API.Models;
//using Xunit;

//namespace Staffing.API.Tests.UnitTests.Services
//{
//    [Trait("UnitTest", "Staffing.API.Services")]
//    public class CaseServiceTests
//    {
//        //[Theory]
//        //[InlineData("2018-11-10", "2018-11-01", "112",
//        //    "endDate should be greater than or equal to startDate")] // 112 -> NewYork
//        //[InlineData("2018-11-01", "2018-11-10", "", "Office Code can not be null")]
//        //public async Task GetActiveCasesByOffices_should_return_ArgumentException(string startDate,
//        //    string endDate, string officeCode, string errorMessage)
//        //{
//        //    //Arrange
//        //    var mockCaseRepo = new Mock<ICaseRepository>();

//        //    var sut = new CaseService(mockCaseRepo.Object);

//        //    //Act
//        //    var exception = await Record.ExceptionAsync(() => sut.GetActiveCasesByOffices(
//        //        Convert.ToDateTime(startDate),
//        //        Convert.ToDateTime(endDate), officeCode));

//        //    //Assert
//        //    exception?.Message.Should().Be(errorMessage);
//        //    exception.Should().BeOfType<ArgumentException>();
//        //}

//        //[Theory]
//        //[InlineData("2018-10-24", "2018-11-28", "110")] // 110 -> Boston
//        //public async Task GetActiveCasesByOffices_should_return_cases_with_nonNullEndDate(string startDate,
//        //    string endDate, string officeCode)
//        //{
//        //    //Arrange
//        //    var mockCaseRepo = new Mock<ICaseRepository>();
//        //    mockCaseRepo.Setup(x => x.GetActiveCasesByOffices(Convert.ToDateTime(startDate),
//        //            Convert.ToDateTime(endDate),
//        //            officeCode))
//        //        .ReturnsAsync(GetFakeActiveCases());

//        //    var sut = new CaseService(mockCaseRepo.Object);

//        //    //Act
//        //    var result = await sut.GetActiveCasesByOffices(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate),
//        //        officeCode);
//        //    var results = result.ToList();
//        //    //Assert
//        //    results.ToList().ForEach(c => c.EndDate.Should().NotBe(DateTime.MinValue));
//        //    results.FirstOrDefault(item => item.OldCaseCode == "W4TW")?.EndDate.Should().Be(new DateTime(2018, 11, 27));
//        //    results.FirstOrDefault(item => item.OldCaseCode == "N7MM")?.EndDate.Should().Be(new DateTime(2018, 11, 5));
//        //}

//        [Theory]
//        [InlineData("2018-11-10", "2018-11-01", "112",
//            "endDate should be greater than or equal to startDate")] // 112 -> NewYork
//        [InlineData("2018-11-01", "2018-11-10", "", "Office Code can not be null or empty")]
//        public async Task GetActiveCasesAndAllocationsByOffices_should_return_ArgumentException(string startDate,
//            string endDate, string officeCode, string errorMessage)
//        {
//            //Arrange
//            var mockCaseRepo = new Mock<ICaseRepository>();

//            var sut = new CaseService(mockCaseRepo.Object);

//            //Act
//            var exception = await Record.ExceptionAsync(() => sut.GetActiveCasesAndAllocationsByOffices(
//                Convert.ToDateTime(startDate),
//                Convert.ToDateTime(endDate), officeCode, null, null));

//            //Assert
//            exception?.Message.Should().Be(errorMessage);
//            exception.Should().BeOfType<ArgumentException>();
//        }

//        [Theory]
//        [InlineData("2018-10-24", "2018-11-28", "110", 1, 20)] // 110 -> Boston
//        public async Task GetActiveCasesAndAllocationsByOffices_should_return_ordered_cases_and_resourcesAllocated_with_nonNullEndDate(string startDate,
//            string endDate, string officeCode, int offsetStartIndex, int pageSize)
//        {
//            //Arrange
//            var mockCaseRepo = new Mock<ICaseRepository>();
//            mockCaseRepo.Setup(x => x.GetActiveCasesAndAllocationsByOffices(Convert.ToDateTime(startDate),
//                    Convert.ToDateTime(endDate),
//                    officeCode,
//                    offsetStartIndex,
//                    pageSize))
//                .ReturnsAsync(GetFakeActiveCasesAndAllocations());

//            var sut = new CaseService(mockCaseRepo.Object);

//            //Act
//            var result = await sut.GetActiveCasesAndAllocationsByOffices(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate),
//                officeCode, offsetStartIndex, pageSize);
//            var results = result.ToList();
//            var expectedOrderResults = results.OrderByDescending(x => x.StartDate).ToList();


//            var allocatedResourcesForCase = results.FirstOrDefault(item => item.OldCaseCode == "W4TW").AllocatedResources;
//            var expectedOrderedAllocatedResourcesForCase = allocatedResourcesForCase.OrderByDescending(x => x.CurrentLevelGrade).ThenBy(y => y.FullName);

//            //Assert
//            results.ForEach(c => c.EndDate.Should().NotBe(DateTime.MinValue));
//            results.FirstOrDefault(item => item.OldCaseCode == "W4TW")?.EndDate.Should().Be(new DateTime(2018, 11, 27));
//            results.FirstOrDefault(item => item.OldCaseCode == "N7MM")?.EndDate.Should().Be(new DateTime(2018, 11, 5));
//            results.FirstOrDefault(item => item.OldCaseCode == "T6Bw").Type.Should().Be("ActiveCase");
//            results.FirstOrDefault(item => item.OldCaseCode == "W4TW").Type.Should().Be("NewDemand");
//            allocatedResourcesForCase.Count().Should().BeGreaterThan(0);

//            //Aseert Ordering
//            expectedOrderResults.SequenceEqual(results).Should().BeTrue();
//            allocatedResourcesForCase.SequenceEqual(allocatedResourcesForCase).Should().BeTrue();
//        }


//        private IEnumerable<Case> GetFakeActiveCases()
//        {
//            var fakeCases = new List<Case>
//            {
//                new Case
//                {
//                    OldCaseCode = "W4TW",
//                    CaseName = "Project MilkyWay",
//                    ClientName = "Waystar",
//                    CaseCode = 1,
//                    ClientCode = 30139,
//                    EndDate = null,
//                    StartDate = new DateTime(2018, 10, 24),
//                    ProjectedEndDate = new DateTime(2018, 11, 27),
//                    OfficeAbbreviation = "BOS"
//                },
//                new Case
//                {
//                    OldCaseCode = "T6Bw",
//                    CaseName = "Workers Comp Market Study",
//                    ClientName = "Apax Partners & Co",
//                    CaseCode = 80,
//                    ClientCode = 6411,
//                    EndDate = new DateTime(2018, 11, 19),
//                    StartDate = new DateTime(2018, 10, 30),
//                    ProjectedEndDate = new DateTime(2018, 11, 19),
//                    OfficeAbbreviation = "BOS"
//                },
//                new Case
//                {
//                    OldCaseCode = "N7MM",
//                    CaseName = "Ortho Distribution Expansion",
//                    ClientName = "Stryker",
//                    CaseCode = 36,
//                    ClientCode = 20236,
//                    EndDate = new DateTime(2018, 11, 5),
//                    StartDate = new DateTime(2018, 11, 5),
//                    ProjectedEndDate = new DateTime(2018, 12, 31),
//                    OfficeAbbreviation = "BOS"
//                }
//            };
//            return fakeCases;
//        }

//        private IEnumerable<Case> GetFakeActiveCasesAndAllocations()
//        {
//            var fixture = new Fixture();
//            var fakeCases = new List<Case>
//            {
//                new Case
//                {
//                    OldCaseCode = "W4TW",
//                    CaseName = "Project MilkyWay",
//                    ClientName = "Waystar",
//                    CaseCode = 1,
//                    ClientCode = 30139,
//                    EndDate = null,
//                    StartDate = new DateTime(2018, 10, 24),
//                    ProjectedEndDate = new DateTime(2018, 11, 27),
//                    OfficeAbbreviation = "BOS",
//                    OfficeName = "Boston",
//                    AllocatedResources = fixture.CreateMany<ResourceAllocation>(3)
//                },
//                new Case
//                {
//                    OldCaseCode = "T6Bw",
//                    CaseName = "Workers Comp Market Study",
//                    ClientName = "Apax Partners & Co",
//                    CaseCode = 80,
//                    ClientCode = 6411,
//                    EndDate = new DateTime(2018, 11, 19),
//                    StartDate = new DateTime(2018, 09, 30),
//                    ProjectedEndDate = new DateTime(2018, 11, 19),
//                    OfficeAbbreviation = "BOS",
//                    OfficeName = "Boston",
//                    AllocatedResources = fixture.CreateMany<ResourceAllocation>(2)
//                },
//                new Case
//                {
//                    OldCaseCode = "N7MM",
//                    CaseName = "Ortho Distribution Expansion",
//                    ClientName = "Stryker",
//                    CaseCode = 36,
//                    ClientCode = 20236,
//                    EndDate = new DateTime(2018, 11, 5),
//                    StartDate = new DateTime(2018, 11, 5),
//                    ProjectedEndDate = new DateTime(2018, 12, 31),
//                    OfficeAbbreviation = "BOS",
//                    OfficeName = "Boston",
//                    AllocatedResources = new List<ResourceAllocation>()
//                }
//            };
//            return fakeCases;
//        }
//    }
//}