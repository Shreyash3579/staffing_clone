using BvuCD.API.Contracts.Repository;
using BvuCD.API.Core.Services;
using BvuCD.API.Models;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BvuCD.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "BvuCD.API.TrainingServices")]
    public class TrainingServiceTests
    {
        [Theory]
        [InlineData("")]
        public async Task GetFutureTrainingsByEmployee_should_return_ArgumentException(string employeeCode)
        {
            //Arrange
            var mockTrainingRepo = new Mock<ITrainingRepository>();

            var sut = new TrainingService(mockTrainingRepo.Object);

            //Act
            var result = await sut.GetTrainingsByEmployee(employeeCode, null, null);

            //Assert
            result.Count().Should().Be(0);
        }

        [Theory]
        [InlineData("39209")]
        public async Task GetFutureTrainingssByEmployee_should_return_trainings_with_futureEndDates(string employeeCode)
        {
            //Arrange
            var mockTrainingRepo = new Mock<ITrainingRepository>();
            mockTrainingRepo.Setup(x => x.GetTrainingsByEmployee(employeeCode, null, null))
                .ReturnsAsync(GetFakeTrainings());

            var sut = new TrainingService(mockTrainingRepo.Object);

            //Act
            var result = await sut.GetTrainingsByEmployee(employeeCode, null, null);
            var results = result.ToList();

            //Assert
            results.ToList().ForEach(c => c.Role.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.TrainingName.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.Type.Should().Be("Training"));
            results.ToList().ForEach(c => c.StartDate.Should().NotBe(DateTime.MinValue));
            results.ToList().ForEach(c => c.EndDate.Should().NotBe(DateTime.MinValue));
            results.ToList().ForEach(c => c.EndDate.Should().BeOnOrAfter(c.StartDate));
        }

        private IEnumerable<Training> GetFakeTrainings()
        {
            var fakeTrainings = new List<Training>
            {
                new Training
                {
                    AttendeeRole = "Trainer",
                    EndDate = new DateTime(2020, 01, 15),
                    StartDate = new DateTime(2020, 01, 13),
                    EmployeeCode = "39209",
                    TrainingName = "ACT 1"

                },
                new Training
                {
                    AttendeeRole = "Trainee",
                    EndDate = new DateTime(2020, 01, 15),
                    StartDate = new DateTime(2020, 01, 13),
                    EmployeeCode = "39209",
                    TrainingName = "Manager Training 1"

                },
                new Training
                {
                    AttendeeRole = "Trainer",
                    EndDate = new DateTime(2020, 11, 15),
                    StartDate = new DateTime(2020, 11, 13),
                    EmployeeCode = "39209",
                    TrainingName = "ACT 5"

                },
            };
            return fakeTrainings;
        }
    }
}