using FluentAssertions;
using Moq;
using Staffing.HttpAggregator.Contracts.Services;
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
    public class ResourceServiceTests
    {
        [Theory]
        [InlineData("", "2019-10-31", "2019-12-31", "", "", "Office Codes can not be null")]
        [InlineData("404", "1/1/0001", "2019-12-31", "", "", "Start Date can not be null")]
        [InlineData("404", "2019-10-31", "1/1/0001", "", "", "End Date can not be null")]
        [InlineData("404", "2019-12-31", "2019-10-31", "", "", "End date should be greater than start date")]
        public async Task GetResourcesFilteredBySelectedValues_should_return_exceptions(string officeCodes,
            DateTime startDate, DateTime endDate, string levelGrades, string staffingTags, string errorMessage)
        {
            //Arrange
            var mockResourceAPIClient = new Mock<IResourceApiClient>();
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            var mockVacationAPIClient = new Mock<IVacationApiClient>();
            var mockBvuCDAPIClient = new Mock<IBvuCDApiClient>();
            var mockHcpdApiService = new Mock<IHcpdApiClient>();
            var mockResourceAllocationService = new Mock<IResourceAllocationService>();
            var mockBasisApiService = new Mock<IBasisApiClient>();
            var supplyFilterCriteria = GetSupplyFilterCriteriaTestObject(officeCodes, startDate, endDate, levelGrades, staffingTags);

            var sut = new ResourceService(mockResourceAPIClient.Object, mockResourceAllocationService.Object, mockVacationAPIClient.Object,
                mockBvuCDAPIClient.Object, mockStaffingAPIClient.Object, mockHcpdApiService.Object, mockBasisApiService.Object);

            //Act
            var exception = await Record.ExceptionAsync(
                () => sut.GetResourcesFilteredBySelectedValues(supplyFilterCriteria)
            );

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("41TBR")]
        public async Task GetAdvisorNameByEmployeeCode_should_return_advisor_name(string employeeCode)
        {
            //Arrange
            var mockResourceAPIClient = new Mock<IResourceApiClient>();
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            var mockVacationAPIClient = new Mock<IVacationApiClient>();
            var mockBvuCDAPIClient = new Mock<IBvuCDApiClient>();
            var mockHcpdApiClient = new Mock<IHcpdApiClient>();
            var mockBasisApiService = new Mock<IBasisApiClient>();

            mockHcpdApiClient.Setup(x => x.GetAdvisorByEmployeeCode(employeeCode)).ReturnsAsync(GetFakeAdvisorDetails());
            mockResourceAPIClient.Setup(x => x.GetEmployeesIncludingTerminated()).ReturnsAsync(GetFakeResourceForAdvisor());
            var mockResourceAllocationService = new Mock<IResourceAllocationService>();
            var sut = new ResourceService(mockResourceAPIClient.Object, mockResourceAllocationService.Object, mockVacationAPIClient.Object,
                mockBvuCDAPIClient.Object, mockStaffingAPIClient.Object, mockHcpdApiClient.Object, mockBasisApiService.Object);

            //Act
            var result = await sut.GetAdvisorNameByEmployeeCode(employeeCode);

            //Assert
            result.Should().NotBeNull();
            result.FullName.Should().NotBeNull();
            result.FullName.Length.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData("")]
        public async Task GetAdvisorNameByEmployeeCode_should_return_empty_advisor_name(string employeeCode)
        {
            //Arrange
            var mockResourceAPIClient = new Mock<IResourceApiClient>();
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            var mockVacationAPIClient = new Mock<IVacationApiClient>();
            var mockBvuCDAPIClient = new Mock<IBvuCDApiClient>();
            var mockHcpdApiClient = new Mock<IHcpdApiClient>();
            var mockBasisApiService = new Mock<IBasisApiClient>();

            mockHcpdApiClient.Setup(x => x.GetAdvisorByEmployeeCode(employeeCode)).ReturnsAsync(GetFakeEmptyAdvisorDetails());
            var mockResourceAllocationService = new Mock<IResourceAllocationService>();
            var sut = new ResourceService(mockResourceAPIClient.Object, mockResourceAllocationService.Object, mockVacationAPIClient.Object,
                mockBvuCDAPIClient.Object, mockStaffingAPIClient.Object, mockHcpdApiClient.Object, mockBasisApiService.Object);

            //Act
            var result = await sut.GetAdvisorNameByEmployeeCode(employeeCode);

            //Assert
            result.Should().NotBeNull();
            result.FullName.Should().BeNull();
        }

        [Theory]
        [InlineData("404", "2019-10-31", "2019-12-31", "", "SL0022")]
        [InlineData("404", "2019-10-31", "2019-12-31", "", "SL0001,SL0022")]
        public async Task GetResourcesFilteredBySelectedValues_should_return_filteredResources(string officeCodes,
            DateTime startDate, DateTime endDate, string levelGrades, string staffingTags)
        {
            //Arrange
            var mockResourceAPIClient = new Mock<IResourceApiClient>();
            var mockHcpdApiService = new Mock<IHcpdApiClient>();
            var mockBasisApiService = new Mock<IBasisApiClient>();
            mockResourceAPIClient.Setup(x => x.GetActiveEmployeesFilteredBySelectedValues(officeCodes, startDate, endDate, levelGrades, staffingTags))
                .ReturnsAsync(GetFakeResources());
            var mockStaffingAPIClient = new Mock<IStaffingApiClient>();
            mockStaffingAPIClient.Setup(x => x.GetCommitmentsWithinDateRange(startDate, endDate))
                .ReturnsAsync(GetFakeCommitments());
            var mockResourceAllocationService = new Mock<IResourceAllocationService>();
            mockResourceAllocationService.Setup(x => x.GetResourceAllocationsByEmployeeCodes(It.IsAny<string>(), startDate, endDate))
                .ReturnsAsync(Enumerable.Empty<ResourceAssignmentViewModel>());
            var mockVacationAPIClient = new Mock<IVacationApiClient>();
            mockVacationAPIClient.Setup(x => x.GetVacationsWithinDateRangeByEmployeeCodes(It.IsAny<string>(), startDate, endDate))
               .ReturnsAsync(Enumerable.Empty<VacationRequestViewModel>());
            var mockBvuCDAPIClient = new Mock<IBvuCDApiClient>();
            mockBvuCDAPIClient.Setup(x => x.GetTrainingsWithinDateRangeByEmployeeCodes(It.IsAny<string>(), startDate, endDate))
               .ReturnsAsync(Enumerable.Empty<TrainingViewModel>());

            var supplyFilterCriteria = GetSupplyFilterCriteriaTestObject(officeCodes, startDate, endDate, levelGrades, staffingTags);

            var sut = new ResourceService(mockResourceAPIClient.Object, mockResourceAllocationService.Object, mockVacationAPIClient.Object,
                mockBvuCDAPIClient.Object, mockStaffingAPIClient.Object, mockHcpdApiService.Object, mockBasisApiService.Object);
            var offices = officeCodes.Split(",");
            var staffingTagCodes = staffingTags.Split(",");


            //Act
            var result = await sut.GetResourcesFilteredBySelectedValues(supplyFilterCriteria);
            var results = result.Resources.ToList();

            //Assert
            results.Count.Should().BeGreaterOrEqualTo(0);
            results.ToList().ForEach(c => c.EmployeeCode.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.FirstName.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.LastName.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.FullName.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.LevelGrade.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.LevelName.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.InternetAddress.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.StartDate.Should().BeAfter(DateTime.MinValue));
            results.ToList().ForEach(c => c.TerminationDate?.Should().BeAfter(DateTime.MinValue));
            results.ToList().ForEach(c => c.ProfileImageUrl.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.Office.Should().BeOfType<Office>());
            results.ToList().ForEach(c => c.Office.OfficeCode.Should().NotBe(0));
            results.ToList().ForEach(c => c.Office.OfficeAbbreviation.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.Office.OfficeName.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.SchedulingOffice.Should().BeOfType<Office>());
            results.ToList().ForEach(c => c.SchedulingOffice.OfficeCode.ToString().Should().BeOneOf(offices));
            results.ToList().ForEach(c => c.SchedulingOffice.OfficeAbbreviation.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.SchedulingOffice.OfficeName.Should().NotBeNullOrEmpty());
            results.ToList().ForEach(c => c.FTE.Should().BeInRange(0, 1));
            results.ToList().ForEach(c => c.ServiceLine.Should().NotBeNull());
            if (staffingTagCodes.Contains("SL0001")) //SL0001 -> Traditional Consulting
            {
                //01SAT has Traditional Consulting as the service as the service line but it has been commited to PEG for this date range.
                if (!staffingTagCodes.Contains("P")) //P -> PEG Commitment
                    results.ToList().ForEach(c => c.EmployeeCode.Should().NotBe("01SAT"));

                //If only traditional consulting is selected, then only resources with service line as traditional consulting should be in the returned list.
                if (staffingTagCodes.Length == 1)
                {
                    results.ToList().ForEach(c => c.ServiceLine.ServiceLineCode.Should().BeOneOf(staffingTagCodes));
                }
            }

            //42PSA is an employee with service line as Traditional Consulting but he has been commited to AAG for selected date range.
            if (staffingTagCodes.Contains("SL0022")) //SL0022 -> AAG
            {
                results.ToList().Exists(c => c.EmployeeCode.Equals("42PSA"));
            }
        }

        private List<Resource> GetFakeResourceForAdvisor()
        {
            var fakeResource = new List<Resource>
            {
                new Resource
                {
                    EmployeeCode = "38CRR",
                    FirstName = "Coleman",
                    LastName = "Radell"
                }
            };

            return fakeResource;
        }
        private Advisor GetFakeAdvisorDetails()
        {
            return new Advisor
            {
                EmployeeCode = "41TBR",
                AdvisorEmployeeCode = "38CRR"
            };
        }

        private Advisor GetFakeEmptyAdvisorDetails()
        {
            Advisor advisor = null;
            return advisor;
        }
        private SupplyFilterCriteria GetSupplyFilterCriteriaTestObject(string officeCodes,
            DateTime startDate, DateTime endDate, string levelGrades, string staffingTags)
        {
            var supplyFilterCriteria = new SupplyFilterCriteria
            {
                StartDate = startDate,
                EndDate = endDate,
                OfficeCodes = officeCodes,
                LevelGrades = levelGrades,
                StaffingTags = staffingTags
            };

            return supplyFilterCriteria;
        }
        private IEnumerable<Resource> GetFakeResources()
        {
            var fakeResources = new List<Resource>
            {
                new Resource
                {
                    EmployeeCode = "01SAT",
                    FirstName = "Amit",
                    LastName = "Sinha",
                    FullName = "Sinha, Amit",
                    LevelGrade = "V1",
                    LevelName = "Vice President",
                    Status = true,
                    InternetAddress = "amit.sinha@bain.com",
                    StartDate = Convert.ToDateTime("2001-06-18T03:00:00-04:00"),
                    ProfileImageUrl = "http://gxcdocs.local.bain.com/gxc3/files/Employee_Images/01SAT.jpg",
                    Office = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    SchedulingOffice = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    FTE = 1,
                    ServiceLine = new ServiceLine()
                    {
                        ServiceLineCode = "SL0001",
                        ServiceLineName = "Traditional Consulting"
                    }
                },
                new Resource
                {
                    EmployeeCode = "01SGH",
                    FirstName = "Karan",
                    LastName = "Singh",
                    FullName = "Singh, Karan",
                    LevelGrade = "V3",
                    LevelName = "Director",
                    Status = true,
                    InternetAddress = "karan.singh@bain.com",
                    StartDate = Convert.ToDateTime("1998-08-03T03:00:00-04:00"),
                    ProfileImageUrl = "http://gxcdocs.local.bain.com/gxc3/files/Employee_Images/01SGH.jpg",
                    Office = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    SchedulingOffice = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    FTE = 1,
                    ServiceLine = new ServiceLine()
                    {
                        ServiceLineCode = "SL0001",
                        ServiceLineName = "Traditional Consulting"
                    }
                },
                new Resource
                {
                    EmployeeCode = "41129",
                    FirstName = "Sharad",
                    LastName = "Kamal",
                    FullName = "Kamal, Sharad",
                    LevelGrade = "TG8",
                    LevelName = "Senior Specialist, Data Science",
                    Status = true,
                    InternetAddress = "Sharad.Kamal@Bain.com",
                    StartDate = Convert.ToDateTime("2017-04-03T03:00:00-04:00"),
                    ProfileImageUrl = "http://gxcdocs.local.bain.com/gxc3/files/Employee_Images/41129.jpg",
                    Office = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    SchedulingOffice = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    FTE = 0,
                    ServiceLine = new ServiceLine()
                    {
                        ServiceLineCode = "SL0022",
                        ServiceLineName = "AAG"
                    }
                },
                new Resource
                {
                    EmployeeCode = "42PSA",
                    FirstName = "Prashant",
                    LastName = "Sarin",
                    FullName = "Sarin, Prashant",
                    LevelGrade = "V1",
                    LevelName = "Vice President",
                    Status = true,
                    InternetAddress = "Prashant.Sarin@Bain.com",
                    StartDate = Convert.ToDateTime("2006-05-29T03:00:00-04:00"),
                    ProfileImageUrl = "http://gxcdocs.local.bain.com/gxc3/files/Employee_Images/42PSA.jpg",
                    Office = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    SchedulingOffice = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    FTE = 0,
                    ServiceLine = new ServiceLine()
                    {
                        ServiceLineCode = "SL0001",
                        ServiceLineName = "Traditional Consulting"
                    }
                },
                new Resource
                {
                    EmployeeCode = "42830",
                    FirstName = "Rohan",
                    LastName = "Rajhans",
                    FullName = "Rajhans, Rohan",
                    LevelGrade = "TT8",
                    LevelName = "Senior Product Designer",
                    Status = true,
                    InternetAddress = "Rohan.Rajhans@Bain.com",
                    StartDate = Convert.ToDateTime("2017-12-13T03:00:00-05:00"),
                    ProfileImageUrl = "http://gxcdocs.local.bain.com/gxc3/files/Employee_Images/42830.jpg",
                    Office = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    SchedulingOffice = new Office()
                    {
                        OfficeCode = 404,
                        OfficeName = "New Delhi",
                        OfficeAbbreviation="NDC"
                    },
                    FTE = 0,
                    ServiceLine = new ServiceLine()
                    {
                        ServiceLineCode = "SL0006",
                        ServiceLineName = "ADAPT"
                    }
                }
            };
            return fakeResources;
        }

        private IEnumerable<CommitmentViewModel> GetFakeCommitments()
        {
            var fakeCommitments = new List<CommitmentViewModel>
            {
                new CommitmentViewModel
                {
                    EmployeeCode = "42PSA",
                    CommitmentTypeCode = "SL0022",
                    StartDate = Convert.ToDateTime("2019-10-15T00:00:00"),
                    EndDate = Convert.ToDateTime("2019-11-03T00:00:00"),
                    Description = "AAG "
                },
                new CommitmentViewModel
                {
                    EmployeeCode = "45360",
                    CommitmentTypeCode = "SL0022",
                    StartDate = Convert.ToDateTime("2019-11-10T00:00:00"),
                    EndDate = Convert.ToDateTime("2019-12-01T00:00:00"),
                    Description = ""
                },
                new CommitmentViewModel
                {
                    EmployeeCode = "42AUM",
                    CommitmentTypeCode = "SL0022",
                    StartDate = Convert.ToDateTime("2019-10-25T00:00:00"),
                    EndDate = Convert.ToDateTime("2019-10-31T00:00:00"),
                    Description = null
                },
                new CommitmentViewModel
                {
                    EmployeeCode = "01SAT",
                    CommitmentTypeCode = "P",
                    StartDate = Convert.ToDateTime("2017-03-28T11:50:00"),
                    EndDate = Convert.ToDateTime("2019-12-31T11:50:00"),
                    Description = null
                },
                new CommitmentViewModel
                {
                    EmployeeCode = "42381",
                    CommitmentTypeCode = "P",
                    StartDate = Convert.ToDateTime("2018-11-06T08:50:00"),
                    EndDate = Convert.ToDateTime("2020-01-01T08:50:00"),
                    Description = null
                },
                new CommitmentViewModel
                {
                    EmployeeCode = "38711",
                    CommitmentTypeCode = "P",
                    StartDate = Convert.ToDateTime("2018-11-12T12:12:00"),
                    EndDate = Convert.ToDateTime("2019-11-30T12:12:00"),
                    Description = null
                }
            };
            return fakeCommitments;
        }

    }
}
