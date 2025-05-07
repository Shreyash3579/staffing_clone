using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vacation.API.Contracts.RepositoryInterfaces;
using Vacation.API.Core.Services;
using Vacation.API.Models;
using Xunit;

namespace Vacation.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Vacation.API.VacationRequestServices")]
    public class VacationRequestServiceTests
    {
        [Theory]
        [InlineData("", "Employee Code can not be null")]
        public async Task GetFutureVacationRequestsByEmployee_should_return_ArgumentException(string employeeCode, string errorMessage)
        {
            //Arrange
            var mockCaseRepo = new Mock<IVacationRequestRepository>();

            var sut = new VacationRequestService(mockCaseRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetVacationRequestsByEmployee(employeeCode, null, null));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("39209")]
        public async Task GetFutureVacationRequestsByEmployee_should_return_vacations_with_futureEndDates(string employeeCode)
        {
            //Arrange
            var mockCaseRepo = new Mock<IVacationRequestRepository>();
            mockCaseRepo.Setup(x => x.GetVacationRequestsByEmployee(employeeCode, null, null))
                .ReturnsAsync(GetFakeVacationRequests());

            var sut = new VacationRequestService(mockCaseRepo.Object);

            //Act
            var result = await sut.GetVacationRequestsByEmployee(employeeCode, null, null);
            var results = result.ToList();

            //Assert
            result.ToList().Count().Should().Be(6);
            results.ToList().ForEach(c => c.Status.Should().NotBeNull());
            results.ToList().ForEach(c => c.Description.Should().NotBeNull());
            results.ToList().ForEach(c => c.StartDate.Should().NotBe(DateTime.MinValue));
            results.ToList().ForEach(c => c.EndDate.Should().NotBe(DateTime.MinValue));
            results.ToList().ForEach(c => c.EndDate.Should().BeOnOrAfter(c.StartDate));
            results[0].StartDate.Should().Be(new DateTime(2021, 05, 12));
            results[0].EndDate.Should().Be(new DateTime(2021, 05, 14));
            results[0].Status.Should().Be("Pending");
            results[1].StartDate.Should().Be(new DateTime(2021, 02, 26));
            results[1].EndDate.Should().Be(new DateTime(2021, 02, 27));
            results[1].Status.Should().Be("Cancelled");
            results[2].StartDate.Should().Be(new DateTime(2021, 05, 26));
            results[2].EndDate.Should().Be(new DateTime(2021, 05, 28));
            results[2].Status.Should().Be("Approved");
            results[3].StartDate.Should().Be(new DateTime(2021, 06, 21));
            results[3].EndDate.Should().Be(new DateTime(2021, 07, 02));
            results[3].Status.Should().Be("Approved");
            results[4].StartDate.Should().Be(new DateTime(2021, 07, 13));
            results[4].EndDate.Should().Be(new DateTime(2021, 07, 16));
            results[4].Status.Should().Be("Approved");
            results[5].StartDate.Should().Be(new DateTime(2021, 07, 20));
            results[5].EndDate.Should().Be(new DateTime(2021, 07, 22));
            results[5].Status.Should().Be("Approved");
        }

        private IEnumerable<VacationRequest> GetFakeVacationRequests()
        {
            var fakeVacationRequests = new List<VacationRequest>
            {
                new VacationRequest
                {
                    Status = "Pending",
                    EndDate = new DateTime(2021, 05, 14),
                    StartDate = new DateTime(2021, 05, 12),
                    Description = "Leave Request 1",

                },
                new VacationRequest
                {
                    Status = "Cancelled",
                    EndDate = new DateTime(2021, 02, 27),
                    StartDate = new DateTime(2021, 02, 26),
                    Description = "Leave Request 2",

                },
                new VacationRequest
                {
                    Status = "Approved",
                    EndDate = new DateTime(2021, 05, 28),
                    StartDate = new DateTime(2021, 05, 26),
                    Description = "Leave Request 3",

                },
                new VacationRequest
                {
                    Status = "Approved",
                    EndDate = new DateTime(2021, 06, 25),
                    StartDate = new DateTime(2021, 06, 21),
                    Description = "Leave Request 4",

                },
                new VacationRequest
                {
                    Status = "Approved",
                    EndDate = new DateTime(2021, 07, 02),
                    StartDate = new DateTime(2021, 06, 28),
                    Description = "Leave Request 5",

                },
                new VacationRequest
                {
                    Status = "Approved",
                    EndDate = new DateTime(2021, 07, 16),
                    StartDate = new DateTime(2021, 07, 13),
                    Description = "Leave Request 6",

                },
                new VacationRequest
                {
                    Status = "Approved",
                    EndDate = new DateTime(2021, 07, 22),
                    StartDate = new DateTime(2021, 07, 20),
                    Description = "Leave Request 7",

                }
            };
            return fakeVacationRequests;
        }
    }
}