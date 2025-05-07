using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CCMPollingController : ControllerBase
    {
        private readonly ICCMPollingService _ccmPollingService;

        public CCMPollingController(ICCMPollingService ccmPollingService)
        {
            _ccmPollingService = ccmPollingService;
        }

        [HttpPost]
        [Route("updatePrePostAllocations")]
        public async Task<IActionResult> UpdatePrePostAllocationsForEndDateChangeInCCM()
        {
            await _ccmPollingService.UpdatePrePostAllocationsForEndDateChangeInCCM();
            return Ok();
        }

        //This polling job is used to process all the cases that were rolled. These cases are polled and out of these the ones whose end date have chanegd in CCM are processed
        [HttpPost]
        [Route("updateCaseRollAllocationsFromCCM")]
        public async Task<IActionResult> UpdateCaseRollAllocationsFromCCM()
        {
            var updatedCaseRolls = await _ccmPollingService.UpdateCaseRollAllocationsFromCCM();
            return Ok(updatedCaseRolls);
        }

        //This polling job is used to process all the cases that were rolled. These cases are polled and out of these the ones whose end date have NOT changed in CCM even after their expected end date has passed, are processed
        [HttpPost]
        [Route("updateCaseRollAllocationsNotUpdatedFromCCM")]
        public async Task<IActionResult> UpdateCaseRollAllocationsNotUpdatedFromCCM()
        {
            await _ccmPollingService.UpdateCaseRollAllocationsNotUpdatedFromCCM();
            return Ok();
        }

        /// <summary>
        ///  Update old case code, client code, case code for all opportunity transactions that converted to case 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("convertOpportunityToCase")]
        public async Task<IActionResult> ConvertOpportunityToCase()
        {
            var convertedOpps = await _ccmPollingService.ConvertOpportunityToCase();
            return Ok(convertedOpps);
        }

        [HttpPost]
        [Route("updateAllocationsForPreponedCasesFromCCM")]
        public async Task<IActionResult> UpdateAllocationsForPreponedCasesFromCCM()
        {
            await _ccmPollingService.UpdateAllocationsForPreponedCasesFromCCM();
            return Ok();
        }

        [HttpPost]
        [Route("upsertCaseMasterAndCaseMasterHistoryFromCCM")]
        public async Task<IActionResult> UpsertCaseMasterAndCaseMasterHistoryFromCCM()
        {
            await _ccmPollingService.UpsertCaseMasterAndCaseMasterHistoryFromCCM();
            return Ok();
        }

        /// <summary>
        /// Upserts Case Additonal Info in basis and analytics DB incrementally or full load based on params
        /// </summary>
        /// <param name="isFullLoad">[True]: If need full refresh of table, [False]: For incremental</param>
        /// <param name="lastUpdated">[Optional] Gets cases updated after the specified date</param>
        /// <returns></returns>
        //TODO: get data  via polling . Right now it uses direct basis joins to pupulate thie table
        [HttpPost]
        [Route("upsertCaseAdditionalInfoFromCCM")]
        public async Task<IActionResult> UpsertCaseAdditionalInfoFromCCM(bool isFullLoad, DateTime? lastUpdated)
        {
            var successMsgString =  await _ccmPollingService.UpsertCaseAdditionalInfoFromCCM(isFullLoad, lastUpdated);
            return Ok(successMsgString);
        }

        /// <summary>
        /// This API is used to upsert currency rates from basis database in Analytics database
        /// 
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost]
        [Route("upsertcurrencyrate")]
        public async Task<IActionResult> UpsertCurrencyRate(DateTime? effectiveFromDate)
        {
            await _ccmPollingService.UpsertCurrencyRates(effectiveFromDate);
            return Ok();
        }

        [HttpPost]
        [Route("upsertcaseattribute")]
        public async Task<IActionResult> UpsertCaseAttribute(DateTime? lastUpdatedDate)
        {
            await _ccmPollingService.UpsertCaseAttributes(lastUpdatedDate);
            return Ok();
        }

        /// <summary>
        /// This API is used to update the cost data in the Analytics database tables for changed currencies after last polled time 
        /// 
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost]
        [Route("updateUSDCostForCurrencyRateChangedRecently")]
        public async Task<IActionResult> UpdateUSDCostForCurrencyRateChangedRecently(DateTime? lastUpdated)
        {
            await _ccmPollingService.UpdateUSDCostForCurrencyRateChangedRecently(lastUpdated);
            return Ok();
        }

        [HttpPost]
        [Route("updateCaseEndDateFromCCMInCasePlannigBoard")]
        public async Task<IActionResult> UpdateCaseEndDateFromCCMInCasePlannigBoard(DateTime? lastUpdated)
        {
            var updatedCasesInCasePlanningBoard = await _ccmPollingService.UpdateCaseEndDateFromCCMInCasePlanningBoard(lastUpdated);
            return Ok(updatedCasesInCasePlanningBoard);
        }

        [HttpPost]
        [Route("correctAllocationsNotConvertedToPrePostAfterCaseRollProcessed")]
        public async Task<IActionResult> CorrectAllocationsNotConvertedToPrePostAfterCaseRollProcessed()
        {
            var updatedCases = await _ccmPollingService.CorrectAllocationsNotConvertedToPrePostAfterCaseRollProcessed();
            return Ok(updatedCases);

        }



    }
}