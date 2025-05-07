using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;

namespace Staffing.Analytics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Updates Analytics Data with cost and other point in time data for employee codes
        /// Requires comma separated list of employee Codes
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// { "listEmployeeCodes" : "x,y,z"}
        /// </remarks>
        [HttpPost]
        [Route("updateCostForResourcesAvailableInFullCapacity")]
        public async Task<IActionResult> UpdateCostForResourcesAvailableInFullCapacity(dynamic payload)
        {
            var listEmployeeCodes = $"{payload["listEmployeeCodes"]}";
            var result = await _analyticsService.UpdateCostForResourcesAvailableInFullCapacity(listEmployeeCodes);
            return Ok(result);
        }

        /// <summary>
        /// Creates Analytics Data for upserted allocations based on allocation Ids
        /// Requires comma separated list of allocation Ids
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// "789eecad-145c-4506-881b-be28eb77c05a"
        /// </remarks>
        [HttpPost]
        [Route("analyticsReport")]
        public async Task<IActionResult> CreateAnalyticsReport([FromBody] string scheduleIds)
        {
            var result = await _analyticsService.CreateAnalyticsReport(scheduleIds);
            return Ok(result);
        }

        [HttpPost]
        [Route("correctanAlyticsData")]
        public async Task<IActionResult> CorrectAnalyticsData()
        {
            var result = await _analyticsService.CorrectAnalyticsData();
            return Ok(result);
        }


        /// <summary>
        /// Upsert Capacity Analysis daily table for tableau reporting
        /// </summary>
        /// <param name="fullLoad">Truncate and insert all records</param>
        /// <param name="loadAfterLastUpdated"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("upsertCapacityAnalysisDaily")]
        public async Task<IActionResult> UpsertCapacityAnalysisDaily(bool? fullLoad, DateTime? loadAfterLastUpdated)
        {
            await _analyticsService.UpsertCapacityAnalysisDaily(fullLoad, loadAfterLastUpdated);

            return Ok();
        }

        /// <summary>
        /// Upsert Capacity Analysis daily table for tableau reporting
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("updateCapacityAnalysisDailyForChangeInCaseAttribute")]
        public async Task<IActionResult> UpdateCapacityAnalysisDailyForChangeInCaseAttribute(DateTime? updateAfterTimeStamp)
        {
            await _analyticsService.UpdateCapacityAnalysisDailyForChangeInCaseAttribute(updateAfterTimeStamp);

            return Ok();
        }

        /// <summary>
        /// Upsert Capacity Analysis Monthly table for tableau reporting
        /// </summary>
        /// <param name="fullLoad">Reload Full dataset</param>
        /// <returns></returns>
        [HttpPut]
        [Route("upsertCapacityAnalysisMonthly")]
        public async Task<IActionResult> UpsertCapacityAnalysisMonthly(bool? fullLoad)
        {
            await _analyticsService.UpsertCapacityAnalysisMonthly(fullLoad);

            return Ok();
        }

        /// <summary>
        /// Update Analytics data for commitments created in BOSS
        /// </summary>
        /// <param name="commitmentIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateAnlayticsDataForUpsertedCommitment")]
        public async Task<IActionResult> UpdateAnlayticsDataForUpsertedCommitment([FromBody] string commitmentIds)
        {
            await _analyticsService.UpdateAnlayticsDataForUpsertedCommitment(commitmentIds);
            return Ok();
        }

        /// <summary>
        /// Update Analytics Data for commitments updated in external system
        /// </summary>
        /// <param name="updatedAfter"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateAnlayticsDataForUpsertedExternalCommitment")]
        public async Task<IActionResult> UpdateAnlayticsDataForUpsertedExternalCommitment([FromBody] DateTime? updatedAfter)
        {
            await _analyticsService.UpdateAnlayticsDataForUpsertedExternalCommitment(updatedAfter);
            return Ok();
        }

        /// <summary>
        ///  Update analytics data for commitments deleted from BOSS
        /// </summary>
        /// <param name="commitmentIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateAnlayticsDataForDeletedCommitment")]
        public async Task<IActionResult> UpdateAnlayticsDataForDeletedCommitment([FromBody] string commitmentIds)
        {
            await _analyticsService.UpdateAnlayticsDataForDeletedCommitment(commitmentIds);
            return Ok();
        }

        //TODO: remove this in favour of upsertAvailabilityDatabetweenDaterange
        ///// <summary>
        ///// Upsert availability data
        ///// </summary>
        ///// <param name="payload"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("upsertAvailabilityData")]
        //public async Task<IActionResult> UpsertAvailabilityData(dynamic payload)
        //{
        //    var listEmployeeCodes = $"{payload["listEmployeeCodes"]}";
        //    await _analyticsService.UpsertAvailabilityData(listEmployeeCodes);
        //    return Ok();
        //}

        /// <summary>
        /// Upsert availability data
        /// </summary>
        /// <remarks>
        ///  Sample Request:
        /// [
        ///     {
        ///         "employeeCode" : "39209",
        ///         "startDate" : "2023-01-01",
        ///         "endDate" : "2023-07-01",
        ///     }
        /// ]
        /// </remarks>
        /// <param name="availabilityData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertAvailabilityDataBetweenDateRange")]
        public async Task<IActionResult> UpsertAvailabilityDataBetweenDateRange(IEnumerable<AvailabilityDateRange> availabilityData)
        {
            await _analyticsService.UpsertAvailabilityDataBetweenDateRange(availabilityData);
            return Ok();
        }

        /// <summary>
        /// used to :-
        /// 1) create availability data for all active employees that have no data 
        /// 2) Insert daily avalability data for all active employees
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("insertDailyAvailabilityTillNextYearForAll")]
        public async Task<IActionResult> InsertDailyAvailabilityTillNextYearForAll(string employeeCodes)
        {
            var employeesWithNoAvailabilityRecords = await _analyticsService.InsertDailyAvailabilityTillNextYearForAll(employeeCodes);
            return Ok(employeesWithNoAvailabilityRecords);
        }

        /// <summary>
        /// Trigger this for data correction in schedulemasterdetail and Resource availability table by scheduleId
        /// USE ONLY WHEN we want to correect production data
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("updateCostAndAvailabilityDataByScheduleId")]
        public async Task<IActionResult> UpdateCostAndAvailabilityDataByScheduleId(string scheduleIds)
        {
            var updatedIds = await _analyticsService.UpdateCostAndAvailabilityDataByScheduleId(scheduleIds);
            return Ok(updatedIds);
        }

        //TO be checked for deletion
        [HttpDelete]
        [Route("DeleteAnalyticsDataByScheduleId")]
        public async Task<IActionResult> DeleteAnalyticsDataForDeletedAllocationByScheduleId(string deletedAllocationId)
        {
            var updatedIds = await _analyticsService.DeleteAnalyticsDataForDeletedAllocationByScheduleId(new Guid(deletedAllocationId));
            return Ok(updatedIds);
        }

        [HttpDelete]
        [Route("DeleteAnalyticsDataByScheduleIds")]
        public async Task<IActionResult> DeleteAnalyticsDataForDeletedAllocationByScheduleIds(string deletedAllocationIds)
        {
            await _analyticsService.DeleteAnalyticsDataForDeletedAllocationByScheduleIds(deletedAllocationIds);
            return Ok();
        }

        //TODO: Delete after 2019 data population or keep for future data population between date ranges
        /// <summary>
        /// used to :-
        /// 1) create availability data for all employees that don't have data between date ranges (right now does for year 2019 )
        /// </summary>
        /// <returns></returns>
        //[HttpPut]
        //[Route("insertAvailabilityDataForForResourcesWithNoDataIn2019")]
        //public async Task<IActionResult> InsertAvailabilityDataForForResourcesWithNoDataBetweenDateRange(string employeeCodes)
        //{
        //    var employeesWithNoAvailabilityRecords = await _analyticsService.InsertAvailabilityDataForForResourcesWithNoDataBetweenDateRange(employeeCodes);
        //    return Ok(employeesWithNoAvailabilityRecords);
        //}

        /// <summary>
        /// Get all allocation and availability within the selected start date and end date or after last updated date.
        /// </summary>
        /// <param name="startDate">End Date is mandatory with Start Date</param>
        /// <param name="endDate">Start Date is mandatory with End Date</param>
        /// <param name="lastUpdatedFrom">last Updated From is a manadatory field</param>
        /// <param name="lastUpdatedTo">last Updated To is a manadatory field</param>
        /// <param name="action">
        /// Send 'upserted' to get inserted and updated records
        /// Send 'deleted' to get deleted records
        /// </param> 
        /// <param name="sourceTable">used to fetch data of particular source table i.e SMD or RA</param>
        /// <param name="pageNumber">tells the system to skip x number of values</param>
        /// <param name="pageSize">tells the system about the page number whose data needs to be fetched</param>
        /// <returns></returns>
        [HttpGet]
        [Route("resourcesAllocationAndAvailability")]
        [Authorize(Policy = Constants.Policy.StaffingAnalyticsRead)]
        public async Task<IActionResult> GetResourcesAllocationAndAvailability(DateTime? startDate, DateTime? endDate, DateTime? lastUpdatedFrom, 
            DateTime? lastUpdatedTo, string action, string sourceTable, short pageNumber = 1, int pageSize = 100000)
        {
            var maxPageSize = int.Parse(ConfigurationUtility.GetValue("MaxPageThresholdForAnaplanAPI"));
            if (pageSize > maxPageSize)
            {
                throw new ArgumentException($"Page Size should not be greater than {maxPageSize}");
            }
            var employeesWithNoAvailabilityRecords = await _analyticsService.GetResourcesAllocationAndAvailabilityByDateRange(startDate, endDate, lastUpdatedFrom, lastUpdatedTo, action, sourceTable, pageNumber, pageSize);
            return Ok(employeesWithNoAvailabilityRecords);
        }


    }
}
