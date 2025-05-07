using Moq;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Services;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Staffing.HttpAggregator.ViewModels;
using System.Linq;
using FluentAssertions;

namespace Staffing.HttpAggregator.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.HttpAggregator.Services")]
    public class ResourceAllocationServiceTests
    {
        [Theory]
        [InlineData("K3NE")]
        public async Task GetCaseRoleAllocationsByOldCaseCodes_should_return_case_role_allocations(string oldCaseCodes)
        {
            //Arrange
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            mockStaffingAPIClient.Setup(x => x.GetResourceAllocationsByCaseCodes(oldCaseCodes)).ReturnsAsync(GetFakeResourceAllocationsByCaseCode());
            var mockResourceAPIClient = new Mock<IResourceApiClient>();
            mockResourceAPIClient.Setup(x => x.GetEmployees()).ReturnsAsync(GetFakeResources());
            var mockCcmAPIClient = new Mock<ICCMApiClient>();
            var mockPipelineAPIClient = new Mock<IPipelineApiClient>();

            var sut = new ResourceAllocationService(mockStaffingAPIClient.Object, mockResourceAPIClient.Object, mockCcmAPIClient.Object, mockPipelineAPIClient.Object);
            var result = sut.GetCaseRoleAllocationsByOldCaseCodes(oldCaseCodes);
            var resourcesAllocations = result.Result.ToList();
            if (resourcesAllocations.Count() > 0)
            {
                resourcesAllocations.ForEach(emp => emp.EmployeeCode.Should().NotBeNullOrEmpty());
                resourcesAllocations.ForEach(emp => emp.EmployeeName.Should().NotBeNullOrEmpty());
                resourcesAllocations.ForEach(emp => emp.CaseRoleCode.Should().NotBeNullOrEmpty());
                resourcesAllocations.ForEach(emp => emp.OldCaseCode.Should().NotBeNullOrEmpty());
                resourcesAllocations.ForEach(emp => emp.PipelineId.Should().BeNullOrEmpty());
            }
        }

        [Theory]
        [InlineData("44e867df-7eb5-47a1-b31c-4a5f37ef26bb")]
        public async Task GetCaseRoleAllocationsByPipelineIds_should_return_case_role_allocations(string pipelineIds)
        {
            //Arrange
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            mockStaffingAPIClient.Setup(x => x.GetResourceAllocationsByPipelineIds(pipelineIds)).ReturnsAsync(GetFakeResourceAllocationsByPipelineIds());
            var mockResourceAPIClient = new Mock<IResourceApiClient>();
            mockResourceAPIClient.Setup(x => x.GetEmployees()).ReturnsAsync(GetFakeResources());
            var mockCcmAPIClient = new Mock<ICCMApiClient>();
            var mockPipelineAPIClient = new Mock<IPipelineApiClient>();

            var sut = new ResourceAllocationService(mockStaffingAPIClient.Object, mockResourceAPIClient.Object, mockCcmAPIClient.Object, mockPipelineAPIClient.Object);
            var result = sut.GetCaseRoleAllocationsByPipelineIds(pipelineIds);
            var resourcesAllocations = result.Result.ToList();
            if (resourcesAllocations.Count() > 0)
            {
                resourcesAllocations.ForEach(emp => emp.EmployeeCode.Should().NotBeNullOrEmpty());
                resourcesAllocations.ForEach(emp => emp.EmployeeName.Should().NotBeNullOrEmpty());
                resourcesAllocations.ForEach(emp => emp.CaseRoleCode.Should().NotBeNullOrEmpty());
                resourcesAllocations.ForEach(emp => emp.OldCaseCode.Should().BeNullOrEmpty());
                resourcesAllocations.ForEach(emp => emp.PipelineId.Should().NotBeNullOrEmpty());
            }
        }

        private IList<ResourceAssignmentViewModel> GetFakeResourceAllocationsByCaseCode()
        {
            var resourceAllocations = new List<ResourceAssignmentViewModel>
            {
                new ResourceAssignmentViewModel
                {
                    
                    OldCaseCode = "K3NE",
                    PipelineId = null,
                    EmployeeCode = "50265",
                    CurrentLevelGrade = "SC2",
                    OperatingOfficeCode = 127,
                    ServiceLineCode = "SL0004",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2020-04-06T00:00:00"),
                    EndDate = Convert.ToDateTime("2021-01-29T00:00:00"),
                    InvestmentCode = null,
                    InvestmentName = null,
                    CaseRoleCode = "OVP",
                    CaseRoleName = null,
                    LastUpdatedBy = null,
                    Notes = ""
                },
                new ResourceAssignmentViewModel
                {
                    OldCaseCode = "K3NE",
                    PipelineId = null,
                    EmployeeCode = "42537",
                    CurrentLevelGrade = "A4",
                    OperatingOfficeCode = 110,
                    ServiceLineCode = "SL0001",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2020-08-04T00:00:00"),
                    EndDate = Convert.ToDateTime("2020-08-24T00:00:00"),
                    InvestmentCode = null,
                    InvestmentName = null,
                    CaseRoleCode = "SP",
                    CaseRoleName = null,
                    LastUpdatedBy = null,
                    Notes = ""
                },
                new ResourceAssignmentViewModel
                {
                    OldCaseCode = "K3NE",
                    PipelineId = null,
                    EmployeeCode = "50130",
                    CurrentLevelGrade = "A4",
                    OperatingOfficeCode = 110,
                    ServiceLineCode = "SL0001",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2020-08-04T00:00:00"),
                    EndDate = Convert.ToDateTime("2020-08-24T00:00:00"),
                    InvestmentCode = null,
                    InvestmentName = null,
                    CaseRoleCode = "OVP",
                    CaseRoleName = null,
                    LastUpdatedBy = null,
                    Notes = ""
                }
            };
            return resourceAllocations;
        }

        private List<Resource> GetFakeResources()
        {
            var resources = new List<Resource>
            { 
                new Resource
                { 
                    EmployeeCode = "50265",
                    FullName = "Dasgupta, Yagna",
                    SchedulingOffice = new Office
                    { 
                        OfficeCode = 127,
                        OfficeAbbreviation = "NY"
                    }
                    
                },
                new Resource
                {
                    EmployeeCode = "42537",
                    FullName = "Sherman, Sam",
                    SchedulingOffice = new Office
                    {
                        OfficeCode = 360,
                        OfficeAbbreviation = "NDC"
                    }
                }
            };
            return resources;
        }

        private IList<ResourceAssignmentViewModel> GetFakeResourceAllocationsByPipelineIds()
        {
            var resourceAllocations = new List<ResourceAssignmentViewModel>
            {
                new ResourceAssignmentViewModel
                {

                    OldCaseCode = null,
                    PipelineId = new Guid("44e867df-7eb5-47a1-b31c-4a5f37ef26bb"),
                    EmployeeCode = "50265",
                    CurrentLevelGrade = "SC2",
                    OperatingOfficeCode = 127,
                    ServiceLineCode = "SL0004",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2020-04-06T00:00:00"),
                    EndDate = Convert.ToDateTime("2021-01-29T00:00:00"),
                    InvestmentCode = null,
                    InvestmentName = null,
                    CaseRoleCode = "OVP",
                    CaseRoleName = null,
                    LastUpdatedBy = null,
                    Notes = ""
                },
                new ResourceAssignmentViewModel
                {
                    OldCaseCode = null,
                    PipelineId = new Guid("44e867df-7eb5-47a1-b31c-4a5f37ef26bb"),
                    EmployeeCode = "42537",
                    CurrentLevelGrade = "A4",
                    OperatingOfficeCode = 110,
                    ServiceLineCode = "SL0001",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2020-08-04T00:00:00"),
                    EndDate = Convert.ToDateTime("2020-08-24T00:00:00"),
                    InvestmentCode = null,
                    InvestmentName = null,
                    CaseRoleCode = "SP",
                    CaseRoleName = null,
                    LastUpdatedBy = null,
                    Notes = ""
                },
                new ResourceAssignmentViewModel
                {
                    OldCaseCode = null,
                    PipelineId = new Guid("44e867df-7eb5-47a1-b31c-4a5f37ef26bb"),
                    EmployeeCode = "50130",
                    CurrentLevelGrade = "A4",
                    OperatingOfficeCode = 110,
                    ServiceLineCode = "SL0001",
                    Allocation = 100,
                    StartDate = Convert.ToDateTime("2020-08-04T00:00:00"),
                    EndDate = Convert.ToDateTime("2020-08-24T00:00:00"),
                    InvestmentCode = null,
                    InvestmentName = null,
                    CaseRoleCode = "OVP",
                    CaseRoleName = null,
                    LastUpdatedBy = null,
                    Notes = ""
                }
            };
            return resourceAllocations;
        }
    }
}
