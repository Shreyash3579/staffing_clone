using FluentAssertions;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.Tests.IntegrationTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.HttpAggregator.Tests.IntegrationsTests.Controller
{
    [Trait("IntegrationTest", "Staffing.HttpAggregator.StaffingAggregatorController")]
    public class StaffingAggregatorControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public StaffingAggregatorControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData("CC73C1B6-C80E-4D9A-BE4B-36B2AED0523C")]
        public async Task GetOpportunityByPipelineId_should_return_OpportunityData(Guid pipelineId)
        {
            //Arrange

            //Act
            var response = await _testServer.Client.GetAsync($"/api/staffingAggregator/opportunityByPipelineId?pipelineId={pipelineId}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var opportunityData = JsonConvert.DeserializeObject<OpportunityDetails>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            opportunityData.PipelineId.Should().NotBeEmpty();
            opportunityData.CoordinatingPartnerCode.Should().NotBeNullOrEmpty();
            opportunityData.CoordinatingPartnerName.Should().NotBeNullOrEmpty();
            opportunityData.BillingPartnerCode.Should().NotBeNullOrEmpty();
            opportunityData.BillingPartnerName.Should().NotBeNullOrEmpty();
            opportunityData.OpportunityName.Should().NotBeNullOrEmpty();
            opportunityData.OpportunityStatus.Should().NotBeNullOrEmpty();
            opportunityData.ClientCode.Should().NotBe(0);
            opportunityData.ClientName.Should().NotBeNullOrEmpty();
            opportunityData.ManagingOfficeName.Should().NotBeNullOrEmpty();
            opportunityData.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty();
            opportunityData.BillingOfficeName.Should().NotBeNullOrEmpty();
            opportunityData.BillingOfficeAbbreviation.Should().NotBeNullOrEmpty();
            opportunityData.Type.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("CC73C1B6-C80E-4D9A-BE4B-36B2AED0523C")]
        public async Task GetOpportunityAndAllocationsByPipelineId_should_return_OpportunityData(Guid pipelineId)
        {
            //Arrange

            //Act
            var response = await _testServer.Client.GetAsync($"/api/staffingAggregator/opportunityAndAllocationsByPipelineId?pipelineId={pipelineId}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var opportunityData = JsonConvert.DeserializeObject<OpportunityDetails>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            opportunityData.PipelineId.Should().NotBeEmpty();
            opportunityData.CoordinatingPartnerCode.Should().NotBeNullOrEmpty();
            opportunityData.CoordinatingPartnerName.Should().NotBeNullOrEmpty();
            opportunityData.BillingPartnerCode.Should().NotBeNullOrEmpty();
            opportunityData.BillingPartnerName.Should().NotBeNullOrEmpty();
            opportunityData.OpportunityName.Should().NotBeNullOrEmpty();
            opportunityData.OpportunityStatus.Should().NotBeNullOrEmpty();
            opportunityData.ClientCode.Should().NotBe(0);
            opportunityData.ClientName.Should().NotBeNullOrEmpty();
            opportunityData.ManagingOfficeName.Should().NotBeNullOrEmpty();
            opportunityData.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty();
            opportunityData.BillingOfficeName.Should().NotBeNullOrEmpty();
            opportunityData.BillingOfficeAbbreviation.Should().NotBeNullOrEmpty();
            opportunityData.Type.Should().NotBeNullOrEmpty();
            opportunityData.AllocatedResources.Should().NotBeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("Eros")]
        public async Task GetOpportunitiesForTypeahead_should_return_Opportunities(string searchString)
        {
            //Arrange

            //Act
            var response = await _testServer.Client.GetAsync($"/api/staffingAggregator/typeaheadOpportunities?searchString={searchString}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var opportunities = JsonConvert.DeserializeObject<IEnumerable<OpportunityData>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
            {
                opportunities.Count().Should().Be(0);
            }
            else if (opportunities.Count > 0)
            {
                opportunities.ForEach(p => p.PipelineId.Should().NotBeEmpty());
                opportunities.ForEach(p => p.CoordinatingPartnerCode.Should().NotBeNullOrEmpty());
                opportunities.ForEach(p => p.OpportunityName.Should().NotBeNullOrEmpty());
                opportunities.ForEach(p => p.ClientCode.Should().NotBe(0));
                opportunities.ForEach(p => p.ClientName.Should().NotBeNullOrEmpty());
                opportunities.ForEach(p => p.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty());
                opportunities.ForEach(p => p.Type.Should().NotBeNullOrEmpty());
            }
        }

        [Theory]
        [InlineData("V7SJ")]
        public async Task GetCaseDataByCaseCodes_should_return_CaseData(string oldCaseCodes)
        {
            //Arrange

            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/staffingAggregator/caseDataByCaseCodes", oldCaseCodes);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var caseData = JsonConvert.DeserializeObject<CaseData>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            caseData.CaseCode.Should().NotBe(0);
            caseData.CaseName.Should().NotBeNullOrEmpty();
            caseData.ClientCode.Should().NotBe(0);
            caseData.ClientName.Should().NotBeNullOrEmpty();
            caseData.OldCaseCode.Should().NotBeNullOrEmpty();
            caseData.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty();
            caseData.StartDate.Should().NotBeOnOrBefore(DateTime.MinValue);
            caseData.EndDate.Should().BeOnOrAfter(caseData.StartDate);
            caseData.Type.Should().BeOneOf("ActiveCase", "NewDemand");
        }

        [Theory]
        [InlineData("V7SJ")]
        public async Task GetCaseDetailsByCaseCode_should_return_CaseData(string oldCaseCode)
        {
            //Arrange

            //Act
            var response = await _testServer.Client.GetAsync($"/api/staffingAggregator/caseDetailsByCaseCode?oldCaseCode={oldCaseCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var caseData = JsonConvert.DeserializeObject<CaseDetails>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            caseData.CaseCode.Should().NotBe(0);
            caseData.CaseName.Should().NotBeNullOrEmpty();
            caseData.ClientCode.Should().NotBe(0);
            caseData.ClientName.Should().NotBeNullOrEmpty();
            caseData.OldCaseCode.Should().NotBeNullOrEmpty();
            caseData.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty();
            caseData.StartDate.Should().NotBeOnOrBefore(DateTime.MinValue);
            caseData.EndDate.Should().BeOnOrAfter(caseData.StartDate);
            caseData.Type.Should().BeOneOf("ActiveCase", "NewDemand");
        }

        [Theory]
        [InlineData("V7SJ")]
        public async Task GetCaseAndAllocationsByCaseCode_should_return_CaseData(string oldCaseCode)
        {
            //Arrange

            //Act
            var response = await _testServer.Client.GetAsync($"/api/staffingAggregator/caseAndAllocationsByCaseCode?oldCaseCode={oldCaseCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var caseData = JsonConvert.DeserializeObject<CaseDetails>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            caseData.CaseCode.Should().NotBe(0);
            caseData.CaseName.Should().NotBeNullOrEmpty();
            caseData.ClientCode.Should().NotBe(0);
            caseData.ClientName.Should().NotBeNullOrEmpty();
            caseData.OldCaseCode.Should().NotBeNullOrEmpty();
            caseData.CaseType.Should().BeOneOf("Billable", "Administrative", "Client Development", "Pro Bono");
            caseData.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty();
            caseData.ManagingOfficeName.Should().NotBeNullOrEmpty();
            caseData.ManagingOfficeCode.Should().NotBeNull().And.NotBe(0);
            caseData.BillingOfficeAbbreviation.Should().NotBeNullOrEmpty();
            caseData.BillingOfficeName.Should().NotBeNullOrEmpty();
            caseData.BillingOfficeCode.Should().NotBeNull().And.NotBe(0);
            caseData.StartDate.Should().NotBeOnOrBefore(DateTime.MinValue);
            caseData.EndDate.Should().BeOnOrAfter(caseData.StartDate);
            caseData.Type.Should().BeOneOf("ActiveCase", "NewDemand");
            caseData.AllocatedResources?.ToList().ForEach(resource =>
            {
                resource.Id.Should().NotBeNull().And.NotBeEmpty();
                resource.OldCaseCode.Should().NotBeNullOrEmpty();
                resource.EmployeeCode.Should().NotBeNullOrEmpty();
                resource.OperatingOfficeCode.Should().NotBe(0);
                resource.EmployeeName.Should().NotBeNullOrEmpty();
                resource.CurrentLevelGrade.Should().NotBeNullOrEmpty();
                resource.OperatingOfficeAbbreviation.Should().NotBeNullOrEmpty();
                resource.Allocation.Should().BeInRange(1, 999);
                resource.StartDate.Should().NotBeOnOrBefore(DateTime.MinValue);
                resource.EndDate.Should().BeOnOrAfter(Convert.ToDateTime(resource.StartDate));
                resource.PipelineId.Should().BeNull();
                resource.InternetAddress.Should().NotBeNullOrEmpty();
            });
        }

        [Theory]
        [InlineData("Bartucca")]
        public async Task GetResourcesIncludingTerminatedWithAllocationsBySearchString_should_return_resourcesDataWithAllocations(string searchString)
        {
            //Arrange
            var response = await _testServer.Client.GetAsync($"/api/staffingAggregator/resourcesIncludingTerminatedStaffingBySearchString?searchString={searchString}");

            //Act
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resourceViewData = JsonConvert.DeserializeObject<IEnumerable<ResourceView>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
            {
                resourceViewData.Count().Should().Be(0);
            }
            else
            {
                if (resourceViewData.Count >= 1)
                {
                    resourceViewData.ForEach(p => p.Resource.EmployeeCode.Should().NotBeNullOrEmpty());
                    resourceViewData.ForEach(p => p.Resource.EmployeeType.Should().NotBeEmpty());
                    resourceViewData.ForEach(p => p.Resource.LastName.Should().NotBeEmpty());
                    resourceViewData.ForEach(p => p.Resource.FirstName.Should().NotBeNullOrEmpty());
                    resourceViewData.ForEach(p => p.Resource.LevelGrade.Should().NotBeEmpty());
                    resourceViewData.ForEach(p => p.Resource.LevelName.Should().NotBeEmpty());
                    resourceViewData.ForEach(p => p.Resource.InternetAddress.Should().NotBeNullOrEmpty());
                    resourceViewData.ForEach(p => p.Resource.StartDate.Should().NotBeNull());
                    resourceViewData.ForEach(p => p.Resource.Office.Should().NotBeNull());
                    resourceViewData.ForEach(p => p.Resource.Office.OfficeCode.Should().NotBe(0));
                    resourceViewData.ForEach(p => p.Resource.Office.OfficeName.Should().NotBeNullOrEmpty());
                    resourceViewData.ForEach(p => p.Resource.Office.OfficeAbbreviation.Should().NotBeNullOrEmpty());
                    resourceViewData.ForEach(p => p.Resource.SchedulingOffice.Should().NotBeNull());
                    resourceViewData.ForEach(p => p.Resource.SchedulingOffice.OfficeCode.Should().NotBe(0));
                    resourceViewData.ForEach(p => p.Resource.SchedulingOffice.OfficeName.Should().NotBeNullOrEmpty());
                    resourceViewData.ForEach(p => p.Resource.SchedulingOffice.OfficeAbbreviation.Should().NotBeNullOrEmpty());
                    resourceViewData.ForEach(p => p.Resource.FTE.Should().BeGreaterOrEqualTo(0));
                    resourceViewData.ForEach(p => p.Resource.ServiceLine.Should().NotBeNull());
                    resourceViewData.ForEach(p => p.Resource.Position.Should().NotBeNull());
                    resourceViewData.ForEach(p => p.Resource.Position.PositionCode.Should().NotBeNullOrEmpty());
                    resourceViewData.ForEach(p => p.Resource.Position.PositionName.Should().NotBeNullOrEmpty());
                    resourceViewData.ForEach(p => p.Resource.Position.PositionGroupName.Should().NotBeNullOrEmpty());

                    var resourceData = resourceViewData.Where(p => p.Resource.EmployeeCode == "42161").FirstOrDefault();

                    resourceData.Should().NotBeNull();
                    resourceData.Resource.TerminationDate.Should().NotBeNull();
                    resourceData.Resource.ActiveStatus.Should().Be("Terminated");

                    resourceViewData.ForEach(p => p.Allocations?.ToList().ForEach(resource =>
                    {
                        resource.Id.Should().NotBeNull().And.NotBeEmpty();
                        resource.EmployeeCode.Should().NotBeNullOrEmpty();
                        resource.EmployeeName.Should().NotBeNullOrEmpty();
                        resource.InternetAddress.Should().NotBeNullOrEmpty();
                        resource.CurrentLevelGrade.Should().NotBeNullOrEmpty();
                        resource.OperatingOfficeCode.Should().NotBe(0);
                        resource.OperatingOfficeAbbreviation.Should().NotBeNullOrEmpty();
                        resource.Allocation.Should().BeInRange(0, 999);
                        resource.StartDate.Should().BeAfter(DateTime.MinValue);
                        resource.EndDate.Should().BeOnOrAfter(Convert.ToDateTime(resource.StartDate));

                        if (resource.PipelineId == null || resource.PipelineId.Equals(Guid.Empty))
                        {
                            resource.OldCaseCode.Should().NotBeNullOrEmpty();
                            resource.CaseName.Should().NotBeNullOrEmpty();
                            resource.CaseStartDate.Should().NotBeNull();
                            resource.CaseEndDate.Should().NotBeNull();
                            resource.CaseEndDate.Value.Should().BeOnOrAfter(resource.CaseStartDate.Value);
                            resource.CaseTypeCode.Should().NotBeNull();
                        }
                    }));
                }
            }
        }
    }
}
