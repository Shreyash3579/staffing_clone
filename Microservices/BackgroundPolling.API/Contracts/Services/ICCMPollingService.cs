using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface ICCMPollingService
    {
        [SkipWhenPreviousJobIsRunning]
        //Task<IEnumerable<CaseChangedModel>> GetCasesOnCaseRoll();
        Task<IEnumerable<ScheduleMaster>> UpdateCaseRollAllocationsFromCCM();
        Task<IEnumerable<ScheduleMaster>> UpdateCaseRollAllocationsNotUpdatedFromCCM();
        Task<IEnumerable<ResourceAssignmentViewModel>> UpdatePrePostAllocationsForEndDateChangeInCCM();
        [SkipWhenPreviousJobIsRunning]
        Task<string> ConvertOpportunityToCase();
        Task<IEnumerable<ResourceAssignmentViewModel>> UpdateAllocationsForPreponedCasesFromCCM();
        Task UpsertCaseMasterAndCaseMasterHistoryFromCCM();
        [SkipWhenPreviousJobIsRunning]
        Task<string> UpsertCaseAdditionalInfoFromCCM(bool isFullLoad, DateTime? lastUpdated);
        Task UpsertCurrencyRates(DateTime? effectiveFromDate);
        Task UpsertCaseAttributes(DateTime? lastUpdatedDate);
        Task UpdateUSDCostForCurrencyRateChangedRecently(DateTime? lastUpdated);
        Task<IEnumerable<string>> UpdateCaseEndDateFromCCMInCasePlanningBoard(DateTime? lastUpdated);
        Task<string> CorrectAllocationsNotConvertedToPrePostAfterCaseRollProcessed();
    }
}