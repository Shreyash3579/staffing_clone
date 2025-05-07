using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Collection("Staffing.API.Integration")]
    [Trait("IntegrationTest", "Staffing.API.SKUCaseTermsController")]
    public class SKUCaseTermsControllerTest : IClassFixture<TestServerHost>, IDisposable
    {
        private readonly TestServerHost _testServer;
        private Guid? Id = Guid.Empty;

        public SKUCaseTermsControllerTest(TestServerHost testServer)
        {
            _testServer = testServer;
        }
        [Fact]
        public async Task GetSKUTermList_should_return_SKUterms()
        {
            //Act
            var response = await _testServer.Client.GetAsync($"/api/skucaseterms/getskutermlist");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var skuTermsMasterData = JsonConvert.DeserializeObject<IEnumerable<SKUTerm>>(responseString);

            var skuTerms = skuTermsMasterData.ToList();
            skuTerms.ForEach(skuTerm =>
            {
                skuTerm.Code.Should().NotBe(0);
                skuTerm.Name.Should().NotBeNullOrEmpty();
                skuTerm.Size.Should().NotBe(0);
            });
        }

        [Theory]
        [ClassData(typeof(SKUOpportunityTermsTestInsertDataGenerator))]
        public async Task GetSKUTermsForOppotunity_should_return_SKUterms(object skuCaseTermsInsertObject)
        {
            var response = await _testServer.Client.PostAsJsonAsync($"/api/skucaseterms/insertskutermsforcase",
                skuCaseTermsInsertObject);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var skuCaseInsertedTerms = JsonConvert.DeserializeObject<SKUCaseTerms>(responseString);
            skuCaseInsertedTerms.Id.Should().NotBe(Guid.Empty);
            Id = skuCaseInsertedTerms.Id;

            var pipelineId = skuCaseInsertedTerms.PipelineId;

            var response1 = await _testServer.Client.GetAsync($"/api/skucaseterms/getskutermsforopportunity?pipelineId={pipelineId}");
            response1.EnsureSuccessStatusCode();
            var responseString1 = await response1.Content.ReadAsStringAsync();
            var skuTermsMasterData = JsonConvert.DeserializeObject<IEnumerable<SKUCaseTermsViewModel>>(responseString1);

            var skuTerms = skuTermsMasterData.ToList();
            skuTerms.ForEach(skuTerm =>
            {
                skuTerm.Id.Should().NotBeEmpty().And.NotBeNull();
                skuTerm.SKUTermsCodes.Should().NotBeNullOrEmpty();
                skuTerm.PipelineId.Should().NotBeEmpty().And.NotBeNull();
                skuTerm.EffectiveDate.Should().NotBeNullOrEmpty();
            });
        }

        [Theory]
        [ClassData(typeof(SKUCaseTermsTestInsertDataGenerator))]
        public async Task getskutermsforcase_should_return_SKUterms(object skuCaseTermsInsertObject)
        {
            var response = await _testServer.Client.PostAsJsonAsync($"/api/skucaseterms/insertskutermsforcase",
                skuCaseTermsInsertObject);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var skuCaseInsertedTerms = JsonConvert.DeserializeObject<SKUCaseTerms>(responseString);
            skuCaseInsertedTerms.Id.Should().NotBe(Guid.Empty);
            Id = skuCaseInsertedTerms.Id;

            var oldCaseCode = skuCaseInsertedTerms.OldCaseCode;

            var response1 = await _testServer.Client.GetAsync($"/api/skucaseterms/getskutermsforcase?oldCaseCode={oldCaseCode}");
            response1.EnsureSuccessStatusCode();
            var responseString1 = await response1.Content.ReadAsStringAsync();
            var skuTermsMasterData = JsonConvert.DeserializeObject<IEnumerable<SKUCaseTermsViewModel>>(responseString1);

            var skuTerms = skuTermsMasterData.ToList();
            skuTerms.ForEach(skuTerm =>
            {
                skuTerm.Id.Should().NotBeEmpty().And.NotBeNull();
                skuTerm.SKUTermsCodes.Should().NotBeNullOrEmpty();
                skuTerm.OldCaseCode.Should().NotBeNullOrEmpty();
                skuTerm.EffectiveDate.Should().NotBeNullOrEmpty();
            });
        }

        [Theory]
        [ClassData(typeof(SKUCaseTermsTestInsertDataGenerator))]
        [ClassData(typeof(SKUOpportunityTermsTestInsertDataGenerator))]
        public async Task InsertSKUCaseTerms_should_insert_row_in_SKUCaseTerms(object skuCaseTermsObject)
        {
            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/skucaseterms/insertskutermsforcase",
                skuCaseTermsObject);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var skuCaseTerms = JsonConvert.DeserializeObject<SKUCaseTerms>(responseString);
            skuCaseTerms.Id.Should().NotBe(Guid.Empty);
            Id = skuCaseTerms.Id;
            if (string.IsNullOrEmpty(skuCaseTerms.OldCaseCode))
            {
                skuCaseTerms.PipelineId.Should().NotBe(Guid.Empty);
            }
            else
            {
                skuCaseTerms.OldCaseCode.Should().NotBeNullOrEmpty();
            }
        }

        [Theory]
        [ClassData(typeof(SKUCaseTermsTestInsertDataGenerator))]
        public async Task UpdateSKUCaseTerms_should_update_row_in_SKUCaseTerms(object skuCaseTermsInsertObject)
        {
            //Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/skucaseterms/insertskutermsforcase",
                skuCaseTermsInsertObject);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var skuCaseInsertedTerms = JsonConvert.DeserializeObject<SKUCaseTerms>(responseString);
            skuCaseInsertedTerms.Id.Should().NotBe(Guid.Empty);
            Id = skuCaseInsertedTerms.Id;
            var skuEffectiveDate = DateTime.Now.AddDays(30);
            var skuCaseTermsUpdateObject = new
            {
                id = Id,
                oldCaseCode = "K7FC",
                skuTermsCodes = "11,12",
                effectiveDate = skuEffectiveDate,
                lastUpdatedBy = "37995"
            };
            var response1 = await _testServer.Client.PutAsJsonAsync($"/api/skucaseterms/updateskutermsforcase",
                skuCaseTermsUpdateObject);
            response1.EnsureSuccessStatusCode();
            var response1String = await response1.Content.ReadAsStringAsync();
            var skuCaseUpdatedTerms = JsonConvert.DeserializeObject<SKUCaseTerms>(response1String);
            skuCaseUpdatedTerms.Id.Should().Be(Id);
            skuCaseUpdatedTerms.OldCaseCode.Should().Be(skuCaseTermsUpdateObject.oldCaseCode);
            skuCaseUpdatedTerms.SKUTermsCodes.Should().Be(skuCaseTermsUpdateObject.skuTermsCodes);
            skuCaseUpdatedTerms.EffectiveDate.Should().BeSameDateAs(skuCaseTermsUpdateObject.effectiveDate);
            skuCaseUpdatedTerms.LastUpdatedBy.Should().Be(skuCaseTermsUpdateObject.lastUpdatedBy);
        }

        //[Theory]
        //[InlineData("K7FC,C8MR", "", "2019-07-16", "2019-09-05")]
        //[InlineData("", "1A1DEE86-5A3B-4EEE-92DA-5ED5D6D50954,901FB7BF-189D-47A4-9310-7293FE9F9FF1", "2019-07-16", "2019-09-05")]
        //public async Task GetSKUTermsForCasesOrOpportunitiesForDuration_should_return_SKUterms(string oldCaseCodes, string pipelineIds,
        //    DateTime startDate, DateTime endDate)
        //{
        //    var response = await _testServer.Client.GetAsync($"/api/skucaseterms/getskutermsforcasesoropportunitiesforduration?" +
        //        $"oldCaseCodes={oldCaseCodes}&pipelineIds={pipelineIds}&startDate={startDate}&endDate={endDate}");
        //    response.EnsureSuccessStatusCode();
        //    var responseString = await response.Content.ReadAsStringAsync();
        //    var skuTermsMasterData = JsonConvert.DeserializeObject<IEnumerable<SKUCaseTermsViewModel>>(responseString);

        //    var skuTerms = skuTermsMasterData.ToList();
        //    skuTerms.ForEach(skuTerm =>
        //    {
        //        skuTerm.Id.Should().NotBeEmpty();
        //        skuTerm.Id.Should().NotBeNull();
        //        skuTerm.SKUTermsCodes.Should().NotBeNullOrEmpty();
        //        if (oldCaseCodes == "")
        //        {
        //            skuTerm.PipelineId.Should().NotBeEmpty();
        //            skuTerm.PipelineId.Should().NotBeNull();
        //        }
        //        else
        //        {
        //            skuTerm.OldCaseCode.Should().NotBeNullOrEmpty();
        //        }
        //        skuTerm.EffectiveDate.Should().NotBeNullOrEmpty();
        //        skuTerm.LastUpdatedBy.Should().NotBeNullOrEmpty();
        //    });
        //}

        public void Dispose()
        {
            if (Id != Guid.Empty)
            {
                var response = _testServer.Client.DeleteAsync($"/api/skucaseterms/deleteskutermsforcase?Id=" + Id);
            }
        }
    }
}

public class SKUCaseTermsTestInsertDataGenerator : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new
            {
                oldCaseCode = "K7FC",
                skuTermsCodes = "1,2",
                effectiveDate = DateTime.Now.AddDays(30),
                lastUpdatedBy = "45088"
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class SKUOpportunityTermsTestInsertDataGenerator : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new
            {
                pipelineId = Guid.NewGuid(),
                skuTermsCodes = "3,4",
                effectiveDate = DateTime.Now.AddDays(30),
                lastUpdatedBy = "45088"
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
