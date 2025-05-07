using FluentAssertions;
using Moq;
using Pipeline.API.Contracts.RepositoryInterfaces;
using Pipeline.API.Core.Services;
using Pipeline.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Pipeline.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Pipeline.API.Services")]
    public class OpportunityServiceTests
    {
        [Theory]
        [InlineData("2019-01-10", "2019-01-01", "110,112", "0,1,2,3,4,5",
            "endDate should be greater than or equal to startDate")] // 112 -> NewYork
        [InlineData("2019-01-01", "2019-01-10", null, "0,1,2,3,4,5", "Office Codes can not be null")]
        public async Task GetOpportunitiesByOffices_should_return_ArgumentException(DateTime startDate, DateTime endDate,
            string officeCodes, string opportunityStatusTypeCodes, string errorMessage)
        {
            //Arrange
            var mockCaseRepo = new Mock<IOpportunityRepository>();

            var sut = new OpportunityService(mockCaseRepo.Object);
            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetOpportunitiesByOfficesActiveInDateRange(
                Convert.ToDateTime(startDate),
                Convert.ToDateTime(endDate),
                officeCodes, opportunityStatusTypeCodes));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("2019-01-01", "2019-01-14", "110,112", "0,1,2,3,4,5")] // 110 -> Boston
        public async Task GetOpportunitiesByOffices_should_return_opportunities_with_nonNullStartDate_and_likelyEndDate(DateTime startDate,
            DateTime endDate, string officeCodes, string opportunityStatusTypeCodes)
        {
            //Arrange
            var mockCaseRepo = new Mock<IOpportunityRepository>();
            mockCaseRepo.Setup(x => x.GetOpportunitiesByOfficesActiveInDateRange(startDate, endDate, officeCodes, opportunityStatusTypeCodes)).ReturnsAsync(GetFakeOpportunities());

            var sut = new OpportunityService(mockCaseRepo.Object);

            //Act
            var results = (await sut.GetOpportunitiesByOfficesActiveInDateRange(startDate, endDate, officeCodes, opportunityStatusTypeCodes)).ToList();
            var orderedResults = results.OrderByDescending(x => x.StartDate);

            //Assert
            results.ForEach(c => c.StartDate.Should().NotBeNull());
            results.ForEach(c => c.ClientName.Should().NotBeNull());
            results.FirstOrDefault(c => c.ClientCode == 30139).EndDate.Should().BeNull();
            results.FirstOrDefault(c => c.ClientCode == 22605).EndDate.Should().Be(new DateTime(2019, 08, 17));
            results.FirstOrDefault(c => c.ClientCode == 30320).EndDate.Should().Be(new DateTime(2019, 05, 29));
            results.FirstOrDefault(c => c.ClientCode == 30335).EndDate.Should().Be(new DateTime(2020, 04, 26));
            //orderedResults.SequenceEqual(results).Should().BeTrue(); //test ordering of results
        }

        [Theory]
        [InlineData("pipelineId can not be null or empty")] // 112 -> NewYork
        public async Task GetOpportunityByPipelineId_should_return_ArgumentException(string errorMessage)
        {
            //Arrange
            var mockCaseRepo = new Mock<IOpportunityRepository>();

            var sut = new OpportunityService(mockCaseRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetOpportunityDetailsByPipelineIds(string.Empty));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }


        private IEnumerable<Opportunity> GetFakeOpportunities()
        {
            var fakeOpportunities = new List<Opportunity>
            {
                new Opportunity
                {
                    OpportunityName = "Reshaping Marketing Mix",
                    ClientName = "Waystar",
                    ClientCode = 30139,
                    Duration = null,
                    StartDate = new DateTime(2019, 01, 13),
                    ProbabilityPercent = 60,
                    PipelineId = new Guid("F019BF07-D8F7-48D1-820D-0003A468031D")
                },
                new Opportunity
                {
                    OpportunityName = "DYNAMO - DYNAmic Marketing Optimization",
                    ClientName = "Endurance International Group",
                    ClientCode = 22605,
                    Duration = "2.25",
                    StartDate = new DateTime(2019, 06, 10),
                    ProbabilityPercent = null,
                    PipelineId = new Guid("7A817935-3A58-4BFA-A73B-00047E1CD606")
                },
                new Opportunity
                {
                    OpportunityName = "Growth Strategy",
                    ClientName = "Implus",
                    ClientCode = 30320,
                    Duration = "4.50",
                    StartDate = new DateTime(2019, 01, 15),
                    ProbabilityPercent = 90
                },
                new Opportunity
                {
                    OpportunityName = "Pricing",
                    ClientName = "StonePoint Materials",
                    ClientCode = 30335,
                    Duration = "8.75",
                    StartDate = new DateTime(2019, 08, 05),
                    ProbabilityPercent = 75
                }
            };
            return fakeOpportunities;
        }
    }
}