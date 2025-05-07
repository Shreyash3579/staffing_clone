using CCM.API.ViewModels;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace CCM.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "CCM.API.CaseController")]
    public class CaseControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public CaseControllerTests(TestServerHost testServerHost)
        {
            _testServer = testServerHost;
        }       

        [Theory]
        [InlineData("110,112", "1,2,3", 1, 20)] // 110 -> Boston, 112 -> New York
        public async Task GetNewDemandCasesByOffices_should_return_newDemandCases(
            string officeCodes, string caseTypeCodes, int offsetStartIndex, int pageSize)
        {
            //set input variables here as inline data is not usable after some time
            string startDate = DateTime.Today.ToShortDateString();
            string endDate = DateTime.Today.AddDays(14).ToShortDateString();
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/case/newDemandCasesByOffices?startDate={startDate}&endDate={endDate}&officeCodes={officeCodes}" +
                    $"&caseTypeCodes={caseTypeCodes}&offsetStartIndex={offsetStartIndex}&pageSize={pageSize}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var newDemands = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseString).ToList();
            var expectedOrderedCases = newDemands.OrderBy(x => x.StartDate);
            int countBostonCases = newDemands.Count(c => c.ManagingOfficeAbbreviation == "BOS");
            int countNewYorkCases = newDemands.Count(c => c.ManagingOfficeAbbreviation == "NYC");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            newDemands.Count().Should().BeGreaterOrEqualTo(0);
            newDemands.Count.Should().Be(countBostonCases + countNewYorkCases);
            newDemands.ForEach(c => c.CaseCode.Should().NotBe(0));
            newDemands.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
            newDemands.ForEach(c => c.ClientCode.Should().NotBe(0));
            newDemands.ForEach(c => c.ClientName.Should().NotBeNullOrEmpty());
            newDemands.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
            newDemands.ForEach(c => c.CaseType.Should().NotBeNullOrEmpty());
            newDemands.ForEach(c => c.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty());
            newDemands.ForEach(c => c.StartDate.Should().NotBeBefore(Convert.ToDateTime(startDate)));
            newDemands.ForEach(c => Assert.True(c.EndDate == DateTime.MinValue || c.EndDate != DateTime.MinValue && c.EndDate >= c.StartDate));
            newDemands.ForEach(c => c.Type.Should().Be("NewDemand"));
            expectedOrderedCases.SequenceEqual(newDemands).Should().BeTrue(); //test ordering of elements
        }

        [Theory]
        [InlineData("110,112", "1,2,3", 1, 20)] // 110 -> Boston, 112 -> New York
        public async Task GetActiveCasesExceptNewDemandsByOffices_should_return_activeCasesExceptNewDemands(
            string officeCodes, string caseTypeCodes, int offsetStartIndex, int pageSize)
        {
            //set input variables here as inline data is not usable after some time
            string startDate = DateTime.Today.ToShortDateString();
            string endDate = DateTime.Today.AddDays(14).ToShortDateString();
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/case/activeCasesExceptNewDemandsByOffices?startDate={startDate}&endDate={endDate}&officeCodes={officeCodes}" +
                    $"&caseTypeCodes={caseTypeCodes}&offsetStartIndex={offsetStartIndex}&pageSize={pageSize}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var activeCases = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseString).ToList();
            var expectedOrderedCases = activeCases.OrderBy(x => x.EndDate);
            int countBostonCases = activeCases.Count(c => c.ManagingOfficeAbbreviation == "BOS");
            int countNewYorkCases = activeCases.Count(c => c.ManagingOfficeAbbreviation == "NYC");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            activeCases.Count().Should().BeGreaterOrEqualTo(0);
            activeCases.Count.Should().Be(countBostonCases + countNewYorkCases);
            activeCases.ForEach(c => c.CaseCode.Should().NotBe(0));
            activeCases.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.ClientCode.Should().NotBe(0));
            activeCases.ForEach(c => c.ClientName.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.CaseType.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.StartDate.Should().NotBeAfter(Convert.ToDateTime(startDate)));
            activeCases.ForEach(c => c.EndDate.Should().NotBeBefore(c.StartDate));
            activeCases.ForEach(c => c.Type.Should().Be("ActiveCase"));
            //expectedOrderedCases.SequenceEqual(activeCases).Should().BeTrue(); //test ordering of elements
        }

        //[Theory]
        //[InlineData("H4VM,C8MR")]
        //public async Task GetCaseDetailsByCaseCodes_should_return_caseDetails(string oldCaseCodes)
        //{
        //    //Act
        //    var response =
        //        await _testServer.Client.GetAsync(
        //            $"/api/case/caseDetailsByCaseCodes?oldCaseCodes={oldCaseCodes}");

        //    response.EnsureSuccessStatusCode();
        //    var responseString = await response.Content.ReadAsStringAsync();
        //    var activeCases = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseString).ToList();

        //    //Assert
        //    response.StatusCode.Should().Be(HttpStatusCode.OK);
        //    activeCases.Count().Should().BeGreaterOrEqualTo(0);
        //    activeCases.ForEach(c => c.CaseCode.Should().NotBe(0));
        //    activeCases.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
        //    activeCases.ForEach(c => c.ClientCode.Should().NotBe(0));
        //    activeCases.ForEach(c => c.ClientName.Should().NotBeNullOrEmpty());
        //    activeCases.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
        //    activeCases.ForEach(c => c.CaseManagerCode.Should().NotBeNullOrEmpty());
        //    activeCases.ForEach(c => c.CaseType.Should().NotBeNullOrEmpty());
        //    activeCases.ForEach(c => c.ManagingOfficeCode.Should().NotBe(0));
        //    activeCases.ForEach(c => c.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty());
        //    activeCases.ForEach(c => c.ManagingOfficeName.Should().NotBeNullOrEmpty());
        //    activeCases.ForEach(c => c.BillingOfficeCode.Should().NotBe(0));
        //    activeCases.ForEach(c => c.BillingOfficeAbbreviation.Should().NotBeNullOrEmpty());
        //    activeCases.ForEach(c => c.BillingOfficeName.Should().NotBeNullOrEmpty());
        //    activeCases.ForEach(c => c.EndDate.Should().NotBeBefore(c.StartDate));
        //    activeCases.FindAll(c => c.StartDate >= DateTime.Today).ForEach(c => c.Type.Should().Be("NewDemand"));
        //    activeCases.FindAll(c => c.StartDate < DateTime.Today).ForEach(c => c.Type.Should().Be("ActiveCase"));
        //}

        [Theory]
        [InlineData("god")]
        public async Task GetCasesForTypeahead_should_return_cases(string searchString)
        {
            //Act
            var response = await _testServer.Client.GetAsync(
                    $"/api/case/typeaheadCases?searchString={searchString}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var activeCases = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseString).ToList();
            var expectedOrder = activeCases.OrderByDescending(t => t.StartDate);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            activeCases.Count().Should().BeGreaterOrEqualTo(0);
            activeCases.Count().Should().BeLessOrEqualTo(100);
            activeCases.ForEach(c => c.CaseCode.Should().NotBe(0));
            activeCases.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.ClientCode.Should().NotBe(0));
            activeCases.ForEach(c => c.ClientName.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.CaseType.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.ManagingOfficeName.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.EndDate.Should().NotBeBefore(c.StartDate));
            activeCases.FindAll(c => c.StartDate >= DateTime.Today).ForEach(c => c.Type.Should().Be("NewDemand"));
            activeCases.FindAll(c => c.StartDate < DateTime.Today).ForEach(c => c.Type.Should().Be("ActiveCase"));
            expectedOrder.SequenceEqual(activeCases).Should().BeTrue();
        }

        [Theory]
        [InlineData(14)]
        public async Task GetCasesEndingBySpecificDate_should_return_cases(int caseEndsBeforeNumberOfDays)
        {
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/case/casesEndingSoon?caseEndsBeforeNumberOfDays={caseEndsBeforeNumberOfDays}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var activeCases = JsonConvert.DeserializeObject<IEnumerable<CaseViewModel>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            activeCases.Count().Should().BeGreaterOrEqualTo(0);
            activeCases.ForEach(c => c.CaseCode.Should().NotBe(0));
            activeCases.ForEach(c => c.CaseName.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.ManagingOfficeCode.Should().NotBe(0));
            activeCases.ForEach(c => c.ManagingOfficeAbbreviation.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.ManagingOfficeName.Should().NotBeNullOrEmpty());
            activeCases.ForEach(c => c.EndDate.Should().NotBeBefore(c.StartDate));
            activeCases.FindAll(c => c.StartDate >= DateTime.Today).ForEach(c => c.Type.Should().Be("NewDemand"));
            activeCases.FindAll(c => c.StartDate < DateTime.Today).ForEach(c => c.Type.Should().Be("ActiveCase"));
        }
    }
}
