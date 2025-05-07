using FluentAssertions;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.Tests.IntegrationTests;
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
    [Trait("IntegrationTest", "Staffing.HttpAggregator.ProjectAggregatorController")]
    public class ProjectAggregatorControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public ProjectAggregatorControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [ClassData(typeof(DemandFilterCriteriaDataGenerator))]
        public async Task GetOpportunitiesAndCasesWithAllocationsBySelectedValues_should_return_opportunities_and_billableAndclientdev_cases(DemandFilterCriteria demandFilterCriteria)
        {
            //Arrange
            demandFilterCriteria.CaseTypeCodes = "1"; //billable and client dev

            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/projectaggregator/projectBySelectedValues",
                demandFilterCriteria);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualProjects = JsonConvert.DeserializeObject<IEnumerable<ProjectData>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actualProjects.Count().Should().BeGreaterOrEqualTo(0);

            actualProjects.ForEach(p => p.StartDate.Should().NotBe(DateTime.MinValue));
            actualProjects.ForEach(p => p.EndDate.Should().NotBe(DateTime.MinValue));
            actualProjects.ForEach(p => p.ManagingOfficeAbbreviation.Should().BeOneOf("BOS", "NDC"));
            actualProjects.ForEach(p => p.ClientName.Should().NotBeNullOrEmpty());

            foreach (var project in actualProjects)
            {
                if (project.PipelineId.HasValue)
                {
                    Test_opportunity_specific_data(project, demandFilterCriteria);
                }
                else
                {
                    //Test case specific data
                    Test_case_specific_data(project, demandFilterCriteria);
                }
            }
        }

        [Theory]
        [ClassData(typeof(DemandFilterCriteriaDataGenerator))]
        public async Task GetOpportunitiesAndCasesWithAllocationsBySelectedValues_should_return_opportunities_with_nullable_endDates(DemandFilterCriteria demandFilterCriteria)
        {
            //Arrange
            demandFilterCriteria = GetDemandFilterCriteriaForNullableEndDates(); //billable and client dev

            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/projectaggregator/projectBySelectedValues",
                demandFilterCriteria);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualProjects = JsonConvert.DeserializeObject<IEnumerable<ProjectData>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actualProjects.Count().Should().BeGreaterOrEqualTo(0);
            var opportunities = actualProjects.Where(x => x.PipelineId.HasValue && string.IsNullOrEmpty(x.OldCaseCode)).ToList();
            opportunities.ForEach(o => o.StartDate.Should().BeAfter(demandFilterCriteria.StartDate));
            opportunities.ForEach(o => o.ManagingOfficeAbbreviation.Should().BeOneOf("LAN"));
            opportunities.ForEach(o => o.OpportunityName.Should().NotBeNullOrEmpty());
        }

        [Theory]
        [ClassData(typeof(DemandFilterCriteriaDataGenerator))]
        public async Task GetOpportunitiesAndCasesWithAllocationsBySelectedValues_should_return_Cases_And_Opps_With_Revenues(DemandFilterCriteria demandFilterCriteria)
        {
            //Arrange

            //multiple service lines that has revenue in the given date range
            demandFilterCriteria = GetDemandFilterCriteriaForMultipleServiceLines();

            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/projectaggregator/projectBySelectedValues",
                demandFilterCriteria);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualProjects = JsonConvert.DeserializeObject<IEnumerable<ProjectData>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actualProjects.Count().Should().BeGreaterOrEqualTo(0);
            var opportunities = actualProjects.Where(x => x.PipelineId.HasValue && string.IsNullOrEmpty(x.OldCaseCode)).ToList();
            opportunities.ForEach(o => o.EndDate.Should().BeOnOrAfter(demandFilterCriteria.StartDate));
            opportunities.ForEach(o => o.StartDate.Should().BeOnOrBefore(demandFilterCriteria.EndDate));
            opportunities.ForEach(o => o.ManagingOfficeAbbreviation.Should().BeOneOf("LAN"));
            opportunities.ForEach(o => o.OpportunityName.Should().NotBeNullOrEmpty());

            var cases = actualProjects.Where(x => x.CaseCode > 0 && string.IsNullOrEmpty(x.PipelineId.ToString())).ToList();
            cases.ForEach(c => c.EndDate.Should().BeOnOrAfter(demandFilterCriteria.StartDate));
            cases.ForEach(c => c.StartDate.Should().BeOnOrBefore(demandFilterCriteria.EndDate));
            cases.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
            cases.ForEach(c => demandFilterCriteria.CaseTypeCodes.Contains(c.CaseTypeCode.ToString()));
        }

        [Theory]
        [InlineData("")]
        [InlineData("h4l")]
        [InlineData("chane")]
        public async Task GetProjectsForTypeahead_should_return_projects(string searchString)
        {
            //Arrange

            //Act
            var response = await _testServer.Client.GetAsync($"/api/projectaggregator/projectTypeahead?searchString={searchString}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualProjects = JsonConvert.DeserializeObject<IEnumerable<ProjectData>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
            {
                actualProjects.Count().Should().Be(0);
            }
            else
            {
                actualProjects.Count().Should().BeGreaterThan(0);

                var opportunities = actualProjects.Where(x => x.PipelineId.HasValue && string.IsNullOrEmpty(x.OldCaseCode)).ToList();
                var cases = actualProjects.Where(x => !string.IsNullOrEmpty(x.OldCaseCode) && !x.PipelineId.HasValue).ToList();

                actualProjects.ForEach(p => p.StartDate.Should().NotBe(DateTime.MinValue));
                actualProjects.ForEach(p => p.EndDate.Should().NotBe(DateTime.MinValue));
                actualProjects.ForEach(p => p.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty());
                actualProjects.ForEach(p => p.ClientName.Should().NotBeNullOrEmpty());

                opportunities.ForEach(p => p.EndDate.Should().BeOnOrAfter(DateTime.Today));
                opportunities.ForEach(o => o.OpportunityName.Should().NotBeNullOrEmpty());

                cases.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
                cases.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
                cases.ForEach(c => c.EndDate.Should().BeOnOrAfter(c.StartDate.Value));
            }

        }

        [Theory]
        [InlineData("")]
        [InlineData("h4l")]
        [InlineData("chane")]
        public async Task GetCasesForTypeahead_should_return_cases(string searchString)
        {
            //Arrange

            //Act
            var response = await _testServer.Client.GetAsync($"/api/projectaggregator/caseTypeahead?searchString={searchString}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var cases = JsonConvert.DeserializeObject<IEnumerable<ProjectData>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
            {
                cases.Count().Should().Be(0);
            }
            else
            {
                cases.Count().Should().BeGreaterThan(0);

                cases.ForEach(c => c.PipelineId.Should().NotHaveValue());
                cases.ForEach(c => c.OpportunityName.Should().BeNullOrEmpty());
                cases.ForEach(c => c.StartDate.Should().NotBe(DateTime.MinValue));
                cases.ForEach(c => c.EndDate.Should().NotBe(DateTime.MinValue));
                cases.ForEach(c => c.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty());
                cases.ForEach(c => c.ClientName.Should().NotBeNullOrEmpty());
                cases.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
                cases.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
                cases.ForEach(c => c.EndDate.Should().BeOnOrAfter(c.StartDate.Value));
            }

        }

        #region Helper Tests

        private void Test_case_specific_data(ProjectData project, DemandFilterCriteria demandFilterCriteria)
        {
            project.CaseType.Should().BeOneOf("Billable", "Client Development");
            project.OldCaseCode.Should().NotBeNullOrEmpty();
            project.CaseName.Should().NotBeNullOrEmpty();
            project.CaseCode.Should().BeGreaterThan(0);
            project.ClientCode.Should().BeGreaterThan(0);
            project.Type.Should().BeOneOf("NewDemand", "ActiveCase");

            if (project.AllocatedResources.Count() > 0)
            {
                project.AllocatedResources.ToList().ForEach(x => x.OldCaseCode.Should().Be(project.OldCaseCode));
                Test_resources_allocated_on_project(project, demandFilterCriteria);
            }
        }

        private void Test_opportunity_specific_data(ProjectData project, DemandFilterCriteria demandFilterCriteria)
        {
            project.OpportunityName.Should().NotBeNullOrEmpty();
            project.Type.Should().Be("Opportunity");

            if (project.AllocatedResources.Count() > 0)
            {
                project.AllocatedResources.ToList().ForEach(x => x.PipelineId.Should().Be(project.PipelineId));
                Test_resources_allocated_on_project(project, demandFilterCriteria);
            }
        }

        private void Test_resources_allocated_on_project(ProjectData project, DemandFilterCriteria demandFilterCriteria)
        {
            project.AllocatedResources.ToList().ForEach(x => x.EmployeeCode.Should().NotBeNullOrEmpty());
            //project.AllocatedResources.ToList().ForEach(x => x.FullName.Should().NotBeNullOrEmpty());
            project.AllocatedResources.ToList().ForEach(x => x.CurrentLevelGrade.Should().NotBeNullOrEmpty());
            //TODO: To be checked
            //project.AllocatedResources.ToList().ForEach(x => x.Allocation.Should().NotBe(0));
            //project.AllocatedResources.ToList().ForEach(x => x.StartDate.Should().BeOnOrBefore(demandFilterCriteria.EndDate));
            project.AllocatedResources.ToList().ForEach(x => x.EndDate.Should().BeOnOrAfter(demandFilterCriteria.StartDate));
            //project.AllocatedResources.ToList().ForEach(x => x.OfficeAbbreviation.Should().NotBeNullOrEmpty());
        }

        #endregion Helper Tests

        public class DemandFilterCriteriaDataGenerator : IEnumerable<object[]>
        {

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new DemandFilterCriteria
                    {
                        StartDate = DateTime.Now,
                        EndDate= DateTime.Now.AddDays(14),
                        OfficeCodes = "110,404", //boston and new delhi
                        CaseTypeCodes="1",    //billable and client dev,
                        DemandTypes = "StaffedCase",
                        CaseAttributeNames = "",
                        CaseExceptionShowList = "",
                        CaseExceptionHideList = "",
                        OpportunityExceptionShowList = "",
                        OpportunityExceptionHideList = "",
                        ProjectStartIndex = 1,
                        PageSize = 10
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public DemandFilterCriteria GetDemandFilterCriteriaForNullableEndDates()
        {
            return new DemandFilterCriteria
            {
                CaseTypeCodes = "1",
                DemandTypes = "Opportunity,NewDemand,ActiveCase",
                EndDate = Convert.ToDateTime("2020-8-5"),
                OfficeCodes = "127,332",
                OpportunityStatusTypeCodes = "0,1,2,3,4,5",
                PageSize = 30,
                ProjectStartIndex = 1,
                StartDate = Convert.ToDateTime("2020-7-22"),
                CaseAttributeNames = "",
                CaseExceptionShowList = "",
                CaseExceptionHideList = "",
                OpportunityExceptionShowList = "",
                OpportunityExceptionHideList = "",
            };
        }

        public DemandFilterCriteria GetDemandFilterCriteriaForMultipleServiceLines()
        {
            return new DemandFilterCriteria
            {
                CaseTypeCodes = "1",
                DemandTypes = "Opportunity,NewDemand,ActiveCase",
                EndDate = Convert.ToDateTime("2020-8-5"),
                OfficeCodes = "127,332",
                OpportunityStatusTypeCodes = "0,1,2,3,4,5",
                PageSize = 30,
                ProjectStartIndex = 1,
                StartDate = Convert.ToDateTime("2020-7-22"),
                CaseAttributeNames = "SL0001, SL0022",
                CaseExceptionShowList = "",
                CaseExceptionHideList = "",
                OpportunityExceptionShowList = "",
                OpportunityExceptionHideList = "",
            };
        }
    }
}