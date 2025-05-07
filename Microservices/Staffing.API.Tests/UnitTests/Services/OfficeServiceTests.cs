using AutoFixture;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace Staffing.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.API.Services")]
    public class OfficeServiceTests
    {
        private static readonly string[] ExpectedOffices = new[] { "Atlanta", "Boston", "Chicago", "Dallas", "London", "Milan", "New Delhi", "New York", "Paris", "Singapore", "Sydney" };

        [Fact]
        //public async Task GetOfficeList_should_return_filterdOffices()
        //{
        //    //Arrange
        //    var mockOfficeRepository = new Mock<IOfficeRepository>();
        //    mockOfficeRepository.Setup(x => x.GetOfficeList()).ReturnsAsync(GetFakeOffices());
        //    var sut = new OfficeService(mockOfficeRepository.Object);

        //    //Act
        //    var result = await sut.GetOfficeList();

        //    //Assert
        //    result.ToList().ForEach(o => ExpectedOffices.Contains(o.OfficeName).Should().BeTrue());


        //}

        private IEnumerable<Office> GetFakeOffices()
        {
            var fixture = new Fixture();
            var officeList = new List<Office>();

            // Add offices to list from expectedOfficeLists
            for (int officeCount = 0; officeCount < 11; officeCount++)
            {
                officeList.Add(fixture.Build<Office>().
                    With(t => t.OfficeName, ExpectedOffices[new Random().Next(1, 10)])
                    .Create());
            }

            //Add random Office
            officeList.AddRange(fixture.CreateMany<Office>(10));

            return officeList;
        }
    }
}
