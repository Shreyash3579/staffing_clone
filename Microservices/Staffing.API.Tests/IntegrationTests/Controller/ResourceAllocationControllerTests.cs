using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "Staffing.API.ResourceAllocationController")]
    public class ResourceAllocationControllerTests : IClassFixture<TestServerHost>, IDisposable
    {
        private readonly TestServerHost _testServer;
        private readonly IList<Guid?> resourceAllocationIds = new List<Guid?>();

        public ResourceAllocationControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        public void Dispose()
        {
            if (resourceAllocationIds.Count > 0)
            {
                foreach (var resourceAllocationId in resourceAllocationIds)
                {
                    var response = Task.Run(async () => await
                    _testServer.Client.DeleteAsync(
                        $"/api/resourceAllocation?allocationId={resourceAllocationId}&lastUpdatedBy=37995"));
                }
            }

        }

        //[Theory]
        //[ClassData(typeof(ResourceAllocationTestDataGenerator))]
        //public async Task AssignResourceToCase_should_insert_row_in_scheduleMaster(ResourceAllocation[] resourceAllocation)
        //{
        //    //Act
        //    var response = await _testServer.Client.PostAsJsonAsync($"/api/resourceAllocation",
        //        JsonConvert.SerializeObject(resourceAllocation));
        //    response.EnsureSuccessStatusCode();
        //    var responseString = await response.Content.ReadAsStringAsync();
        //    var insertedData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseString);

        //    var assignedResources = insertedData.ToList();
        //    assignedResources.ForEach(allocation =>
        //    {
        //        resourceAllocationIds.Add(allocation.Id);
        //        response.StatusCode.Should().Be(HttpStatusCode.OK);
        //        allocation.Id.Should().NotBe(Guid.Empty);
        //        allocation.OldCaseCode.Should().NotBeNullOrEmpty();
        //        allocation.CaseCode.Should().NotBe(0);
        //        allocation.ClientCode.Should().NotBe(0);
        //        allocation.EmployeeCode.Should().NotBeNullOrEmpty();
        //        allocation.OperatingOfficeCode.Should().NotBe(0);
        //        allocation.CurrentLevelGrade.Should().NotBeNullOrEmpty();
        //        allocation.Allocation.Should().NotBe(0);
        //        allocation.StartDate.Should().NotBe(DateTime.MinValue);
        //        allocation.EndDate.Should().NotBe(DateTime.MinValue);
        //        allocation.LastUpdatedBy.Should().NotBeNullOrEmpty();
        //    });


        //}

        //[Theory]
        //[ClassData(typeof(ResourceAllocationUpdateTestDataGenerator))]
        //public async Task updateResourceAssignment_should_update_row_in_scheduleMaster(ResourceAllocation[] resourceAllocation)
        //{
        //    //Arrange
        //    //INSERT A record
        //    var insertedResponse = await _testServer.Client.PostAsJsonAsync($"/api/resourceAllocation",
        //        JsonConvert.SerializeObject(resourceAllocation));

        //    var responseString = await insertedResponse.Content.ReadAsStringAsync();
        //    var insertedData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseString);
        //    var assignedResources = insertedData.ToList();

        //    resourceAllocationIds.Add(assignedResources[0].Id); //used for deleting resource after tests complete
        //    resourceAllocation[0].Id = assignedResources[0].Id;
        //    resourceAllocation[0].OldCaseCode = "XYZA";
        //    resourceAllocation[0].CaseCode = 1;
        //    resourceAllocation[0].ClientCode = 1110;
        //    resourceAllocation[0].PipelineId = null;
        //    resourceAllocation[0].Allocation = 10;
        //    resourceAllocation[0].EndDate = DateTime.Today.AddDays(-3);

        //    // Act
        //    var response = await _testServer.Client.PutAsJsonAsync($"/api/resourceAllocation/updateResourceAssignment",
        //        JsonConvert.SerializeObject(resourceAllocation[0]));
        //    response.EnsureSuccessStatusCode();

        //    var responseStringUpdated = await response.Content.ReadAsStringAsync();
        //    var updatedData = JsonConvert.DeserializeObject<ResourceAllocation>(responseStringUpdated);

        //    // Assert
        //    response.StatusCode.Should().Be(HttpStatusCode.OK);
        //    updatedData.Id.Should().Be(resourceAllocation[0].Id);
        //    updatedData.ClientCode.Should().Be(resourceAllocation[0].ClientCode);
        //    updatedData.CaseCode.Should().Be(resourceAllocation[0].CaseCode);
        //    updatedData.OldCaseCode.Should().Be(resourceAllocation[0].OldCaseCode);
        //    updatedData.PipelineId.Should().Be(resourceAllocation[0].PipelineId);
        //    updatedData.Allocation.Should().Be(resourceAllocation[0].Allocation);
        //    updatedData.EndDate.Should().Be(resourceAllocation[0].EndDate);
        //}

        //[Theory]
        //[ClassData(typeof(CaseRollTestDataGenerator))]
        //public async Task UpdateResourcesEndDateOnCaseRoll_should_update_row_in_caseRoll(object caseRollData)
        //{
        //    //Act
        //    var response = await _testServer.Client.PutAsJsonAsync($"/api/resourceAllocation/updateResourcesEndDateOnCaseRoll",
        //        caseRollData);

        //    var success = await response.Content.ReadAsAsync<bool>();

        //    response.StatusCode.Should().Be(HttpStatusCode.OK);
        //    success.Should().Be(true);
        //}


        [Theory]
        [MemberData(nameof(ResourceAllocationParameter.SelectedValues), MemberType = typeof(ResourceAllocationParameter))]
        public async Task GetResourceAllocationsBySelectedValues_should_return_resourceAllocations(string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate)
        {
            //Act
            var payload = new { oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate };
            var response =
                await _testServer.Client.PostAsJsonAsync("api/resourceAllocation/allocationsBySelectedValues", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseString);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var assignedResources = actualData.ToList();
            assignedResources.ForEach(allocation =>
            {
                allocation.Id.Should().NotBe(Guid.Empty);
                Assert.True(!string.IsNullOrEmpty(allocation.OldCaseCode) || allocation.PipelineId != Guid.Empty);
                Assert.True((!string.IsNullOrEmpty(allocation.OldCaseCode) && allocation.CaseCode != default(int))
                    || allocation.PipelineId != Guid.Empty);
                allocation.ClientCode.Should().NotBe(0);
                allocation.EmployeeCode.Should().NotBeNullOrEmpty();
                allocation.OperatingOfficeCode.Should().NotBe(0);
                allocation.CurrentLevelGrade.Should().NotBeNullOrEmpty();
                allocation.Allocation.Should().BeGreaterOrEqualTo(0);
                allocation.StartDate.Should().NotBe(DateTime.MinValue);
                allocation.EndDate.Should().NotBe(DateTime.MinValue);
                if (allocation.InvestmentCode != null)
                {
                    allocation.InvestmentCode.Should().NotBe(0);
                    allocation.InvestmentName.Should().NotBeNullOrEmpty();
                }
                if (allocation.CaseRoleCode != null)
                {
                    allocation.CaseRoleCode.Should().NotBeNullOrEmpty();
                    allocation.CaseRoleName.Should().NotBeNullOrEmpty();
                }
                if (oldCaseCodes != null)
                {
                    Assert.Contains(allocation.OldCaseCode.ToUpper(), oldCaseCodes.ToUpper());
                }
                if (employeeCodes != null)
                {
                    Assert.Contains(allocation.EmployeeCode, employeeCodes);
                }
                if (startDate != null && endDate == null)
                {
                    allocation.EndDate.Should().BeOnOrAfter(startDate.Value.Date);
                }
                if (startDate != null && endDate != null)
                {
                    allocation.StartDate.Should().BeOnOrBefore(endDate.Value.Date);
                    allocation.EndDate.Should().BeOnOrAfter(startDate.Value.Date);
                }
                if (lastUpdated != null)
                {
                    allocation.LastUpdated.Should().BeOnOrAfter(lastUpdated.Value.Date);
                }
                allocation.LastUpdated.Should().NotBe(DateTime.MinValue);
            });


        }

        [Theory]
        [InlineData("98585161-3C04-4C36-AA7F-100620AD452F,FD9F4612-FCDF-4CD2-A91C-6AA59EE9EEED")]
        public async Task GetResourceAllocationsByPipelineIds_should_return_resourceAllocations(
            string pipelineIds)
        {

            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/resourceAllocation/allocationsByPipelineIds",
               new { pipelineIds });

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseString);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var assignedResources = actualData.ToList();
            assignedResources.ForEach(allocation =>
            {
                allocation.Id.Should().NotBe(Guid.Empty);
                allocation.PipelineId.Should().NotBe(Guid.Empty);
                allocation.EmployeeCode.Should().NotBeNullOrEmpty();
                allocation.OperatingOfficeCode.Should().NotBe(0);
                allocation.CurrentLevelGrade.Should().NotBeNullOrEmpty();
                allocation.Allocation.Should().BeGreaterOrEqualTo(0);
                allocation.StartDate.Should().NotBe(DateTime.MinValue);
                allocation.EndDate.Should().NotBe(DateTime.MinValue);
            });


        }

        [Theory]
        [InlineData("98585161-3C04-4C36-AA7F-100620AD452F")]
        [InlineData("FD9F4612-FCDF-4CD2-A91C-6AA59EE9EEED")]
        public async Task GetResourceAllocationsByPipelineId_should_return_resourceAllocations(
            string pipelineId)
        {

            //Act
            var response = await _testServer.Client.GetAsync(
                $"/api/resourceAllocation/allocationsByPipelineId?pipelineId={pipelineId}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseString);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var assignedResources = actualData.ToList();
            assignedResources.ForEach(allocation =>
            {
                allocation.Id.Should().NotBe(Guid.Empty);
                allocation.PipelineId.Should().NotBe(Guid.Empty);
                allocation.EmployeeCode.Should().NotBeNullOrEmpty();
                allocation.OperatingOfficeCode.Should().NotBe(0);
                allocation.CurrentLevelGrade.Should().NotBeNullOrEmpty();
                allocation.Allocation.Should().BeGreaterOrEqualTo(0);
                allocation.StartDate.Should().NotBe(DateTime.MinValue);
                allocation.EndDate.Should().NotBe(DateTime.MinValue);
                if (allocation.InvestmentCode != null)
                {
                    allocation.InvestmentCode.Should().NotBe(0);
                    allocation.InvestmentName.Should().NotBeNullOrEmpty();
                }
                if (allocation.CaseRoleCode != null)
                {
                    allocation.CaseRoleCode.Should().NotBeNullOrEmpty();
                    allocation.CaseRoleName.Should().NotBeNullOrEmpty();
                }
            });


        }

        [Theory]
        [InlineData("T5Fk,J3BN")]
        public async Task GetResourceAllocationsByCaseCodes_should_return_resourceAllocations(
           string oldCaseCodes)
        {

            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/resourceAllocation/allocationsByCaseCodes",
               new { oldCaseCodes });

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseString);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var assignedResources = actualData.ToList();
            assignedResources.ForEach(allocation =>
            {
                allocation.Id.Should().NotBe(Guid.Empty);
                allocation.OldCaseCode.Should().NotBeNullOrEmpty();
                allocation.CaseCode.Should().NotBe(0);
                allocation.ClientCode.Should().NotBe(0);
                allocation.EmployeeCode.Should().NotBeNullOrEmpty();
                allocation.OperatingOfficeCode.Should().NotBe(0);
                allocation.CurrentLevelGrade.Should().NotBeNullOrEmpty();
                allocation.Allocation.Should().BeGreaterOrEqualTo(0);
                allocation.StartDate.Should().NotBe(DateTime.MinValue);
                allocation.EndDate.Should().NotBe(DateTime.MinValue);
            });


        }

        [Theory]
        [InlineData("T5Fk")]
        [InlineData("J3BN")]
        public async Task GetResourceAllocationsByCaseCode_should_return_resourceAllocations(
            string oldCaseCode)
        {

            //Act
            var response = await _testServer.Client.GetAsync(
                $"/api/resourceAllocation/allocationsByCaseCode?oldCaseCode={oldCaseCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseString);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var assignedResources = actualData.ToList();
            assignedResources.ForEach(allocation =>
            {
                allocation.Id.Should().NotBe(Guid.Empty);
                allocation.OldCaseCode.Should().NotBeNullOrEmpty();
                allocation.CaseCode.Should().NotBe(0);
                allocation.ClientCode.Should().NotBe(0);
                allocation.EmployeeCode.Should().NotBeNullOrEmpty();
                allocation.OperatingOfficeCode.Should().NotBe(0);
                allocation.CurrentLevelGrade.Should().NotBeNullOrEmpty();
                allocation.Allocation.Should().BeGreaterOrEqualTo(0);
                allocation.StartDate.Should().NotBe(DateTime.MinValue);
                allocation.EndDate.Should().NotBe(DateTime.MinValue);
            });


        }

        [Theory]
        [InlineData("110,150", "2020-01-01", "2020-03-31")]
        public async Task GetResourceAllocationsByOfficeCodes_should_return_resourceAllocations(
           string officeCodes, DateTime startDate, DateTime endDate)
        {
            //Act
            var response = await _testServer.Client.GetAsync(
                $"/api/resourceAllocation/allocationsByOffices?officeCodes={officeCodes}&" +
                $"startDate={startDate}&endDate={endDate}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseString);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var assignedResources = actualData.ToList();
            assignedResources.ForEach(allocation =>
            {
                allocation.Id.Should().NotBe(Guid.Empty);
                Assert.True(!string.IsNullOrEmpty(allocation.OldCaseCode) || allocation.PipelineId != Guid.Empty);
                Assert.True((!string.IsNullOrEmpty(allocation.OldCaseCode) && allocation.CaseCode != default(int))
                    || allocation.PipelineId != Guid.Empty);
                allocation.ClientCode.Should().NotBe(0);
                allocation.EmployeeCode.Should().NotBeNullOrEmpty();
                allocation.OperatingOfficeCode.Should().NotBe(0);
                allocation.Allocation.Should().BeGreaterOrEqualTo(0);
                allocation.StartDate.Should().NotBe(DateTime.MinValue);
                allocation.EndDate.Should().NotBe(DateTime.MinValue);
            });


        }

        [Theory]
        [InlineData("38749,50128", "2020-01-01", "2020-08-30")]
        public async Task GetResourceAllocationsByEmployeeCodes_should_return_resourceAllocations(
           string employeeCodes, string startDate, string endDate)
        {

            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/resourceAllocation/allocationsByEmployees",
               new { employeeCodes, startDate, endDate });

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseString);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var assignedResources = actualData.ToList();
            assignedResources.ForEach(allocation =>
            {
                allocation.Id.Should().NotBe(Guid.Empty);
                Assert.True(!string.IsNullOrEmpty(allocation.OldCaseCode) || allocation.PipelineId != Guid.Empty);
                Assert.True((!string.IsNullOrEmpty(allocation.OldCaseCode) && allocation.CaseCode != default(int))
                    || allocation.PipelineId != Guid.Empty);
                allocation.ClientCode.Should().NotBe(0);
                allocation.EmployeeCode.Should().NotBeNullOrEmpty();
                allocation.OperatingOfficeCode.Should().NotBe(0);
                allocation.CurrentLevelGrade.Should().NotBeNullOrEmpty();
                allocation.Allocation.Should().BeGreaterOrEqualTo(0);
                allocation.StartDate.Should().NotBe(DateTime.MinValue);
                allocation.EndDate.Should().NotBe(DateTime.MinValue);
            });
        }

        [Theory]
        [ClassData(typeof(ResourceAllocationTestDataGenerator))]
        public async Task UpsertResourceAllocations_should_return_resourceAllocations(
        ResourceAllocation[] resourceAllocation)
        {

            //// Act
            //var response = await _testServer.Client.PostAsJsonAsync($"/api/resourceAllocation/upsertResourceAllocations",
            //      JsonConvert.SerializeObject(resourceAllocation));
            //// Assert
            //response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

    }

    public class ResourceAllocationTestDataGenerator : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new ResourceAllocation[]
                {
                    new ResourceAllocation
                    {
                       Id = Guid.NewGuid(),
                        OldCaseCode = "U8UB",
                        CaseCode = 18784,
                        CaseName = "BCC-Staffing Agile Team Costs",
                        ClientCode = 9110,
                        ClientName = "Non-Billable",
                        CaseTypeCode = 2,
                        CaseTypeName = "Non-Billable",
                        EmployeeCode="37995",
                        EmployeeName= "Jain, Nitin",
                        Fte = 1.0M,
                        ServiceLineCode = "SL0001",
                        ServiceLineName = "General Consulting",
                        CurrentLevelGrade="M9",
                        Position = "Lead Software Engineer",
                        OperatingOfficeCode = 332,
                        OperatingOfficeAbbreviation= "NDS",
                        OperatingOfficeName = "New Delhi - BCC",
                        ManagingOfficeCode = 332,
                        ManagingOfficeAbbreviation= "NDS",
                        ManagingOfficeName = "New Delhi - BCC",
                        BillingOfficeCode = 332,
                        BillingOfficeAbbreviation= "NDS",
                        BillingOfficeName = "New Delhi - BCC",
                        Allocation=100,
                        HireDate = new DateTime(2016,01,11),
                        StartDate=new DateTime(2019,01,02),
                        EndDate=new DateTime(2019,01,11),
                        InvestmentCode = 5,
                        InvestmentName = "Client Development",
                        CaseRoleCode = null,
                        CaseRoleName = null,
                        LastUpdatedBy = "37995",
                        Notes = ""}
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CaseRollTestDataGenerator : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new
                {
                    oldCaseCode="H2ES",
                    currentEndDate=new DateTime(2019,06,10),
                    expectedEndDate=new DateTime(2019,07,15),
                    isUpdateEndDateFromCCM = false,
                    lastUpdatedBy = "45088"
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ResourceAllocationUpdateTestDataGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new []
                {
                    new ResourceAllocation
                    {
                        OldCaseCode = "E7MB",
                        CaseCode = 102,
                        CaseName = "Project Atlas",
                        ClientCode = 7471,
                        ClientName = "Goldman Sachs",
                        CaseTypeCode = 1,
                        CaseTypeName = "Billable",
                        PipelineId = null,
                        OpportunityName = null,
                        EmployeeCode = "43902",
                        EmployeeName = "Jain, Nitin",
                        Fte = 0.8M,
                        ServiceLineName = "Traditional Consultant",
                        Position = "Manager",
                        CurrentLevelGrade = "C2",
                        BillCode = 1,
                        OperatingOfficeCode = 110,
                        OperatingOfficeAbbreviation = "BOS",
                        OperatingOfficeName = "Boston",
                        ManagingOfficeCode = 110,
                        ManagingOfficeAbbreviation = "BOS",
                        ManagingOfficeName = "Boston",
                        BillingOfficeCode = 110,
                        BillingOfficeAbbreviation = "BOS",
                        BillingOfficeName = "Boston",
                        Allocation = 80,
                        StartDate = new DateTime(2020, 01, 02),
                        EndDate = new DateTime(2020, 04, 11),
                        InvestmentCode = null,
                        InvestmentName = null,
                        CaseRoleCode = null,
                        CaseRoleName = null,
                        LastUpdatedBy = "39209"
                    },
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ResourceAllocationParameter : TheoryData<string, string, DateTime?, DateTime?, DateTime?>
    {
        public static TheoryData<string, string, DateTime?, DateTime?, DateTime?> SelectedValues =>
            new TheoryData<string, string, DateTime?, DateTime?, DateTime?>
            {
                { "T5Fk,J3BN", null, null, null, null },
                { null, "38749,50128", null, null, null },
                { null, null, DateTime.Now, null, null },
                { null, null, null, DateTime.Today, null },
                { null, null, null, DateTime.Today, DateTime.Today.AddMonths(2) }
            };
    }

}
