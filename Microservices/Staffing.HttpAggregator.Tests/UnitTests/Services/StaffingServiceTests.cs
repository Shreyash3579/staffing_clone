using FluentAssertions;
using Moq;
using Staffing.HttpAggregator.Contracts;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Core.Services;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.HttpAggregator.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.HttpAggregator.Services")]
    public class StaffingServiceTests
    {
        [Theory]
        [InlineData("")]
        public async Task GetResourcesIncludingTerminatedWithAllocationsBySearchString_should_return_emptyList(string searchString)
        {
            //Arrange
            var mockPipelineAPIClient = new Mock<IPipelineApiClient>();
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            var mockCcmAPIClient = new Mock<ICCMApiClient>();
            var mockResourceAPIClient = new Mock<IResourceApiClient>();
            var mockResourceAllocationApiClient = new Mock<IResourceAllocationService>();
            var mockVacationApiClient = new Mock<IVacationApiClient>();
            var mockbvuCDApiClient = new Mock<IBvuCDApiClient>();
            var mockBasisApiClient = new Mock<IBasisApiClient>();
            var mockLookupService = new Mock<ILookupService>();

            var sut = new StaffingService(mockPipelineAPIClient.Object, mockStaffingAPIClient.Object, mockCcmAPIClient.Object,
                mockResourceAPIClient.Object, mockResourceAllocationApiClient.Object, mockVacationApiClient.Object,
                mockbvuCDApiClient.Object, mockBasisApiClient.Object, mockLookupService.Object);

            //Act
            var result = await sut.GetResourcesIncludingTerminatedWithAllocationsBySearchString(searchString);

            //Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("Bartucca")]
        public async Task GetResourcesIncludingTerminatedWithAllocationsBySearchString_should_return_resourceViewList(string searchString)
        {
            //Arrange
            var mockPipelineAPIClient = new Mock<IPipelineApiClient>();
            mockPipelineAPIClient.Setup(x => x.GetOpportunitiesWithTaxonomiesByPipelineIds(It.IsAny<string>()))
                .ReturnsAsync(new List<OpportunityData>());

            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            mockStaffingAPIClient.Setup(x => x.GetResourceAllocationsByEmployeeCodes(It.IsAny<string>(), DateTime.Now.Date, null))
                .ReturnsAsync(new List<ResourceAssignmentViewModel>());
            mockStaffingAPIClient.Setup(x => x.GetInvestmentCategoryList())
                .ReturnsAsync(GetFakeInvestmentCategories().ToList());
            mockStaffingAPIClient.Setup(x => x.GetCaseRoleTypeList())
                .ReturnsAsync(GetFakeCaseRoleType().ToList());

            var mockCcmAPIClient = new Mock<ICCMApiClient>();
            mockCcmAPIClient.Setup(x => x.GetCaseDataByCaseCodes(It.IsAny<string>()))
                .ReturnsAsync(new List<CaseData>());

            var mockResourceAPIClient = new Mock<IResourceApiClient>();
            mockResourceAPIClient.Setup(x => x.GetOffices())
                .ReturnsAsync(GetFakeOffices());
            mockResourceAPIClient.Setup(x => x.GetEmployeesIncludingTerminatedBySearchString(searchString, false))
                .ReturnsAsync(GetFakeSearchedTerminatedResourceData());

            var mockResourceAllocationApiClient = new Mock<IResourceAllocationService>();
            var mockVacationApiClient = new Mock<IVacationApiClient>();
            var mockbvuCDApiClient = new Mock<IBvuCDApiClient>();
            var mockBasisApiClient = new Mock<IBasisApiClient>();
            var mockLookupService = new Mock<ILookupService>();

            var sut = new StaffingService(mockPipelineAPIClient.Object, mockStaffingAPIClient.Object, mockCcmAPIClient.Object,
                mockResourceAPIClient.Object, mockResourceAllocationApiClient.Object, mockVacationApiClient.Object, 
                mockbvuCDApiClient.Object, mockBasisApiClient.Object, mockLookupService.Object);

            //Act
            var result = await sut.GetResourcesIncludingTerminatedWithAllocationsBySearchString(searchString);
            var results = result.ToList();

            //Assert
            results.Should().HaveCount(1);
            result.FirstOrDefault().Resource.Should().BeEquivalentTo(GetFakeTerminatedResourceViewData().FirstOrDefault().Resource);
        }

        #region Methods to get fake data
        private IEnumerable<ResourceView> GetFakeActiveResourceViewData()
        {
            var fakeResourceViewData = new List<ResourceView>
            {
                new ResourceView
                {
                    Resource = new Resource
                    {
                        EmployeeCode = "45088",
                        EmployeeType = "Employee",
                        FirstName = "Dheeraj",
                        LastName = "Upadhyaya",
                        FullName = "Upadhyaya, Dheeraj",
                        LevelGrade = "N6",
                        LevelName = "Software Engineer",
                        BillCode = 0,
                        Status = true,
                        ActiveStatus = "Active",
                        InternetAddress = "Dheeraj.Upadhyaya@Bain.com",
                        StartDate = new DateTime(2018, 10, 10),
                        TerminationDate = null,
                        ProfileImageUrl = "https://gxcdocs.local.bain.com/gxc3/files/Employee_Images/45088.jpg",
                        Office = new Office
                        {
                            OfficeCode = 332,
                            OfficeName = "New Delhi - BCC",
                            OfficeAbbreviation = "NDS"
                        },
                        SchedulingOffice = new Office
                        {
                            OfficeCode = 332,
                            OfficeName = "New Delhi - BCC",
                            OfficeAbbreviation = "NDS"
                        },
                        FTE = 1,
                        ServiceLine = new ServiceLine
                        {
                            ServiceLineCode = "SL0014",
                            ServiceLineName = "Business Services"
                        },
                        Position = new Position
                        {
                            PositionCode = "125325",
                            PositionName = "Software Engineer",
                            PositionGroupName = "General"
                        },
                        PendingTransactions = null
                    },
                    Allocations = new List<ResourceAssignmentViewModel>()
                    {
                        new ResourceAssignmentViewModel
                        {
                            Id = new Guid("264a40f9-7dbb-ea11-a99f-9e45e39de707"),
                            OldCaseCode = "AF57",
                            CaseName = "Dheeraj Realty",
                            PipelineId = null,
                            OpportunityName = null,
                            ClientName = "Non-Billable",
                            CaseTypeCode = Constants.CaseType.ClientDevelopment,
                            EmployeeCode = "45088",
                            EmployeeName = "Upadhyaya, Dheeraj",
                            InternetAddress = "Dheeraj.Upadhyaya@Bain.com",
                            CurrentLevelGrade = "N6",
                            OperatingOfficeCode = 332,
                            OperatingOfficeAbbreviation = "NDS",
                            Allocation = 100,
                            StartDate = new DateTime(2020,07,01),
                            EndDate = new DateTime(2021,07,02),
                            ServiceLineCode = null,
                            InvestmentCode = null,
                            CaseRoleCode = null,
                            PrimaryIndustry = null,
                            PrimaryCapability = null,
                            LastUpdatedBy = null,
                            CaseStartDate = new DateTime(2014,11,03),
                            CaseEndDate = new DateTime(2050,12,31),
                            OpportunityStartDate = null,
                            OpportunityEndDate = null,
                            ProbabilityPercent = null,
                            Notes = ""
                        },
                        new ResourceAssignmentViewModel
                        {
                            Id = new Guid("dfb02f48-79bb-ea11-a99f-9e45e39de707"),
                            OldCaseCode = "K7LD",
                            CaseName = "Retainer Team 3",
                            PipelineId = null,
                            OpportunityName = null,
                            ClientName = "Tiger Global Management",
                            CaseTypeCode = Constants.CaseType.Billable,
                            EmployeeCode = "45088",
                            EmployeeName = "Upadhyaya, Dheeraj",
                            InternetAddress = "Dheeraj.Upadhyaya@Bain.com",
                            CurrentLevelGrade = "N6",
                            OperatingOfficeCode = 332,
                            OperatingOfficeAbbreviation = "NDS",
                            Allocation = 100,
                            StartDate = new DateTime(2020,07,01),
                            EndDate = new DateTime(2020,07,31),
                            ServiceLineCode = null,
                            InvestmentCode = null,
                            CaseRoleCode = null,
                            PrimaryIndustry = null,
                            PrimaryCapability = null,
                            LastUpdatedBy = null,
                            CaseStartDate = new DateTime(2019,04,01),
                            CaseEndDate = new DateTime(2020,07,31),
                            OpportunityStartDate = null,
                            OpportunityEndDate = null,
                            ProbabilityPercent = null,
                            Notes = ""
                        }
                    }
                },
                new ResourceView
                {
                    Resource = new Resource
                    {
                        EmployeeCode = "46426",
                        EmployeeType = "Employee",
                        FirstName = "Dheera",
                        LastName = "Singla",
                        FullName = "Singla, Dheera",
                        LevelGrade = "BC6",
                        LevelName = "BCC Associate",
                        BillCode = 1,
                        Status = true,
                        ActiveStatus = "Active",
                        InternetAddress = "Dheera.Singla@Bain.com",
                        StartDate = new DateTime(2018, 02, 18),
                        TerminationDate = null,
                        ProfileImageUrl = "https://gxcdocs.local.bain.com/gxc3/files/Employee_Images/46426.jpg",
                        Office = new Office
                        {
                          OfficeCode = 332,
                          OfficeName = "New Delhi - BCC",
                          OfficeAbbreviation = "NDS"
                        },
                        SchedulingOffice = new Office
                        {
                          OfficeCode = 332,
                          OfficeName = "New Delhi - BCC",
                          OfficeAbbreviation = "NDS"
                        },
                        FTE = 1,
                        ServiceLine = new ServiceLine
                        {
                            ServiceLineCode = "SL0010",
                            ServiceLineName = "BCN"
                        },
                        Position = new Position
                        {
                            PositionCode = "3320025",
                            PositionName = "BCC Associate",
                            PositionGroupName = "BCN Associate"
                        },
                        PendingTransactions = null
                    },
                    Allocations = new List<ResourceAssignmentViewModel>()
                }
            };
            return fakeResourceViewData;
        }

        private IEnumerable<ResourceView> GetFakeTerminatedResourceViewData()
        {
            var fakeTerminatedResourceViewData = new List<ResourceView>
            {
                new ResourceView
                {
                    Resource = new Resource
                    {
                        EmployeeCode = "42161",
                    EmployeeType = "Employee",
                    FirstName = "Erika",
                    LastName = "Bartucca",
                    FullName = "Bartucca, Erika",
                    LevelGrade = "N6",
                    LevelName = "Scrum Master",
                    BillCode = 0,
                    Status = false,
                    ActiveStatus = "Terminated",
                    InternetAddress = "Erika.Bartucca@Bain.com",
                    StartDate = new DateTime(2017, 09, 25),
                    TerminationDate = new DateTime(2019, 07, 19),
                    ProfileImageUrl = "https://gxcdocs.local.bain.com/gxc3/files/Employee_Images/42161.jpg",
                    Office = new Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "BOS"
                    },
                    SchedulingOffice = new Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "BOS"
                    },
                    FTE = 1,
                    ServiceLine = new ServiceLine
                    {
                        ServiceLineCode = null,
                        ServiceLineName = null
                    },
                    Position = new Position
                    {
                        PositionCode = "126216",
                        PositionName = "Scrum Master",
                        PositionGroupName = "General"
                    },
                    PendingTransactions = null
                    },
                    Allocations = new List<ResourceAssignmentViewModel>()
                }
            };
            return fakeTerminatedResourceViewData;
        }

        private IEnumerable<Resource> GetFakeSearchedActiveResourceData()
        {
            var fakeSearchedResourceData = new List<Resource>
            {
                new Resource
                {
                    EmployeeCode = "45088",
                    EmployeeType = "Employee",
                    FirstName = "Dheeraj",
                    LastName = "Upadhyaya",
                    FullName = "Upadhyaya, Dheeraj",
                    LevelGrade = "N6",
                    LevelName = "Software Engineer",
                    BillCode = 0,
                    Status = true,
                    ActiveStatus = "Active",
                    InternetAddress = "Dheeraj.Upadhyaya@Bain.com",
                    StartDate = new DateTime(2018, 10, 10),
                    TerminationDate = null,
                    ProfileImageUrl = "https://gxcdocs.local.bain.com/gxc3/files/Employee_Images/45088.jpg",
                    Office = new Office
                    {
                        OfficeCode = 332,
                        OfficeName = "New Delhi - BCC",
                        OfficeAbbreviation = "NDS"
                    },
                    SchedulingOffice = new Office
                    {
                        OfficeCode = 332,
                        OfficeName = "New Delhi - BCC",
                        OfficeAbbreviation = "NDS"
                    },
                    FTE = 1,
                    ServiceLine = new ServiceLine
                    {
                        ServiceLineCode = "SL0014",
                        ServiceLineName = "Business Services"
                    },
                    Position = new Position
                    {
                        PositionCode = "125325",
                        PositionName = "Software Engineer",
                        PositionGroupName = "General"
                    },
                    PendingTransactions = null
                },
                new Resource
                {
                    EmployeeCode = "46426",
                    EmployeeType = "Employee",
                    FirstName = "Dheera",
                    LastName = "Singla",
                    FullName = "Singla, Dheera",
                    LevelGrade = "BC6",
                    LevelName = "BCC Associate",
                    BillCode = 1,
                    Status = true,
                    ActiveStatus = "Active",
                    InternetAddress = "Dheera.Singla@Bain.com",
                    StartDate = new DateTime(2018, 02, 18),
                    TerminationDate = null,
                    ProfileImageUrl = "https://gxcdocs.local.bain.com/gxc3/files/Employee_Images/46426.jpg",
                    Office = new Office
                    {
                      OfficeCode = 332,
                      OfficeName = "New Delhi - BCC",
                      OfficeAbbreviation = "NDS"
                    },
                    SchedulingOffice = new Office
                    {
                      OfficeCode = 332,
                      OfficeName = "New Delhi - BCC",
                      OfficeAbbreviation = "NDS"
                    },
                    FTE = 1,
                    ServiceLine = new ServiceLine
                    {
                        ServiceLineCode = "SL0010",
                        ServiceLineName = "BCN"
                    },
                    Position = new Position
                    {
                        PositionCode = "3320025",
                        PositionName = "BCC Associate",
                        PositionGroupName = "BCN Associate"
                    },
                    PendingTransactions = null
                }
            };
            return fakeSearchedResourceData;
        }

        private IEnumerable<Resource> GetFakeSearchedTerminatedResourceData()
        {
            var fakeSearchedResourceData = new List<Resource>
            {
                new Resource
                {
                    EmployeeCode = "42161",
                    EmployeeType = "Employee",
                    FirstName = "Erika",
                    LastName = "Bartucca",
                    FullName = "Bartucca, Erika",
                    LevelGrade = "N6",
                    LevelName = "Scrum Master",
                    BillCode = 0,
                    Status = false,
                    ActiveStatus = "Terminated",
                    InternetAddress = "Erika.Bartucca@Bain.com",
                    StartDate = new DateTime(2017, 09, 25),
                    TerminationDate = new DateTime(2019, 07, 19),
                    ProfileImageUrl = "https://gxcdocs.local.bain.com/gxc3/files/Employee_Images/42161.jpg",
                    Office = new Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "BOS"
                    },
                    SchedulingOffice = new Office
                    {
                        OfficeCode = 110,
                        OfficeName = "Boston",
                        OfficeAbbreviation = "BOS"
                    },
                    FTE = 1,
                    ServiceLine = new ServiceLine
                    {
                        ServiceLineCode = null,
                        ServiceLineName = null
                    },
                    Position = new Position
                    {
                        PositionCode = "126216",
                        PositionName = "Scrum Master",
                        PositionGroupName = "General"
                    },
                    PendingTransactions = null
                }
            };
            return fakeSearchedResourceData;
        }

        private IEnumerable<ResourceAssignmentViewModel> GetFakeSearchedResourceAllocationData()
        {
            var fakeSearchedResourceData = new List<ResourceAssignmentViewModel>
            {
                new ResourceAssignmentViewModel
                {
                    Id = new Guid("264a40f9-7dbb-ea11-a99f-9e45e39de707"),
                    OldCaseCode = "AF57",
                    PipelineId = null,
                    EmployeeCode = "45088",
                    CurrentLevelGrade = "N6",
                    OperatingOfficeCode = 332,
                    ServiceLineCode = null,
                    Allocation = 100,
                    StartDate = new DateTime(2020,07,01),
                    EndDate = new DateTime(2021,07,02),
                    InvestmentCode = null,
                    CaseRoleCode = null,
                    LastUpdatedBy = null,
                    Notes = ""
                },
                new ResourceAssignmentViewModel
                {
                    Id = new Guid("dfb02f48-79bb-ea11-a99f-9e45e39de707"),
                    OldCaseCode = "K7LD",
                    PipelineId = null,
                    EmployeeCode = "45088",
                    CurrentLevelGrade = "N6",
                    OperatingOfficeCode = 332,
                    ServiceLineCode = null,
                    Allocation = 100,
                    StartDate = new DateTime(2020,07,01),
                    EndDate = new DateTime(2020,07,31),
                    InvestmentCode = null,
                    CaseRoleCode = null,
                    LastUpdatedBy = null,
                    Notes = ""
                }
            };
            return fakeSearchedResourceData;
        }

        private IEnumerable<Office> GetFakeOffices()
        {
            var fakeOffices = new List<Office>
            {
                new Office
                {
                    OfficeCode = 332,
                    OfficeName = "New Delhi - BCC",
                    OfficeAbbreviation = "NDS"
                }
            };
            return fakeOffices;
        }

        private IEnumerable<InvestmentCategory> GetFakeInvestmentCategories()
        {
            var fakeInvestmentCategory = new List<InvestmentCategory>
            {
                new InvestmentCategory
                {
                  InvestmentCode = 5,
                  InvestmentName = "Internal PD",
                  InvestmentDescription = "Allocation driven by individual PD needs or office policy, e.g., new joiner staffed for free",
                  Precedence = 1
                },
                new InvestmentCategory
                {
                  InvestmentCode = 2,
                  InvestmentName = "Client Variable",
                  InvestmentDescription = "Can be changed by staffing and tied to a particular individual, i.e., additional resources staffed at case partner's request to get the job done",
                  Precedence = 2
                },
                new InvestmentCategory
                {
                  InvestmentCode = 4,
                  InvestmentName = "Pre/Post Revenue",
                  InvestmentDescription = "Specific individuals held before, in between, or after paid workstreams to secure future revenue (allocation extends outside of BASIS case dates)",
                  Precedence = 3
                },
                new InvestmentCategory
                {
                  InvestmentCode = 12,
                  InvestmentName = "Backfill",
                  InvestmentDescription = "Backfill",
                  Precedence = 4
                }
            };
            return fakeInvestmentCategory;
        }

        private IEnumerable<CaseRoleType> GetFakeCaseRoleType()
        {
            var fakeCaseRoleType = new List<CaseRoleType>
            {
                new CaseRoleType
                {
                    CaseRoleCode = "AD",
                    CaseRoleName = "Advisor"
                },
                new CaseRoleType
                {
                    CaseRoleCode = "OVP",
                    CaseRoleName = "Operating VP"
                },
                new CaseRoleType
                {
                    CaseRoleCode = "SP",
                    CaseRoleName = "Senior Partner"
                }
            };
            return fakeCaseRoleType;
        }

        private IEnumerable<CaseData> GetFakeCaseData()
        {
            var fakeCaseData = new List<CaseData>
            {
                new CaseData
                {
                    CaseCode = 143,
                    CaseName = "Retainer Team 3",
                    ClientCode = 7893,
                    ClientName = "Tiger Global Management",
                    OldCaseCode = "K7LD",
                    CaseTypeCode = 1,
                    CaseType = "Billable",
                    ManagingOfficeCode = 404,
                    ManagingOfficeAbbreviation = "NDC",
                    ManagingOfficeName = "New Delhi",
                    BillingOfficeCode = 125,
                    BillingOfficeAbbreviation = "SFR",
                    BillingOfficeName = "San Francisco",
                    StartDate = new DateTime(2019,04,01),
                    EndDate = new DateTime(2020,07,31),
                    PrimaryIndustry = null,
                    PrimaryCapability = null,
                    IsPrivateEquity = false,
                    CaseAttributes = null,
                    Type = "ActiveCase"
                },
                new CaseData
                {
                    CaseCode = 11126,
                    CaseName = "Dheeraj Realty",
                    ClientCode = 9110,
                    ClientName = "Non-Billable",
                    OldCaseCode = "AF57",
                    CaseTypeCode = 4,
                    CaseType = "Client Development",
                    ManagingOfficeCode = 404,
                    ManagingOfficeAbbreviation = "NDC",
                    ManagingOfficeName = "New Delhi",
                    BillingOfficeCode = 404,
                    BillingOfficeAbbreviation = "NDC",
                    BillingOfficeName = "New Delhi",
                    StartDate = new DateTime(2014,11,03),
                    EndDate = new DateTime(2050,12,31),
                    PrimaryIndustry = null,
                    PrimaryCapability = null,
                    IsPrivateEquity = false,
                    CaseAttributes = null,
                    Type = "ActiveCase"
                }
            };
            return fakeCaseData;
        }

        #endregion
    }
}
