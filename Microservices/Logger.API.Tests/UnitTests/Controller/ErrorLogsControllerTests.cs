using FluentAssertions;
using Logger.API.Controllers;
using Microservices.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Logger.API.Tests.UnitTests.Controller
{
    [Trait("UnitTest", "Logger.API.ErrorLogsServices")]
    public class ErrorLogsControllerTests
    {
        [Theory]
        [ClassData((typeof(ErrorLogsTestDataGenerator)))]
        public async Task LogErrors_should_return_ArgumentException(ErrorLogs errorLogs)
        {
            //Arrange
            string errorMessage = "Error cannot be null";
            var sut = new ErrorLogsController();

            //Act
            var exception = await Record.ExceptionAsync(() => (Task)sut.LogErrors(errorLogs));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Fact]
        public async Task LogErrors_with_dynamic_parameter_should_return_ArgumentException()
        {
            //Arrange
            string errorMessage = "payload cannot be null or empty";

            var sut = new ErrorLogsController();

            //Act
            var exception = await Record.ExceptionAsync(() => (Task)sut.LogErrors(""));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }
    }
    public class ErrorLogsTestDataGenerator : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new ErrorLogs
                {
                    //Error = new NotImplementedException("Test Data"),
                    ApplicationName = "Test Application",
                    EmployeeCode = "45088"
                }

            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
