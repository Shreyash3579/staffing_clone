using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkdayPollingController : ControllerBase
    {
        private readonly IWorkdayPollingService _workdayPollingService;

        public WorkdayPollingController(IWorkdayPollingService workdayPollingService)
        {
            _workdayPollingService = workdayPollingService;
        }

        /// <summary>
        ///     Update Cost for pending Promotions, LoAs, Transfers, Transitions and Terminations
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("pendingTransactions")]
        public async Task<IActionResult> UpdateAnalyitcsDataForPendingTransactions(DateTime? lastModifiedDateOnOrAfter)
        {
            await _workdayPollingService.UpdateAnalyitcsDataForPendingTransactions(lastModifiedDateOnOrAfter);
            return Ok();
        }

        [HttpPut]
        [Route("costForAllocationsAnalytics")]
        public async Task<IActionResult> UpdateCostForAllocationsAnalyticsData()
        {
            await _workdayPollingService.UpdateCostForAllocationsAnalyticsData();
            return Ok("Bill Rates Updated");
        }

        [HttpPut]
        [Route("costForPlaceholdersAnalytics")]
        public async Task<IActionResult> UpdateCostForPlaceholdersAnalyticsData()
        {
            await _workdayPollingService.UpdateCostForPlaceholdersAnalyticsData();
            return Ok("Bill Rates Updated");
        }


        [HttpPut]
        [Route("analyticsDataUpdateForLoAUpdatedRecently")]
        public async Task<IActionResult> UpdateAnalyticsDataForLoAUpdatedRecently(DateTime? lastModifiedDateOnOrAfter, DateTime? fromEffectiveDate)
        {
            await _workdayPollingService.UpdateAnalyticsDataForLoAUpdatedRecently(lastModifiedDateOnOrAfter, fromEffectiveDate);
            return Ok("Data updated for LOA updated recently");
        }

        [HttpPut]
        [Route("insertDailyAvailabilityTillNextYearForAll")]
        public async Task<IActionResult> InsertDailyAvailabilityTillNextYearForAll()
        {
            await _workdayPollingService.InsertDailyAvailabilityTillNextYearForAll();
            return Ok();
        }

        ////Is this needed any more?
        //[HttpPut]
        //[Route("updateServiceLineInScheduleMaster")]
        //public async Task<IActionResult> UpdateServiceLineInScheduleMaster()
        //{
        //    await _workdayPollingService.UpdateServiceLineInScheduleMaster();
        //    return Ok("Service line name updated for records with no service line");
        //}


        //Is this needed any more? Didn't find the SP in database
        /// <summary>
        ///     Update the commitments created as placeholder in staffing system once they are added/updated in the source system 
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("updateOverrideFlagForStaffingCommtimentsFromSource")]
        public async Task<IActionResult> UpdateOverrideFlagForStaffingCommtimentsFromSource()
        {
            await _workdayPollingService.UpdateOverrideFlagForStaffingCommtimentsFromSource();
            return Ok("Override flag updated for staffing Commitments on the basis of the entry in source system");
        }

        /// <summary>
        ///     deletes the additional/extra staffing data for employees who have been terminated after their termination date
        ///     Updates Schedulemaster allocations to the last day of termination
        ///     Deletes rows from SchedulemasterDetail and resource Availability tables after their termination date
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("deleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate")]
        public async Task<IActionResult> DeleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate()
        {
            var responseString = await _workdayPollingService.DeleteStaffingAndCommitmentDataForTermedEmployeesAfterTerminationDate();
            return Ok(responseString);
        }

        [HttpPost]
        [Route("saveWorkdayLoaAndTranstionAsShortTermCommitment")]
        public async Task<IActionResult> SaveWorkdayLoaAndTransitionsAsShortTermCommitments()
        {
            var shortTermCommitments = await _workdayPollingService.SaveWorkdayLoaAndTransitionAsShortTermCommitment();
            return Ok(shortTermCommitments);
        }

        [HttpPost]
        [Route("saveWorkdayServiceLineListForTableau")]
        public async Task<IActionResult> SaveWorkdayServiceLineListForTableau()
        {
            var serviceLines = await _workdayPollingService.SaveWorkdayServiceLineListForTableau();
            return Ok(serviceLines);
        }

        [HttpPost]
        [Route("saveWorkdayPDGradeListForTableau")]
        public async Task<IActionResult> SaveWorkdayPDGradeListForTableau()
        {
            var pdGrades = await _workdayPollingService.SaveWorkdayPDGradeListForTableau();
            return Ok(pdGrades);
        }

        [HttpPost]
        [Route("saveWorkdayTimeOffsForTableau")]
        public async Task<IActionResult> SaveWorkdayTimeOffsForTableau()
        {
            var message = await _workdayPollingService.SaveWorkdayTimeOffsForTableau();
            return Ok(message);
        }


        /// <summary>
        ///    FAllback mechanism to find and update the analytics records having incorrect workday info.
        /// </summary>
        [HttpPost]
        [Route("updateAnalyticsDataHavingIncorrectWorkdayInfo")]
        public async Task<IActionResult> UpdateAnalyticsDataHavingIncorrectWorkdayInfo()
        {
            await _workdayPollingService.UpdateAnalyticsDataHavingIncorrectWorkdayInfo();
            return Ok();
        }

        /// <summary>
        ///    FAllback mechanism to find and update the analytics records having incorrect case info.
        /// </summary>
        [HttpPost]
        [Route("updateAnalyticsDataHavingIncorrectCaseInfo")]
        public async Task<IActionResult> UpdateAnalyticsDataHavingIncorrectCaseInfo()
        {
            await _workdayPollingService.UpdateAnalyticsDataHavingIncorrectCaseInfo();
            return Ok();
        }

        [HttpPost]
        [Route("updateAnalyticsPlaceholderDataHavingIncorrectWorkdayInfo")]
        public async Task<IActionResult> UpdateAnalyticsPlaceholderDataHavingIncorrectWorkdayInfo()
        {
            await _workdayPollingService.UpdateAnalyticsPlaceholdersDataHavingIncorrectWorkdayInfo();
            return Ok();
        }

        /// <summary>
        ///    Fallback mechanism to find and update the analytics records having incorrect workday info in RA table.
        /// </summary>
        [HttpPost]
        [Route("updateAvailabilityDataHavingIncorrectWorkdayInfo")]
        public async Task<IActionResult> UpdateAvailabilityDataHavingIncorrectWorkdayInfo()
        {
            await _workdayPollingService.UpdateAvailabilityDataHavingIncorrectWorkdayInfo();
            return Ok();
        }

        /// <summary>
        ///    Fallback mechanism to find and update the analytics records having incorrect commitments and Ringfence in RA table.
        /// </summary>
        [HttpPost]
        [Route("updateAvailabilityDataForExternalCommitmentsAndRingfence")]
        public async Task<IActionResult> UpdateAvailabilityDataForExternalCommitmentsAndRingfence()
        {
            await _workdayPollingService.UpdateAvailabilityDataForExternalCommitmentsAndRingfence();
            return Ok();
        }

        /// <summary>
        ///    Fallback mechanism to find and update the analytics records having incorrect availability in RA table.
        /// </summary>
        [HttpPost]
        [Route("updateAvailabilityDataForMissingOrIrrelevantEntries")]
        public async Task<IActionResult> UpdateAvailabilityDataForMissingOrIrrelevantEntries()
        {
            await _workdayPollingService.UpdateAvailabilityDataForMissingOrIrrelevantEntries();
            return Ok();
        }

        /// <summary>
        ///    Save workday employee staffing transaction in Databse for anlaytics report verification
        /// </summary>
        [HttpPost]
        [Route("upsertWorkdayEmployeeStaffingTransactionToDB")]
        public async Task<IActionResult> UpsertWorkdayEmployeeStaffingTransactionToDB()
        {
            var result = await _workdayPollingService.UpsertWorkdayEmployeeStaffingTransactionToDB();
            return Ok(result);
        }

        /// <summary>
        ///    Save workday employee LOA transaction in Databse for anlaytics report verification
        /// </summary>
        [HttpPost]
        [Route("upsertWorkdayEmployeeLoATransactionToDB")]
        public async Task<IActionResult> UpsertWorkdayEmployeeLoATransactionToDB()
        {
            var result = await _workdayPollingService.UpsertWorkdayEmployeeLoATransactionToDB();
            return Ok(result);
        }

        /// <summary>
        ///    Save workday employee data to be used in tableau
        /// </summary>
        [HttpPost]
        [Route("saveWorkdayEmployeeDataForTableau")]
        public async Task<IActionResult> SaveWorkdayEmployeeDataForTableau()
        {
            var result = await _workdayPollingService.SaveWorkdayEmployeeDataForTableau();
            return Ok(result);
        }

        /// <summary>
        ///    Delete Availability Data for Rescinded employees
        ///    employeeCodes (optional) : If passed, will delete data for only these rescinded employees
        ///                    Else, delete for all rescinded employees
        /// </summary>
        [HttpDelete]
        [Route("deleteAvailabilityDataForRescindedEmployees")]
        public async Task<IActionResult> DeleteAvailabilityDataForRescindedEmployees(string employeeCodes)
        {
            var result = await _workdayPollingService.DeleteAvailabilityDataForRescindedEmployees(employeeCodes);
            return Ok(result);
        }

        /// <summary>
        ///     Update the staffabl-as flag for promoted employees
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("archiveStaffableAsRoleForPromotedEmployees")]
        public async Task<IActionResult> ArchiveStaffableAsRoleForPromotedEmployees()
        {
            var result = await _workdayPollingService.ArchiveStaffableAsRoleForPromotedEmployees();
            
            return Ok(result);
        }

        /// <summary>
        /// Update Capacity Analysis Monthly table for changes in Practice Affiliations of employees
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("updateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations")]
        public async Task<IActionResult> updateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations()
        {
            await _workdayPollingService.UpdateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations();

            return Ok();
        }

        /// <summary>
        ///    Save workday employee certifications in DB to be used in Coveo
        /// </summary>
        [HttpPost]
        [Route("saveWorkdayEmployeesCertificationsToDB")]
        public async Task<IActionResult> SaveWorkdayEmployeesCertificationsToDB()
        {
            var result = await _workdayPollingService.SaveWorkdayEmployeesCertificationsToDB();
            return Ok(result);
        }

        /// <summary>
        ///    Save workday employees languages in DB to be used in Coveo
        /// </summary>
        [HttpPost]
        [Route("saveWorkdayEmployeesLanguagesToDB")]
        public async Task<IActionResult> SaveWorkdayEmployeesLanguagesToDB()
        {
            var result = await _workdayPollingService.SaveWorkdayEmployeesLanguagesToDB();
            return Ok(result);
        }


    }
}