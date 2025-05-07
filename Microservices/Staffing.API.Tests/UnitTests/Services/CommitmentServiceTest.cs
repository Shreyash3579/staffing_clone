using FluentAssertions;
using Hangfire;
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
    public class CommitmentServiceTest
    {
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public async Task InsertResourcesCommitments_Should_Return_Empty_Commitment_Collection(Guid id)
        {
            //Arrange
            var mockCommitmentRepository = new Mock<ICommitmentRepository>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            var sut = new CommitmentService(mockCommitmentRepository.Object, mockBackgroundJobClient.Object);

            var emptyCommitmentsList = new List<Commitment>();

            //Act
            var response = await Record.ExceptionAsync(() => sut.UpsertResourcesCommitments(emptyCommitmentsList));

            //Assert
            response.Should().Be(null);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public async Task InsertResourcesCommitments_Should_Return_Saved_Commitment_Collection(Guid id)
        {
            //Arrange
            var mockCommitmentRepository = new Mock<ICommitmentRepository>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            var sut = new CommitmentService(mockCommitmentRepository.Object, mockBackgroundJobClient.Object);

            var resourcesCommitments = GetCommitment();

            //Act
            var response = await sut.UpsertResourcesCommitments(resourcesCommitments);

            //Assert
            response.Should().NotBeNull();
        }

        private IList<Commitment> GetCommitment()
        {
            return new List<Commitment>
            {
                new Commitment
                {
                    Id = Guid.Empty,
                    CommitmentType = new CommitmentType
                    {
                        CommitmentTypeCode = "T",
                        CommitmentTypeName = "Training",
                        IsStaffingTag = false,
                        Precedence = 7
                    },
                    EmployeeCode = "51030",
                    EndDate = Convert.ToDateTime("2020-8-31"),
                    IsSourceStaffing = true,
                    LastUpdatedBy = "51030",
                    Notes = "test training",
                    StartDate = Convert.ToDateTime("2020-8-15")
                },
                new Commitment
                {
                    Id = Guid.Empty,
                    CommitmentType = new CommitmentType
                    {
                        CommitmentTypeCode = "T",
                        CommitmentTypeName = "Training",
                        IsStaffingTag = false,
                        Precedence = 7
                    },
                    EmployeeCode = "47627",
                    EndDate = Convert.ToDateTime("2020-8-31"),
                    IsSourceStaffing = true,
                    LastUpdatedBy = "51030",
                    Notes = "test training",
                    StartDate = Convert.ToDateTime("2020-8-15")
                }
            };
        }
    }
}
