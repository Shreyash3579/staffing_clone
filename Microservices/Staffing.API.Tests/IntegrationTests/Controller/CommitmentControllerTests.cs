using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "Staffing.API.CommitmentController")]
    public class CommitmentControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        private readonly IList<Guid?> resourceAllocationIds = new List<Guid?>();

        public CommitmentControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData("ST,NA", "43248", "2019-06-15", null)]
        [InlineData("ST,NA", "39400,43248", null, null)]
        [InlineData(null, "39400,43248", null, null)]
        public async Task GetCommitmentBySelectedValues(string commitmentTypeCodes, string employeeCodes, string startDate, string endDate)
        {


            // Act
            var response = await _testServer.Client.PostAsJsonAsync($"/api/Commitment/commitmentBySelectedValues",
              (new
              {
                  commitmentTypeCodes = commitmentTypeCodes,
                  employeeCodes = employeeCodes,
                  startDate = startDate,
                  endDate = endDate
              }));

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<CommitmentViewModel>>(responseString);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var commitmentsData = actualData.ToList();

            commitmentsData.ForEach(commitment =>
            {
                commitment.EmployeeCode.Should().NotBeNullOrEmpty();
                if (employeeCodes != null)
                {
                    Assert.Contains(commitment.EmployeeCode, employeeCodes);
                }
                commitment.EmployeeCode.Should().NotBeNullOrEmpty();
                if (commitmentTypeCodes != null)
                {
                    Assert.Contains(commitment.CommitmentTypeCode, commitmentTypeCodes);
                }

                commitment.StartDate.Should().NotBe(DateTime.MinValue);
                commitment.EndDate.Should().NotBe(DateTime.MinValue);

                if (startDate != null && endDate == null)
                {
                    commitment.EndDate.Should().BeOnOrAfter(Convert.ToDateTime(startDate).Date);
                }
                if (startDate != null && endDate != null)
                {
                    commitment.StartDate.Should().BeOnOrBefore(Convert.ToDateTime(endDate).Date);
                    commitment.EndDate.Should().BeOnOrAfter(Convert.ToDateTime(startDate).Date);
                }
            });

        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", "51030", "T", "2020-8-31", "2020-8-15", "test training", "51030", false)]
        public async Task InsertCommitmentsTest_To_Create_New_Commitment(
            Guid id,
            string employeeCodes,
            string commitmentTypeCodes,
            string startDate,
            string endDate,
            string notes,
            string lastUpdatedBy,
            bool isSourceStaffing)
        {
            var commitment = GetCommitment(
                id,
                employeeCodes,
                commitmentTypeCodes,
                startDate,
                endDate,
                notes,
                lastUpdatedBy,
                isSourceStaffing);

            var response = await _testServer.Client.PostAsJsonAsync($"/api/Commitment/resourcesCommitments", commitment);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<Commitment>>(responseString);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var commitmentsData = actualData.ToList();

            commitmentsData.ForEach(addedCommitment =>
            {
                if (id != Guid.Empty)
                {
                    addedCommitment.Id.Should().NotBe(id);
                }
                addedCommitment.EmployeeCode.Should().NotBeNullOrEmpty();
                if (employeeCodes != null)
                {
                    Assert.Contains(addedCommitment.EmployeeCode, employeeCodes);
                }
                addedCommitment.EmployeeCode.Should().NotBeNullOrEmpty();
                addedCommitment.StartDate.Should().NotBe(DateTime.MinValue);
                addedCommitment.EndDate.Should().NotBe(DateTime.MinValue);

                if (startDate != null && endDate == null)
                {
                    addedCommitment.EndDate.Should().Be(Convert.ToDateTime(startDate).Date);
                    addedCommitment.EndDate.Should().Be(Convert.ToDateTime(startDate).Date);
                }
            });
        }

        [Theory]
        [InlineData("07db8f06-63dd-ea11-a9a2-f016bbc25c2a", "51030", "T", "2020-8-31", "2020-8-15", "test training", "51030", false)]
        public async Task InsertCommitmentsTest_To_Update_Existing_Commitment(
            Guid id,
            string employeeCodes,
            string commitmentTypeCodes,
            string startDate,
            string endDate,
            string notes,
            string lastUpdatedBy,
            bool isSourceStaffing)
        {
            var commitment = GetCommitment(
                id,
                employeeCodes,
                commitmentTypeCodes,
                startDate,
                endDate,
                notes,
                lastUpdatedBy,
                isSourceStaffing);

            var response = await _testServer.Client.PostAsJsonAsync($"/api/Commitment/resourcesCommitments", commitment);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualData = JsonConvert.DeserializeObject<IEnumerable<Commitment>>(responseString);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var commitmentsData = actualData.ToList();

            commitmentsData.ForEach(addedCommitment =>
            {
                //if (id != Guid.Empty)
                //{
                //    addedCommitment.Id.Should().Be(id);
                //}
                addedCommitment.EmployeeCode.Should().NotBeNullOrEmpty();
                if (employeeCodes != null)
                {
                    Assert.Contains(addedCommitment.EmployeeCode, employeeCodes);
                }
                addedCommitment.EmployeeCode.Should().NotBeNullOrEmpty();
                addedCommitment.StartDate.Should().NotBe(DateTime.MinValue);
                addedCommitment.EndDate.Should().NotBe(DateTime.MinValue);

                if (startDate != null && endDate == null)
                {
                    addedCommitment.EndDate.Should().Be(Convert.ToDateTime(startDate).Date);
                    addedCommitment.EndDate.Should().Be(Convert.ToDateTime(startDate).Date);
                }
            });
        }

        private IList<Commitment> GetCommitment(Guid id,
            string employeeCodes,
            string commitmentTypeCodes,
            string startDate,
            string endDate,
            string notes,
            string lastUpdatedBy,
            bool isSourceStaffing)
        {
            return new List<Commitment>
            {
                new Commitment
                {
                    Id = id,
                    CommitmentType = new CommitmentType
                    {
                        CommitmentTypeCode = commitmentTypeCodes,
                        CommitmentTypeName = "Training",
                        IsStaffingTag = false,
                        Precedence = 7
                    },
                    EmployeeCode = employeeCodes,
                    EndDate = Convert.ToDateTime(endDate),
                    IsSourceStaffing = isSourceStaffing,
                    LastUpdatedBy = lastUpdatedBy,
                    Notes = notes,
                    StartDate = Convert.ToDateTime(startDate)
                }
            };
        }
    }

}
