using FluentAssertions;
using Moq;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.API.Services")]
    public class SKUCaseTermsServiceTests
    {
        [Theory]
        [InlineData("pipelineId cannot be null or empty")]
        public async Task GetSKUTermsForOpportunity_should_return_SKUTermsForOldCaseCode(string errorMessage)
        {
            //Arrange
            var mockCaseRollRepo = new Mock<ISKUCaseTermsRepository>();
            var sut = new SKUCaseTermsService(mockCaseRollRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetSKUTermsForOpportunity(Guid.Empty));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("oldCaseCode cannot be null or empty")]
        public async Task GetSKUTermsForCase_should_return_SKUTermsForOldCaseCode(string errorMessage)
        {
            //Arrange
            var mockCaseRollRepo = new Mock<ISKUCaseTermsRepository>();
            var sut = new SKUCaseTermsService(mockCaseRollRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetSKUTermsForCase(string.Empty));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("skuCaseTerms cannot be null")]
        public async Task InsertSKUCaseTerms_should_return_Inserted_SKUTerms(string errorMessage)
        {
            //Arrange
            var mockCaseRollRepo = new Mock<ISKUCaseTermsRepository>();
            var sut = new SKUCaseTermsService(mockCaseRollRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.InsertSKUCaseTerms(null));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [MemberData(nameof(SKUCaseTermsInsertCaseData))]
        [MemberData(nameof(SKUCaseTermsInsertOpportunityData))]
        public async Task InsertSKUCaseTerms_should_return_ErrorMessage(SKUCaseTerms skuCaseTerms, string errorMessage)
        {
            //Arrange
            var mockCaseRollRepo = new Mock<ISKUCaseTermsRepository>();
            var sut = new SKUCaseTermsService(mockCaseRollRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.InsertSKUCaseTerms(skuCaseTerms));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("skuCaseTerms cannot be null")]
        public async Task UpdateSKUCaseTerms_should_return_Inserted_SKUTerms(string errorMessage)
        {
            //Arrange
            var mockCaseRollRepo = new Mock<ISKUCaseTermsRepository>();
            var sut = new SKUCaseTermsService(mockCaseRollRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.UpdateSKUCaseTerms(null));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [MemberData(nameof(SKUCaseTermsUpdateData))]
        public async Task UpdateSKUCaseTerms_should_return_ErrorMessage(SKUCaseTerms skuCaseTerms, string errorMessage)
        {
            //Arrange
            var mockCaseRollRepo = new Mock<ISKUCaseTermsRepository>();
            var sut = new SKUCaseTermsService(mockCaseRollRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.UpdateSKUCaseTerms(skuCaseTerms));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("Id cannot be null or empty")]
        public async Task DeleteSKUCaseTermsById_should_return_ErrorMessage(string errorMessage)
        {
            //Arrange
            var mockCaseRollRepo = new Mock<ISKUCaseTermsRepository>();
            var sut = new SKUCaseTermsService(mockCaseRollRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.DeleteSKUCaseTermsById(Guid.Empty));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [MemberData(nameof(SKUCaseTermsCaseDataWithDuration))]
        public async Task GetSKUTermsForCaseOrOpportunityForDuration_should_return_ErrorMessage(string oldCaseCodes, string pipelineIds,
            DateTime startDate, DateTime endDate, string errorMessage)
        {
            //Arrange
            var mockCaseRollRepo = new Mock<ISKUCaseTermsRepository>();
            var sut = new SKUCaseTermsService(mockCaseRollRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetSKUTermsForCaseOrOpportunityForDuration(oldCaseCodes, pipelineIds,
                startDate, endDate));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        public static IEnumerable<object[]> SKUCaseTermsInsertCaseData =>
            new List<object[]>
            {
                new object[]
                {
                    new SKUCaseTerms
                    {
                        OldCaseCode = "", SKUTermsCodes = "1,2", EffectiveDate = DateTime.Now.AddDays(30), LastUpdatedBy = "45088"
                    },
                    "OldCaseCode and PipelineId both cannot be null or empty"
                },
                new object[]
                {
                    new SKUCaseTerms
                    {
                        OldCaseCode = "K7FC", SKUTermsCodes = "1,2", EffectiveDate = DateTime.MinValue, LastUpdatedBy = "45088"
                    },
                    "EffectiveDate cannot be null"
                },
                new object[]
                {
                    new SKUCaseTerms
                    {
                        OldCaseCode = "K7FC", SKUTermsCodes = "1,2", EffectiveDate = DateTime.Now.AddDays(30), LastUpdatedBy = ""
                    },
                    "LastUpdatedBy cannot be null or empty"
                }
            };

        public static IEnumerable<object[]> SKUCaseTermsInsertOpportunityData =>
            new List<object[]>
            {
                new object[]
                {
                    new SKUCaseTerms
                    {
                        PipelineId = Guid.Empty, SKUTermsCodes = "1,2", EffectiveDate = DateTime.Now.AddDays(30), LastUpdatedBy = "45088"
                    },
                    "OldCaseCode and PipelineId both cannot be null or empty"
                },
                new object[]
                {
                    new SKUCaseTerms
                    {
                        PipelineId = Guid.NewGuid(), SKUTermsCodes = "1,2", EffectiveDate = DateTime.MinValue, LastUpdatedBy = "45088"
                    },
                    "EffectiveDate cannot be null"
                },
                new object[]
                {
                    new SKUCaseTerms
                    {
                        PipelineId = Guid.NewGuid(), SKUTermsCodes = "1,2", EffectiveDate = DateTime.Now.AddDays(30), LastUpdatedBy = ""
                    },
                    "LastUpdatedBy cannot be null or empty"
                }
            };
        public static IEnumerable<object[]> SKUCaseTermsUpdateData =>
            new List<object[]>
            {
                new object[]
                {
                    new SKUCaseTerms
                    {
                        Id= Guid.Empty, OldCaseCode = "K7FC", PipelineId = Guid.Empty, SKUTermsCodes = "1,2", EffectiveDate = DateTime.Now.AddDays(30), LastUpdatedBy = "45088"
                    },
                    "Id cannot be null or empty"
                },
                new object[]
                {
                    new SKUCaseTerms
                    {
                        Id= Guid.NewGuid(), OldCaseCode = "", PipelineId = Guid.Empty, SKUTermsCodes = "1,2", EffectiveDate = DateTime.Now.AddDays(30), LastUpdatedBy = "45088"
                    },
                    "OldCaseCode and PipelineId both cannot be null or empty"
                },
                new object[]
                {
                    new SKUCaseTerms
                    {
                        Id= Guid.NewGuid(), OldCaseCode = "K7FC", PipelineId = Guid.Empty, SKUTermsCodes = "1,2", EffectiveDate = DateTime.MinValue, LastUpdatedBy = "45088"
                    },
                    "EffectiveDate cannot be null"
                },
                new object[]
                {
                    new SKUCaseTerms
                    {
                        Id= Guid.NewGuid(), OldCaseCode = "", PipelineId = Guid.NewGuid(), SKUTermsCodes = "1,2", EffectiveDate = DateTime.Now.AddDays(30), LastUpdatedBy = ""
                    },
                    "LastUpdatedBy cannot be null or empty"
                }
            };

        /// <summary>
        /// This IEnumerable is used to verify the error messages for the GetSKUTermsForCaseOrOpportunityForDuration method of the SKUCaseTermsService
        /// </summary>
        public static IEnumerable<object[]> SKUCaseTermsCaseDataWithDuration =>
            new List<object[]>
            {
                new object[]
                {
                    "",
                    "",
                    "2019-07-16",
                    "2019-09-05",
                    "Both oldCaseCodes and pipelineIds cannot be null or empty"
                },
                new object[]
                {
                    "K7FC,C8MR",
                    "",
                    DateTime.MinValue,
                    "2019-09-05",
                    "startDate cannot be null"
                },
                new object[]
                {
                    "K7FC,C8MR",
                    "",
                    "2019-07-16",
                    DateTime.MinValue,
                    "endDate cannot be null"
                }
            };
    }
}
