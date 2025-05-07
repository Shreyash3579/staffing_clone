using FluentAssertions;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.Tests.IntegrationTests;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.HttpAggregator.Tests.IntegrationsTests.Controller
{
    [Trait("IntegrationTest", "Staffing.HttpAggregator.ResourceAggregatorController")]
    public class ResourceAggregatorControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        private const string REGION_AMERICA = "511,160,120,110,112,115,116,153,150,151,125,128,127,504,508,176,170,175,177";
        private const string REGION_APAC = "319,318,320,325,336,335,397,332,334,404,324,360,333,520,323,315,310";
        private const string REGION_EMEA = "396,322,265,260,502,227,221,220,275,505,506,512,210,281,280,236,380,507,235,370,237,341,399,342,285,407,295,290,230,385,343";

        public ResourceAggregatorControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }


        [Theory]
        [ClassData(typeof(SupplyFilterCriteriaDataGenerator))]
        public async Task GetResourcesFilteredBySelectedValues_should_return_filtered_resources(SupplyFilterCriteria supplyFilterCriteria)
        {
            //Arrange
            var offices = supplyFilterCriteria.OfficeCodes.Split(",");
            var serviceLines = supplyFilterCriteria.StaffingTags.Split(",");
            var levelGrades = supplyFilterCriteria.LevelGrades.Split(",");

            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/resourceaggregator/resourcesFilteredBySelectedValues", supplyFilterCriteria);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resourceCommitments = JsonConvert.DeserializeObject<ResourceCommitment>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            foreach (var resource in resourceCommitments.Resources)
            {
                resource.EmployeeCode.Should().NotBeNullOrEmpty();
                resource.FirstName.Should().NotBeNullOrEmpty();
                resource.LastName.Should().NotBeNullOrEmpty();
                resource.FullName.Should().NotBeNullOrEmpty();
                resource.LevelGrade.Should().NotBeNullOrEmpty();
                resource.LevelName.Should().NotBeNullOrEmpty();
                resource.InternetAddress?.Should().NotBeNullOrEmpty();
                resource.StartDate.Should().NotBe(DateTime.MinValue);
                resource.TerminationDate?.Should().BeOnOrAfter(supplyFilterCriteria.StartDate);
                resource.ProfileImageUrl.Should().NotBeNullOrEmpty();
                resource.Office.Should().NotBeNull();
                resource.Office.Should().BeOfType<Office>();
                resource.SchedulingOffice.Should().NotBeNull();
                resource.SchedulingOffice.OfficeCode.ToString().Should().BeOneOf(offices);
                resource.ServiceLine.ServiceLineCode.Should().BeOneOf(serviceLines);
                resource.FTE.Should().BeInRange(0, 1);

                if (!string.IsNullOrEmpty(supplyFilterCriteria.LevelGrades))
                {
                    resource.LevelGrade.Should().BeOneOf(levelGrades);
                }
            }
        }

        [Theory]
        [InlineData("41TBR")]
        public async Task GetAdvisorByEmployeeCode_should_return_advisor_name(string employeeCode)
        {

            ////Act
            //var response = await _testServer.Client.GetAsync($"/api/resourceaggregator/advisorByEmployeeCode?employeeCode={employeeCode}");

            //response.EnsureSuccessStatusCode();
            //var responseString = await response.Content.ReadAsStringAsync();
            //var advisor = JsonConvert.DeserializeObject<AdvisorViewModel>(responseString);

            ////Assert
            //response.StatusCode.Should().Be(HttpStatusCode.OK);
            //advisor.FullName.Should().NotBeNullOrEmpty();
            //advisor.FullName.Length.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData("")]
        public async Task GetAdvisorByEmployeeCode_should_return_blank_advisor_name(string employeeCode)
        {

            //Act
            var response = await _testServer.Client.GetAsync($"/api/resourceaggregator/advisorByEmployeeCode?employeeCode={employeeCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var advisor = JsonConvert.DeserializeObject<AdvisorViewModel>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            advisor.FullName.Should().BeNullOrEmpty();
        }

        public class SupplyFilterCriteriaDataGenerator : IEnumerable<object[]>
        {

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new SupplyFilterCriteria
                    {
                        StartDate = DateTime.Now,
                        EndDate= DateTime.Now.AddDays(14),
                        OfficeCodes = REGION_AMERICA,
                        LevelGrades = "",
                        StaffingTags = "SL0001"
                    }
                };

                yield return new object[]
                {
                    new SupplyFilterCriteria
                    {
                        StartDate = DateTime.Now,
                        EndDate= DateTime.Now.AddDays(14),
                        OfficeCodes = REGION_EMEA,
                        LevelGrades = "",
                        StaffingTags = "SL0001"
                    }
                };

                yield return new object[]
                {
                    new SupplyFilterCriteria
                    {
                        StartDate = DateTime.Now,
                        EndDate= DateTime.Now.AddDays(14),
                        OfficeCodes = REGION_APAC,
                        LevelGrades = "A1,A2,C1,C2,M1,M2",
                        StaffingTags = "SL0001" //General Consulting
                    }
                };

                yield return new object[]
                {
                    new SupplyFilterCriteria
                    {
                        StartDate = DateTime.Now,
                        EndDate= DateTime.Now.AddDays(14),
                        OfficeCodes = REGION_AMERICA,
                        LevelGrades = "F4,F6,F8,F9,F10,F12,TG6,TG8,TG9",
                        StaffingTags = "SL0022,SL0011" //AAG, FRWD
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
