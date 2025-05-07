//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Newtonsoft.Json;
//using Staffing.API.ViewModels;
//using Staffing.API.Core.Helpers;
//using Xunit;

//namespace Staffing.API.Tests.IntegrationTests.Controller
//{
//    [Trait("IntegrationTest", "Staffing.API.Controller")]
//    public class CaseControllerTests : IClassFixture<TestServerHost>
//    {
//        private readonly TestServerHost _testServer;

//        public CaseControllerTests(TestServerHost testServer)
//        {
//            _testServer = testServer;
//        }

//        //[Theory]
//        //[InlineData("2018-11-01", "2018-11-01", "110,404")] // 110 -> Boston, 404 -> New Delhi
//        //public async Task GetActiveCasesByOffices_should_return_cases_activeInSpecifiedDateRange(string startDate, string endDate,
//        //    string officeCodes)
//        //{
//        //    //Act
//        //    var response =
//        //        await _testServer.Client.GetAsync(
//        //            $"/api/case/casesbyoffices?startDate={startDate}&endDate={endDate}&officeCodes={officeCodes}");

//        //    response.EnsureSuccessStatusCode();
//        //    var responseString = await response.Content.ReadAsStringAsync();
//        //    var actualActiveCases = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseString).ToList();

//        //    //Assert
//        //    response.StatusCode.Should().Be(HttpStatusCode.OK);
//        //    actualActiveCases.Count().Should().BeGreaterOrEqualTo(0);
//        //    actualActiveCases.ForEach(c => c.ClientCode.Should().NotBe(0));
//        //    actualActiveCases.ForEach(c => c.CaseCode.Should().NotBe(0));
//        //    actualActiveCases.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
//        //    actualActiveCases.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
//        //    actualActiveCases.ForEach(c => c.ClientName.Should().NotBeNullOrEmpty());
//        //    actualActiveCases.ForEach(c => c.StartDate.Should().NotBeAfter(Convert.ToDateTime(endDate)));
//        //    actualActiveCases.ForEach(c => c.EndDate.Should().NotBeBefore(Convert.ToDateTime(startDate)));

//        //    foreach (var item in actualActiveCases){
//        //        if (item.StartDate >= Convert.ToDateTime(startDate))
//        //            item.Type.Should().Be(Constants.NewDemand);
//        //        else
//        //            item.Type.Should().Be(Constants.ActiveCase);
//        //    }

//        //}

//        [Theory]
//        [InlineData("2018-11-01", "2018-11-01", "404", 1, 20)] // 404 -> New Delhi
//        public async Task GetActiveCasesAndAllocationByOffices_should_return_cases_activeInSpecifiedDateRange_and_resourcesAllocatedToThem(string startDate, string endDate,
//            string officeCodes, int offsetStartIndex, int pageSize)
//        {
//            //Act
//            var response =
//                await _testServer.Client.GetAsync(
//                    $"/api/case/casesandallocationsbyoffices?startDate={startDate}&endDate={endDate}&officeCodes={officeCodes}&offsetStartIndex={offsetStartIndex}&pageSize={pageSize}");

//            response.EnsureSuccessStatusCode();
//            var responseString = await response.Content.ReadAsStringAsync();
//            var actualActiveCases = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseString).ToList();

//            //Assert
//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//            actualActiveCases.Count().Should().BeGreaterOrEqualTo(0);
//            actualActiveCases.Count().Should().BeLessOrEqualTo(pageSize);
//            actualActiveCases.ForEach(c => c.ClientCode.Should().NotBe(0));
//            actualActiveCases.ForEach(c => c.CaseCode.Should().NotBe(0));
//            actualActiveCases.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
//            actualActiveCases.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
//            actualActiveCases.ForEach(c => c.ClientName.Should().NotBeNullOrEmpty());
//            actualActiveCases.ForEach(c => c.StartDate.Should().NotBeAfter(Convert.ToDateTime(endDate)));
//            actualActiveCases.ForEach(c => c.EndDate.Should().NotBeBefore(Convert.ToDateTime(startDate)));
//            actualActiveCases.ForEach(c => c.OfficeAbbreviation.Should().NotBeNullOrEmpty());
//            actualActiveCases.ForEach(c => c.OfficeName.Should().NotBeNullOrEmpty());

//            foreach (var item in actualActiveCases)
//            {
//                if (item.StartDate >= Convert.ToDateTime(startDate))
//                    item.Type.Should().Be(Constants.NewDemand);
//                else
//                    item.Type.Should().Be(Constants.ActiveCase);

