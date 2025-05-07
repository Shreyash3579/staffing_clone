using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Collection("Staffing.API.Integration")]
    [Trait("IntegrationTest", "Staffing.API.UserPreferencesController")]
    public class UserPreferencesControllerTests : IClassFixture<TestServerHost>, IDisposable
    {
        private readonly TestServerHost _testServer;
        private string insertedEmployeeCode = "";

        public UserPreferencesControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(insertedEmployeeCode))
            {
                Task.Run(async () => await
                    _testServer.Client.DeleteAsync("/api/userPreferences?employeeCode=" + insertedEmployeeCode)).Wait();
            }
        }

        [Theory]
        [ClassData(typeof(UserPreferencesTestDataGenerator))]
        public async Task GetUserPreferences_should_get_userPreferences_for_employee(
            UserPreferences testUserPreferences)
        {
            //Arrange (insert test data in db)
            var response = await _testServer.Client.PostAsync("/api/userPreferences",
                new StringContent(JsonConvert.SerializeObject(testUserPreferences), Encoding.Default,
                    "application/json"));
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var insertedData = JsonConvert.DeserializeObject<UserPreferences>(responseString);

            insertedEmployeeCode = insertedData.EmployeeCode;

            //ACT
            response = await _testServer.Client.GetAsync(
                $"/api/userPreferences?employeeCode={insertedEmployeeCode}");
            response.EnsureSuccessStatusCode();

            responseString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<UserPreferences>(responseString);

            //ASSERT
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseData.EmployeeCode.Should().NotBeNullOrEmpty();
            responseData.EmployeeCode.Should().Be(testUserPreferences.EmployeeCode);
            responseData.SupplyViewOfficeCodes.Should().Be(testUserPreferences.SupplyViewOfficeCodes);
            responseData.SupplyViewStaffingTags.Should().Be(testUserPreferences.SupplyViewStaffingTags);
            responseData.LevelGrades.Should().Be(testUserPreferences.LevelGrades);
            responseData.GroupBy.Should().Be(testUserPreferences.GroupBy);
            responseData.SortBy.Should().Be(testUserPreferences.SortBy);
            responseData.SupplyWeeksThreshold.Should().Be(testUserPreferences.SupplyWeeksThreshold);
            responseData.VacationThreshold.Should().Be(testUserPreferences.VacationThreshold);
            responseData.TrainingThreshold.Should().Be(testUserPreferences.TrainingThreshold);
            responseData.DemandTypes.Should().Be(testUserPreferences.DemandTypes);
            responseData.DemandViewOfficeCodes.Should().Be(testUserPreferences.DemandViewOfficeCodes);
            responseData.CaseTypeCodes.Should().Be(testUserPreferences.CaseTypeCodes);
            responseData.CaseAttributeNames.Should().Be(testUserPreferences.CaseAttributeNames);
            responseData.OpportunityStatusTypeCodes.Should().Be(testUserPreferences.OpportunityStatusTypeCodes);
            responseData.MinOpportunityProbability.Should().Be(testUserPreferences.MinOpportunityProbability);
            responseData.DemandWeeksThreshold.Should().Be(testUserPreferences.DemandWeeksThreshold);
            responseData.CaseExceptionHideList.Should().Be(testUserPreferences.CaseExceptionHideList);
            responseData.CaseExceptionShowList.Should().Be(testUserPreferences.CaseExceptionShowList);
            responseData.OpportunityExceptionHideList.Should().Be(testUserPreferences.OpportunityExceptionHideList);
            responseData.OpportunityExceptionShowList.Should().Be(testUserPreferences.OpportunityExceptionShowList);
            responseData.LastUpdatedBy.Should().Be(testUserPreferences.LastUpdatedBy);
            responseData.CaseAllocationsSortBy.Should().Be(testUserPreferences.CaseAllocationsSortBy);
            responseData.CaseOppSortOrder.Should().Be(testUserPreferences.CaseOppSortOrder);
            responseData.PlanningCardsSortOrder.Should().Be(testUserPreferences.PlanningCardsSortOrder);
        }

        [Theory]
        [ClassData(typeof(UserPreferencesTestDataGenerator))]
        public async Task InsertUserPreferences_should_insert_row_in_userPreferences(
            UserPreferences testUserPreferences)
        {
            //Act
            var response = await _testServer.Client.PostAsync("/api/userPreferences",
                new StringContent(JsonConvert.SerializeObject(testUserPreferences), Encoding.Default,
                    "application/json"));
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var insertedData = JsonConvert.DeserializeObject<UserPreferences>(responseString);

            insertedEmployeeCode = insertedData.EmployeeCode;

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            insertedData.EmployeeCode.Should().NotBeNullOrEmpty();
            insertedData.EmployeeCode.Should().Be(testUserPreferences.EmployeeCode);
            insertedData.SupplyViewOfficeCodes.Should().Be(testUserPreferences.SupplyViewOfficeCodes);
            insertedData.SupplyViewStaffingTags.Should().Be(testUserPreferences.SupplyViewStaffingTags);
            insertedData.LevelGrades.Should().Be(testUserPreferences.LevelGrades);
            insertedData.GroupBy.Should().Be(testUserPreferences.GroupBy);
            insertedData.SortBy.Should().Be(testUserPreferences.SortBy);
            insertedData.SupplyWeeksThreshold.Should().Be(testUserPreferences.SupplyWeeksThreshold);
            insertedData.VacationThreshold.Should().Be(testUserPreferences.VacationThreshold);
            insertedData.TrainingThreshold.Should().Be(testUserPreferences.TrainingThreshold);
            insertedData.DemandViewOfficeCodes.Should().Be(testUserPreferences.DemandViewOfficeCodes);
            insertedData.CaseTypeCodes.Should().Be(testUserPreferences.CaseTypeCodes);
            insertedData.CaseAttributeNames.Should().Be(testUserPreferences.CaseAttributeNames);
            insertedData.OpportunityStatusTypeCodes.Should().Be(testUserPreferences.OpportunityStatusTypeCodes);
            insertedData.MinOpportunityProbability.Should().Be(testUserPreferences.MinOpportunityProbability);
            insertedData.DemandTypes.Should().Be(testUserPreferences.DemandTypes);
            insertedData.DemandWeeksThreshold.Should().Be(testUserPreferences.DemandWeeksThreshold);
            insertedData.CaseExceptionHideList.Should().Be(testUserPreferences.CaseExceptionHideList);
            insertedData.CaseExceptionShowList.Should().Be(testUserPreferences.CaseExceptionShowList);
            insertedData.OpportunityExceptionHideList.Should().Be(testUserPreferences.OpportunityExceptionHideList);
            insertedData.OpportunityExceptionShowList.Should().Be(testUserPreferences.OpportunityExceptionShowList);
            insertedData.LastUpdatedBy.Should().Be(testUserPreferences.LastUpdatedBy);
            insertedData.CaseAllocationsSortBy.Should().Be(testUserPreferences.CaseAllocationsSortBy);
            insertedData.CaseOppSortOrder.Should().Be(testUserPreferences.CaseOppSortOrder);
            insertedData.PlanningCardsSortOrder.Should().Be(testUserPreferences.PlanningCardsSortOrder);
        }

        [Theory]
        [ClassData(typeof(UserPreferencesTestDataGenerator))]
        public async Task UpdateUserPreferences_should_update_in_userPreferences(UserPreferences testUserPreferences)
        {
            //Arrange (insert test data in db)
            var response = await _testServer.Client.PostAsync("/api/userPreferences",
                new StringContent(JsonConvert.SerializeObject(testUserPreferences), Encoding.Default,
                    "application/json"));
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var insertedData = JsonConvert.DeserializeObject<UserPreferences>(responseString);

            insertedEmployeeCode = insertedData.EmployeeCode;

            //ACT
            testUserPreferences.SupplyViewOfficeCodes = "404,334";
            testUserPreferences.DemandViewOfficeCodes = "404,120";
            testUserPreferences.SupplyWeeksThreshold = 2;
            testUserPreferences.DemandWeeksThreshold = 3;
            testUserPreferences.GroupBy = "Position";
            testUserPreferences.LastUpdatedBy = "37995";
            testUserPreferences.CaseAllocationsSortBy = "endDateAsc";
            testUserPreferences.CaseOppSortOrder = "";
            testUserPreferences.PlanningCardsSortOrder = "";

            response = await _testServer.Client.PutAsync("/api/userPreferences",
                new StringContent(JsonConvert.SerializeObject(testUserPreferences), Encoding.Default,
                    "application/json"));
            response.EnsureSuccessStatusCode();

            responseString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<UserPreferences>(responseString);

            //ASSERT
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseData.EmployeeCode.Should().NotBeNullOrEmpty();
            responseData.EmployeeCode.Should().Be(testUserPreferences.EmployeeCode);
            responseData.SupplyViewOfficeCodes.Should().Be(testUserPreferences.SupplyViewOfficeCodes);
            responseData.SupplyViewStaffingTags.Should().Be(testUserPreferences.SupplyViewStaffingTags);
            responseData.LevelGrades.Should().Be(testUserPreferences.LevelGrades);
            responseData.GroupBy.Should().Be(testUserPreferences.GroupBy);
            responseData.SortBy.Should().Be(testUserPreferences.SortBy);
            responseData.SupplyWeeksThreshold.Should().Be(testUserPreferences.SupplyWeeksThreshold);
            responseData.VacationThreshold.Should().Be(testUserPreferences.VacationThreshold);
            responseData.TrainingThreshold.Should().Be(testUserPreferences.TrainingThreshold);
            responseData.DemandViewOfficeCodes.Should().Be(testUserPreferences.DemandViewOfficeCodes);
            responseData.CaseTypeCodes.Should().Be(testUserPreferences.CaseTypeCodes);
            responseData.CaseAttributeNames.Should().Be(testUserPreferences.CaseAttributeNames);
            responseData.OpportunityStatusTypeCodes.Should().Be(testUserPreferences.OpportunityStatusTypeCodes);
            responseData.MinOpportunityProbability.Should().Be(testUserPreferences.MinOpportunityProbability);
            responseData.DemandTypes.Should().Be(testUserPreferences.DemandTypes);
            responseData.DemandWeeksThreshold.Should().Be(testUserPreferences.DemandWeeksThreshold);
            responseData.CaseExceptionHideList.Should().Be(testUserPreferences.CaseExceptionHideList);
            responseData.CaseExceptionShowList.Should().Be(testUserPreferences.CaseExceptionShowList);
            responseData.OpportunityExceptionHideList.Should().Be(testUserPreferences.OpportunityExceptionHideList);
            responseData.OpportunityExceptionShowList.Should().Be(testUserPreferences.OpportunityExceptionShowList);
            responseData.LastUpdatedBy.Should().Be(testUserPreferences.LastUpdatedBy);
            responseData.CaseAllocationsSortBy.Should().Be(testUserPreferences.CaseAllocationsSortBy);
            responseData.CaseOppSortOrder.Should().Be(testUserPreferences.CaseOppSortOrder);
            responseData.PlanningCardsSortOrder.Should().Be(testUserPreferences.PlanningCardsSortOrder);
        }

        [Theory]
        [ClassData(typeof(UserPreferencesTestDataGenerator))]
        public async Task DeleteUserPreferences_should_delete_in_userPreferences(UserPreferences testUserPreferences)
        {
            //Arrange (insert test data in db)
            //This is done to avoid duplicate entries for TEST1 when insert and update tests are run in parallel
            testUserPreferences.EmployeeCode = "TEST2";
            var response = await _testServer.Client.PostAsync("/api/userPreferences",
                new StringContent(JsonConvert.SerializeObject(testUserPreferences), Encoding.Default,
                    "application/json"));
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var insertedData = JsonConvert.DeserializeObject<UserPreferences>(responseString);
            insertedEmployeeCode = insertedData.EmployeeCode;

            //ACT
            response = await _testServer.Client.DeleteAsync(
                $"/api/userPreferences?employeeCode={insertedEmployeeCode}");
            response.EnsureSuccessStatusCode();
            var deletedResponseString = await response.Content.ReadAsStringAsync();
            var deletedData = JsonConvert.DeserializeObject<UserPreferences>(responseString);

            //ASSERT
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            deletedData.EmployeeCode.Should().NotBeNullOrEmpty();
            deletedData.EmployeeCode.Should().Be(testUserPreferences.EmployeeCode);
            deletedData.SupplyViewOfficeCodes.Should().Be(testUserPreferences.SupplyViewOfficeCodes);
            deletedData.SupplyViewStaffingTags.Should().Be(testUserPreferences.SupplyViewStaffingTags);
            deletedData.LevelGrades.Should().Be(testUserPreferences.LevelGrades);
            deletedData.GroupBy.Should().Be(testUserPreferences.GroupBy);
            deletedData.SortBy.Should().Be(testUserPreferences.SortBy);
            deletedData.SupplyWeeksThreshold.Should().Be(testUserPreferences.SupplyWeeksThreshold);
            deletedData.VacationThreshold.Should().Be(testUserPreferences.VacationThreshold);
            deletedData.TrainingThreshold.Should().Be(testUserPreferences.TrainingThreshold);
            deletedData.DemandViewOfficeCodes.Should().Be(testUserPreferences.DemandViewOfficeCodes);
            deletedData.CaseTypeCodes.Should().Be(testUserPreferences.CaseTypeCodes);
            deletedData.CaseAttributeNames.Should().Be(testUserPreferences.CaseAttributeNames);
            deletedData.OpportunityStatusTypeCodes.Should().Be(testUserPreferences.OpportunityStatusTypeCodes);
            deletedData.MinOpportunityProbability.Should().Be(testUserPreferences.MinOpportunityProbability);
            deletedData.DemandTypes.Should().Be(testUserPreferences.DemandTypes);
            deletedData.DemandWeeksThreshold.Should().Be(testUserPreferences.DemandWeeksThreshold);
            deletedData.CaseExceptionHideList.Should().Be(testUserPreferences.CaseExceptionHideList);
            deletedData.CaseExceptionShowList.Should().Be(testUserPreferences.CaseExceptionShowList);
            deletedData.OpportunityExceptionHideList.Should().Be(testUserPreferences.OpportunityExceptionHideList);
            deletedData.OpportunityExceptionShowList.Should().Be(testUserPreferences.OpportunityExceptionShowList);
            deletedData.LastUpdatedBy.Should().Be(testUserPreferences.LastUpdatedBy);
            deletedData.CaseAllocationsSortBy.Should().Be(testUserPreferences.CaseAllocationsSortBy);
            deletedData.CaseOppSortOrder.Should().Be(testUserPreferences.CaseOppSortOrder);
            deletedData.PlanningCardsSortOrder.Should().Be(testUserPreferences.PlanningCardsSortOrder);

            insertedEmployeeCode = ""; //to prevent delete from firing again in Dispose
        }
    }

    public class UserPreferencesTestDataGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new UserPreferences
                {
                    EmployeeCode = "TEST1",
                    SupplyViewOfficeCodes = "110,112",
                    SupplyViewStaffingTags = "SL0001,SL0006,SL0022,SL0004,P",
                    LevelGrades = "A1,M1,C1,V1,SC1,TG10,TT12,U3",
                    GroupBy = "ServiceLine",
                    SortBy = "levelGrade,fullName",
                    TrainingThreshold = 5,
                    VacationThreshold = 5,
                    SupplyWeeksThreshold = 4,
                    DemandTypes = "Opportunity,ActiveCase",
                    DemandViewOfficeCodes = "110,404,332",
                    CaseTypeCodes = "1,2",
                    DemandWeeksThreshold = 4,
                    CaseAttributeNames = "PEG,FRWD,ADAPT,AAG",
                    OpportunityStatusTypeCodes = "1,5",
                    MinOpportunityProbability = 30,
                    CaseExceptionHideList = "W3EW",
                    CaseExceptionShowList = null,
                    OpportunityExceptionHideList = "1f43a820-5dc2-4d09-8614-02c04ff54da5",
                    OpportunityExceptionShowList = null,
                    LastUpdatedBy = "39209",
                    CaseAllocationsSortBy = "nameZtoA",
                    CaseOppSortOrder = "C5RU,R5BZ,A6TJ,G3UE",
                    PlanningCardsSortOrder = "849d056a-2d98-eb11-a9a8-d7c2df109166,623282b4-3498-eb11-a9a8-d7c2df109166"
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}