//                //Below case code has resources allocated to it. Testing it
//                if(item.OldCaseCode == "V5UL")
//                {
//                    item.AllocatedResources.Count().Should().BeGreaterThan(0);
//                    item.AllocatedResources.ToList().ForEach(x => x.OldCaseCode.Should().Be("V5UL"));
//                    item.AllocatedResources.ToList().ForEach(x => x.EmployeeCode.Should().NotBeNullOrEmpty());
//                    item.AllocatedResources.ToList().ForEach(x => x.FullName.Should().NotBeNullOrEmpty());
//                    item.AllocatedResources.ToList().ForEach(x => x.CurrentLevelGrade.Should().NotBeNullOrEmpty());
//                    item.AllocatedResources.ToList().ForEach(x => x.Allocation.Should().NotBe(0));
//                    item.AllocatedResources.ToList().ForEach(x => x.EndDate.Should().BeOnOrAfter(item.StartDate));
//                    item.AllocatedResources.ToList().ForEach(x => x.OfficeAbbreviation.Should().NotBeNullOrEmpty());
//                }
//            }

//        }

//        [Theory]
//        [InlineData("H4VM")]
//        public async Task GetCaseAndAllocationsByCaseCode_should_return_case_and_resourcesAllocated(string oldCaseCode)
//        {
//            //Act
//            var response =
//                await _testServer.Client.GetAsync(
//                    $"/api/case/caseandallocationsbycasecode?oldCaseCode={oldCaseCode}");

//            response.EnsureSuccessStatusCode();
//            var responseString = await response.Content.ReadAsStringAsync();
//            var searchedCase = JsonConvert.DeserializeObject<CaseViewModel>(responseString);

//            //Assert
//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//            searchedCase.OldCaseCode.Should().Be("H4VM");
//            searchedCase.CaseCode.Should().NotBe(0);
//            searchedCase.CaseName.Should().NotBeNullOrEmpty();
//            searchedCase.CaseType.Should().NotBeNullOrEmpty();
//            searchedCase.ClientName.Should().NotBeNullOrEmpty();
//            searchedCase.OfficeAbbreviation.Should().NotBeNullOrEmpty();
//            searchedCase.OfficeName.Should().NotBeNullOrEmpty();
//            searchedCase.EndDate.Should().BeOnOrAfter(searchedCase.StartDate);
//            if (searchedCase.StartDate >= DateTime.Today)
//                searchedCase.Type.Should().Be(Constants.NewDemand);
//            else if(searchedCase.EndDate >= DateTime.Today)
//                searchedCase.Type.Should().Be(Constants.ActiveCase);

//            searchedCase.AllocatedResources.Count().Should().BeGreaterThan(0);
//            searchedCase.AllocatedResources.ToList().ForEach(resource =>
//            {
//                resource.OldCaseCode.Should().Be("H4VM");
//                resource.EmployeeCode.Should().NotBeNullOrEmpty();
//                resource.FullName.Should().NotBeNullOrEmpty();
//                resource.CurrentLevelGrade.Should().NotBeNullOrEmpty();
//                resource.OfficeAbbreviation.Should().NotBeNullOrEmpty();
//                resource.Allocation.Should().NotBe(0);
//                resource.EndDate.Should().BeOnOrAfter(resource.StartDate);
//            });

//        }

//        [Theory]
//        [InlineData("R6EC")]
//        public async Task GetCasesForTypeahead_should_return_case_info(string searchString)
//        {
//            //Act
//            var response =
//                await _testServer.Client.GetAsync(
//                    $"/api/case/typeaheadcases?searchString={searchString}");

//            response.EnsureSuccessStatusCode();
//            var responseString = await response.Content.ReadAsStringAsync();
//            var searchedCase = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseString).ToList();

//            //Assert
//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//            searchedCase.Count().Should().BeGreaterOrEqualTo(0);
//            searchedCase.ForEach(c => c.ClientCode.Should().NotBe(0));
//            searchedCase.ForEach(c => c.CaseCode.Should().NotBe(0));
//            searchedCase.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
//            searchedCase.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
//            searchedCase.ForEach(c => c.ClientName.Should().NotBeNullOrEmpty());
//            searchedCase.ForEach(c => c.OfficeAbbreviation.Should().NotBeNullOrEmpty());
//        }
//    }
//}