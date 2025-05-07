using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class CCMPollingService : ICCMPollingService
    {
        private readonly ICcmApiClient _ccmApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IHttpAggregatorClient _httpAggregatorClient;
        private readonly ICCMPollingRepository _CCMPollingRepository;
        private readonly IPipelineApiClient _pipelineApiClient;
        private readonly IPollMasterRepository _pollMasterRepostory;
        private readonly IBasisApiClient _basisApiClient;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public CCMPollingService(ICcmApiClient ccmApiClient, IStaffingApiClient staffingApiClient,
            IHttpAggregatorClient httpAggregatorClient, ICCMPollingRepository ccmPollingRepository,
            IPipelineApiClient pipelineApiClient, IPollMasterRepository pollMasterRepostory, IBackgroundJobClient backgroundJobClient,
            IBasisApiClient basisApiClient)
        {
            _ccmApiClient = ccmApiClient;
            _staffingApiClient = staffingApiClient;
            _httpAggregatorClient = httpAggregatorClient;
            _CCMPollingRepository = ccmPollingRepository;
            _pipelineApiClient = pipelineApiClient;
            _pollMasterRepostory = pollMasterRepostory;
            _basisApiClient = basisApiClient;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> UpdatePrePostAllocationsForEndDateChangeInCCM()
        {
            var prePostAllocationsOnOrAfterToday = await _staffingApiClient.GetAllocationsForEmployeesOnPrePost();

            if (prePostAllocationsOnOrAfterToday.Count() > 0)
            {
                var newAllocations = new List<ScheduleMaster>();

                var listOldCaseCode = string.Join(",", prePostAllocationsOnOrAfterToday.Select(ppc => ppc.OldCaseCode).Distinct());
                var casesFromCCM = await _ccmApiClient.GetCaseDataByCaseCodes(listOldCaseCode);

                foreach (var ccmCase in casesFromCCM)
                {
                    var prePostAllocationsOnSpecificCase = prePostAllocationsOnOrAfterToday.Where(ppc => ppc.OldCaseCode == ccmCase.OldCaseCode);

                    foreach (var prePostAllocation in prePostAllocationsOnSpecificCase)
                    {
                        if (isPrePostOverlapsWithCaseDuration(prePostAllocation.StartDate, prePostAllocation.EndDate, ccmCase.StartDate, ccmCase.EndDate))
                        {
                            if (IsPrePostEndsBetweenCaseDuration(prePostAllocation.StartDate, prePostAllocation.EndDate, ccmCase.StartDate, ccmCase.EndDate))
                            {
                                newAllocations.Add(CreateNewAllocation(
                                   prePostAllocation, Convert.ToDateTime(prePostAllocation.StartDate), ccmCase.StartDate.AddDays(-1), 4));
                                newAllocations.Add(ConvertToRegularAllocation(prePostAllocation, ccmCase.StartDate, prePostAllocation.EndDate.Value.Date));

                            }
                            else if (IsPrePostStartsAndEndsWithinCaseDuration(prePostAllocation.StartDate, prePostAllocation.EndDate, ccmCase.StartDate, ccmCase.EndDate))
                            {
                                newAllocations.Add(ConvertToRegularAllocation(prePostAllocation, prePostAllocation.StartDate, prePostAllocation.EndDate));
                            }
                            else if (IsPrePostStartsBetweenCaseDuration(prePostAllocation.StartDate, prePostAllocation.EndDate, ccmCase.StartDate, ccmCase.EndDate))
                            {
                                newAllocations.Add(CreateNewAllocation(
                                   prePostAllocation, ccmCase.EndDate.AddDays(1), Convert.ToDateTime(prePostAllocation.EndDate), 4));
                                newAllocations.Add(ConvertToRegularAllocation(prePostAllocation, prePostAllocation.StartDate.Value.Date, ccmCase.EndDate));

                            }
                        }
                    }
                }

                if (newAllocations.Count() > 0)
                {
                    var updatedAllocations = await _httpAggregatorClient.UpsertResourceAllocations(newAllocations);
                    return updatedAllocations;
                }
            }

            return Enumerable.Empty<ResourceAssignmentViewModel>();
        }

        public async Task<IEnumerable<ScheduleMaster>> UpdateCaseRollAllocationsFromCCM()
        {
            var lastUpdatedBy = "Auto-CCM";

            // TODO: update this once CCM polling using Azure Bus service is done.
            #region CCM Polling
            var unprocessedCasesOnCaseRoll = await _staffingApiClient.GetAllUnprocessedCasesOnCaseRoll();
            if (!unprocessedCasesOnCaseRoll.Any())
                return Enumerable.Empty<ScheduleMaster>();

            var listOldCaseCodes = string.Join(",", unprocessedCasesOnCaseRoll.ToList().Select(un => un.RolledFromOldCaseCode).Distinct());

            // Invoking CCM and
            // delete any allocation from mapping table that were manually updated by user and need not be tracked with CCM
            var casesCCMDataTask = _ccmApiClient.GetCaseDataByCaseCodes(listOldCaseCodes);
            var deleteTrackingTask = _staffingApiClient.DeleteRolledAllocationsMappingFromCaseRollTracking(lastUpdatedBy, listOldCaseCodes);

            await Task.WhenAll(casesCCMDataTask, deleteTrackingTask);

            var casesCCMData = casesCCMDataTask.Result;

            #endregion CCM Polling

            // Create the updated case Roll object using CCM
            var updatedCaseRolls = new List<CaseRoll>();

            updatedCaseRolls = (from ccmCase in casesCCMData
                                join rolledCase in unprocessedCasesOnCaseRoll on ccmCase.OldCaseCode equals rolledCase.RolledFromOldCaseCode
                                where ccmCase.EndDate.Date != rolledCase.CurrentCaseEndDate.Value.Date
                                select new CaseRoll()
                                {
                                    Id = rolledCase.Id,
                                    RolledFromOldCaseCode = rolledCase.RolledFromOldCaseCode,
                                    RolledToOldCaseCode = rolledCase.RolledToOldCaseCode,
                                    CurrentCaseEndDate = ccmCase.EndDate.Date, //set the current roll date from CCM
                                    ExpectedCaseEndDate = rolledCase.ExpectedCaseEndDate,
                                    RolledScheduleIds = rolledCase.RolledScheduleIds,
                                    IsProcessedFromCCM = true, //Set the record as processed
                                    LastUpdatedBy = lastUpdatedBy
                                }).ToList();

            if (updatedCaseRolls.ToList().Count == 0)
                return Enumerable.Empty<ScheduleMaster>();

            // get all allocation data corresponding to caseRoll data

            var listCasesUpdatedInCCM = string.Join(",", updatedCaseRolls.ToList().Select(c => c.RolledFromOldCaseCode));
            var allAllocationsByCases = await _staffingApiClient.GetResourceAllocationsOnCaseRollByCaseCodes(listCasesUpdatedInCCM);

            var allocationsToUpsert = CreateResourceAllocationsToUpsert(allAllocationsByCases, updatedCaseRolls);

            var upsertedCaseRolls = await _httpAggregatorClient.UpsertCaseRollsAndAllocations(updatedCaseRolls, allocationsToUpsert);


            var casesRolledInCcmHavingEndDateGreaterThanExpectedEndDate = updatedCaseRolls.Where(x => x.CurrentCaseEndDate > x.ExpectedCaseEndDate);
            if (casesRolledInCcmHavingEndDateGreaterThanExpectedEndDate.Any())
            {

                // update prepost allocation once CCM has updated the end Date
                await UpdatePrePostAllocationsForEndDateChangeInCCM();
                await DeleteOverlappedAllocations(casesRolledInCcmHavingEndDateGreaterThanExpectedEndDate);
            }

            return allocationsToUpsert;
        }

        public async Task<IEnumerable<ScheduleMaster>> UpdateCaseRollAllocationsNotUpdatedFromCCM()
        {
            var lastUpdatedBy = "Auto-CCM";
            var unprocessedCasesOnCaseRoll = await _staffingApiClient.GetAllUnprocessedCasesOnCaseRoll();

            //filter cases whose expected case end date has passed (giving 3 days leeway for CCM to make changes after the date has passed)
            var updatedCaseRolls = unprocessedCasesOnCaseRoll.Where(x => x.ExpectedCaseEndDate?.Date <= DateTime.Today.AddDays(-3));

            if (!updatedCaseRolls.Any())
                return Enumerable.Empty<ScheduleMaster>();

            foreach (var caseRoll in updatedCaseRolls)
            {
                caseRoll.IsProcessedFromCCM = true;
                caseRoll.LastUpdatedBy = lastUpdatedBy;
            }
            var listOldCaseCodes = string.Join(",", updatedCaseRolls.ToList().Select(un => un.RolledFromOldCaseCode).Distinct());

            // delete any allocation from mapping table that were manually updated by user and need not be tracked with CCM
            await _staffingApiClient.DeleteRolledAllocationsMappingFromCaseRollTracking(lastUpdatedBy, listOldCaseCodes);

            // get all allocation data corresponding to changed cases in CCM

            var allAllocationsByCases = await _staffingApiClient.GetResourceAllocationsOnCaseRollByCaseCodes(listOldCaseCodes);

            var allocationsToUpsert = CreateResourceAllocationsToUpsert(allAllocationsByCases, updatedCaseRolls);

            var upsertedCaseRolls = await _httpAggregatorClient.UpsertCaseRollsAndAllocations(updatedCaseRolls, allocationsToUpsert);

            return allocationsToUpsert;

        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> UpdateAllocationsForPreponedCasesFromCCM()
        {
            var updatedAllocations = Enumerable.Empty<ResourceAssignmentViewModel>();

            // Get the cases whose end date has been updated in CCM
            var lastPolledDateTime = await _pollMasterRepostory.GetLastPolledTimeStampFromStaffingDB(Constants.CasesPreponedRecentlyInCCM);

            var casesWithEndDatesUpdatedInCCM = await _ccmApiClient.GetCasesWithEndDateUpdatedInCCM(lastPolledDateTime);

            //Filter the cases that have been preponed in CCM
            var preponedCases = casesWithEndDatesUpdatedInCCM?.Where(x => x.EndDate < x.OriginalEndDate
            || (x.OriginalEndDate == null && x.EndDate != null)).ToList();

            if (preponedCases.Count > 0)
            {
                var listOldCaseCodes = string.Join(",", preponedCases.Select(x => x.OldCaseCode).Distinct());
                var preponedCasesAllocations = await _staffingApiClient.GetResourceAllocationsByCaseCodes(listOldCaseCodes);

                if (preponedCasesAllocations.Count > 0)
                {
                    var allocationIdsToBeDeleted = new List<Guid?>();
                    var allocationsToBeUpdated = new List<ResourceAssignmentViewModel>();
                    var auditData = new List<PreponedCasesAllocationsAudit>();

                    foreach (var preponedCase in preponedCases)
                    {
                        var updatedCaseAllocations = new List<ResourceAssignmentViewModel>();
                        var allocationsToBeDeleted = new List<ResourceAssignmentViewModel>();
                        var allocationOnSpecificCase = preponedCasesAllocations.Where(ppc => ppc.OldCaseCode == preponedCase.OldCaseCode);
                        allocationsToBeDeleted = allocationOnSpecificCase.Where(x => x.StartDate > preponedCase.EndDate).ToList();
                        allocationIdsToBeDeleted.AddRange(allocationsToBeDeleted.Select(y => y.Id));

                        foreach (var allocation in allocationOnSpecificCase.Where(allocation => allocation.StartDate <= preponedCase.EndDate && allocation.EndDate > preponedCase.EndDate))
                        {
                            allocation.EndDate = preponedCase.EndDate;
                            allocation.LastUpdatedBy = "Auto-CaseEndEarly";

                            updatedCaseAllocations.Add(allocation);
                        }

                        auditData.AddRange(ConvertToPreponedCasesAllocationsAudit(updatedCaseAllocations.Concat(allocationsToBeDeleted), preponedCase));
                        allocationsToBeUpdated.AddRange(updatedCaseAllocations);
                    }

                    if (allocationIdsToBeDeleted.Count() > 0)
                    {
                        var allocationsIds = string.Join(",", allocationIdsToBeDeleted.Distinct());

                        await _staffingApiClient.DeleteResourceAllocationByIds(allocationsIds, "Auto-CaseEndEarly");
                    }

                    if (allocationsToBeUpdated.Count() > 0)
                    {
                        updatedAllocations = await _httpAggregatorClient.UpsertResourceAllocations(allocationsToBeUpdated);
                    }
                    await _pollMasterRepostory.UpsertPollMasterOnStaffingDB(Constants.CasesPreponedRecentlyInCCM, DateTime.Now);
                    
                    if(auditData.Count() > 0)
                    {
                        await _staffingApiClient.UpsertPreponedCaseAllocationsAudit(auditData);
                    }
                }
            }
            return updatedAllocations;
        }

        public async Task<string> CorrectAllocationsNotConvertedToPrePostAfterCaseRollProcessed()
        {
            var updatedAllocations = Enumerable.Empty<ResourceAssignmentViewModel>();

            // Get the cases whose end date has been updated in CCM
            var lastPollDateTime = await _pollMasterRepostory.GetLastPolledTimeStampFromStaffingDB(Constants.CaseRollProcessedRecentlyInStaffing);
            var now = DateTime.Now;
            var lastUpdatedBy = "Auto-CCM";

            var listRecentlyProcessedCaseRollCaseCodes = await _staffingApiClient.GetCaseRollsRecentlyProcessedInStaffing(lastPollDateTime);

            if (listRecentlyProcessedCaseRollCaseCodes.Length > 0)
            {
                var listOldCaseCodes = listRecentlyProcessedCaseRollCaseCodes;

                var casesCCMDataTask = _ccmApiClient.GetCaseDataByCaseCodes(listOldCaseCodes);
                var recentlyProcessedCaseRollAllocationsTask = _staffingApiClient.GetResourceAllocationsByCaseCodes(listOldCaseCodes);

                await Task.WhenAll(casesCCMDataTask, recentlyProcessedCaseRollAllocationsTask);

                var casesCCMData = casesCCMDataTask.Result;
                var recentlyProcessedCaseRollAllocations = recentlyProcessedCaseRollAllocationsTask.Result;

                if (recentlyProcessedCaseRollAllocations.Count > 0)
                {
                    var allocationsToUpsert = new List<ResourceAssignmentViewModel>();

                    foreach (var caseData in casesCCMData)
                    {
                        var allocationsNotConvertedToPrePost = recentlyProcessedCaseRollAllocations
                            .Where(x => x.OldCaseCode == caseData.OldCaseCode && x.EndDate > caseData.EndDate && x.InvestmentCode != (short)Constants.InvestmentTypeCodes.PrePost);

                        if (allocationsNotConvertedToPrePost.Count() > 0)
                        {
                            foreach (var allocation in allocationsNotConvertedToPrePost)
                            {
                                if (IsCcmCaseEndsAfterAllocationStarts(allocation.StartDate, caseData.EndDate))
                                {
                                    //Create Pre-Post
                                    var newAllocation = allocation.Clone();
                                    newAllocation.Id = null;
                                    newAllocation.StartDate = caseData.EndDate.AddDays(1);
                                    newAllocation.EndDate = allocation.EndDate;
                                    newAllocation.InvestmentCode = (short)Constants.InvestmentTypeCodes.PrePost;
                                    newAllocation.LastUpdatedBy = lastUpdatedBy;

                                    //Update end date of existing to case end date
                                    allocation.EndDate = caseData.EndDate;
                                    allocation.LastUpdatedBy = lastUpdatedBy;

                                    allocationsToUpsert.Add(allocation);
                                    allocationsToUpsert.Add(newAllocation);
                                }
                                else if (IsCcmCaseEndsBeforeAllocationStarts(allocation.StartDate, caseData.EndDate))
                                {
                                    allocation.LastUpdatedBy = lastUpdatedBy;
                                    allocation.InvestmentCode = (short)Constants.InvestmentTypeCodes.PrePost;
                                    allocationsToUpsert.Add(allocation);
                                }
                            }

                        }

                    }

                    if (!allocationsToUpsert.Any())
                    {
                        return string.Empty;
                    }

                    await _httpAggregatorClient.UpsertResourceAllocations(allocationsToUpsert);
                    await _pollMasterRepostory.UpsertPollMasterOnStaffingDB(Constants.CaseRollProcessedRecentlyInStaffing, now);
                }
            }
            return string.Join(",", updatedAllocations.Select(x => x.OldCaseCode).Distinct());
        }

        public async Task<string> ConvertOpportunityToCase()
        {

            var opportunitiesNotConvertedToCase = await _CCMPollingRepository.GetOpportunitiesNotConvertedToCase();
            var opportunitiesPinnedByUsersNotConvertedToCase = await _CCMPollingRepository.GetOpportunitiesPinnedByUsers();

            var opportunities = new List<Guid>();
            opportunities.AddRange(opportunitiesNotConvertedToCase);
            opportunities.AddRange(opportunitiesPinnedByUsersNotConvertedToCase);

            var distinctOpportunitiesNotConvertedToCase = opportunities.Distinct().ToList();

            if (distinctOpportunitiesNotConvertedToCase.Count <= 0) return string.Empty;

            var pipelineIds = string.Join(",", distinctOpportunitiesNotConvertedToCase);
            var casesToWhichOpportunityConverted = await _ccmApiClient.GetCasesByPipelineIds(pipelineIds);
            if (casesToWhichOpportunityConverted.Count <= 0) return string.Empty;

            var convertedPipelineIds = string.Join(",", casesToWhichOpportunityConverted.Select(x => x.PipelineId));

            var allocationsOnOpportuntiesConvertedToCaseTask = _staffingApiClient.GetResourceAllocationsByPipelineIds(convertedPipelineIds);
            var palceholderAllocationsOnOpportuntiesConvertedToCaseTask = _staffingApiClient.GetPlaceholderAllocationsByPipelineIds(convertedPipelineIds);

            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(convertedPipelineIds);
            var convertedOpportunitiesTask = _pipelineApiClient.GetOpportunitiesByPipelineIds(convertedPipelineIds);

            //get pricing team size here
            var oppDataWithPricingTeamSizeTask = _staffingApiClient.GetOppCortexPlaceholderInfoByPipelineIds(convertedPipelineIds);

            await Task.WhenAll(allocationsOnOpportuntiesConvertedToCaseTask, pipelineChangesTask,
                convertedOpportunitiesTask, oppDataWithPricingTeamSizeTask);

            var allocationsOnOpportuntiesConvertedToCase = allocationsOnOpportuntiesConvertedToCaseTask.Result;
            var palceholderAllocationsOnOpportuntiesConvertedToCase = palceholderAllocationsOnOpportuntiesConvertedToCaseTask.Result;
            var pipelineChanges = pipelineChangesTask.Result;
            var convertedOpportuntiesData = convertedOpportunitiesTask.Result;
            var oppDataWithPricingTeamSize = oppDataWithPricingTeamSizeTask.Result;

            pipelineChanges.Join(convertedOpportuntiesData, (pipelineChange) => pipelineChange.PipelineId, (opportunity) => opportunity.PipelineId, (pipelineChange, opportunity) =>
            {
                opportunity.StartDate = pipelineChange.StartDate ?? opportunity.StartDate;
                opportunity.EndDate = pipelineChange.EndDate ?? opportunity.EndDate;
                opportunity.ProbabilityPercent = pipelineChange.ProbabilityPercent ?? opportunity.ProbabilityPercent;
                opportunity.CaseServedByRingfence = pipelineChange.CaseServedByRingfence;
                return opportunity;
            }).ToList();

            casesToWhichOpportunityConverted.Join(convertedOpportuntiesData, (caseData) => caseData.PipelineId, (opportunity) => opportunity.PipelineId, (caseData, opportunity) =>
            {
                caseData.CortexOpportunityId = opportunity.CortexOpportunityId;
                caseData.EstimatedTeamSize = opportunity.EstimatedTeamSize;
                caseData.CaseServedByRingfence = opportunity.CaseServedByRingfence;
                return caseData;
            }).ToList();

            casesToWhichOpportunityConverted.Join(oppDataWithPricingTeamSize, (caseData) => caseData.PipelineId, (opportunity) => opportunity.PipelineId, (caseData, opportunity) =>
            {
                caseData.PricingTeamSize = opportunity.PricingTeamSize;
                return caseData;
            }).ToList();

            var opportunityCaseMapDto = GetOpportunityCaseMapDto(casesToWhichOpportunityConverted);

            var allocationsConvertingToCase = UpdateAllocationEndDateToCaseEndDate(convertedOpportuntiesData, allocationsOnOpportuntiesConvertedToCase, casesToWhichOpportunityConverted);
            var placeholderAllocationsConvertingToCase = UpdatePlaceholderAllocationWithCaseData(convertedOpportuntiesData, palceholderAllocationsOnOpportuntiesConvertedToCase, casesToWhichOpportunityConverted);
            var allocationsToUpsert = CheckAndSplitAllocationForPrePostRevenue(allocationsConvertingToCase, casesToWhichOpportunityConverted);
            await _httpAggregatorClient.UpsertResourceAllocations(allocationsToUpsert);
            await _staffingApiClient.checkPegRingfenceAllocationAndInsertDownDayCommitments(ConvertToResourceAssignmentViewModel(allocationsConvertingToCase));
            //TODO: Create endpoint to upsert caseoppchanges:caseservedbyringfence in caseoppchangestable  
            await _httpAggregatorClient.UpsertResourcePlaceholderAllocations(placeholderAllocationsConvertingToCase);
            await _CCMPollingRepository.UpdateRingfenceForOpportunitiesConvertedToCase(opportunityCaseMapDto);
            await _CCMPollingRepository.UpdateSkuTermsForOpportunitiesConvertedToCase(opportunityCaseMapDto);
            await _CCMPollingRepository.UpdateUserPreferencesForOpportunitiesConvertedToCase(opportunityCaseMapDto);

            return convertedPipelineIds;
        }

        public async Task UpsertCaseMasterAndCaseMasterHistoryFromCCM()
        {
            var lastPolledDateTime = await _pollMasterRepostory.GetLastPolledTimeStampFromAnalyticsDB(Constants.CaseMasterAndCaseMasterHistoryUpdatedRecentlyInBasis);
            var caseMasterAndCaseMasterHistoryDataChanges = await _ccmApiClient.GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(lastPolledDateTime);
            if (caseMasterAndCaseMasterHistoryDataChanges.CaseMaster.Any() || caseMasterAndCaseMasterHistoryDataChanges.CaseMasterHistory.Any())
            {
                var caseMasterDataTable = ConvertToCaseMasterDataTable(caseMasterAndCaseMasterHistoryDataChanges.CaseMaster);
                var caseMasterHistoryDataTable = ConvertToCaseMasterHistoryDataTable(caseMasterAndCaseMasterHistoryDataChanges.CaseMasterHistory);

                DateTime? previousPolledDateTime = lastPolledDateTime;
                if (lastPolledDateTime == DateTime.MinValue)
                {
                    previousPolledDateTime = null;
                }

                await _CCMPollingRepository.UpsertCaseMasterAndCaseMasterHistory(caseMasterDataTable, caseMasterHistoryDataTable, previousPolledDateTime);

                var updatedLastPolledTime = caseMasterAndCaseMasterHistoryDataChanges.CaseMasterHistory?.Max(cmh => cmh?.SysEndTime) ?? lastPolledDateTime;
                await _pollMasterRepostory.UpsertPollMasterOnAnalyticsDB(Constants.CaseMasterAndCaseMasterHistoryUpdatedRecentlyInBasis, updatedLastPolledTime);
            }
        }

        public async Task<string> UpsertCaseAdditionalInfoFromCCM(bool isFullLoad, DateTime? lastUpdated)
        {
            var returnMsgStringForCases = string.Empty;

            DateTime? lastPolledDateTime = lastUpdated.HasValue
                ? lastUpdated.Value
                : await _pollMasterRepostory.GetLastPolledTimeStampFromStaffingDB(Constants.CaseAdditionalInfo);

            if(isFullLoad || lastPolledDateTime == DateTime.MinValue)
            {
                lastPolledDateTime = null;
            }

            var now = DateTime.UtcNow; //saving UTC date in Poll Master as we are comparing with sysstarttime to get incremental data which is store in UTC

            var caseAdditionalInfos = await _ccmApiClient.GetCaseAdditionalInfo(lastPolledDateTime);

            if (caseAdditionalInfos.Any())
            {
                var caseAdditionalInfoDataTable = ConvertToCaseAdditionalInfoDataTable(caseAdditionalInfos);

                await _CCMPollingRepository.UpsertCaseAdditionalInfo(caseAdditionalInfoDataTable, isFullLoad);
                await _CCMPollingRepository.UpsertCaseAdditionalInfoInBasis(caseAdditionalInfoDataTable, isFullLoad);
            }

            if(lastPolledDateTime is not null)
            {
                returnMsgStringForCases = string.Join(",", caseAdditionalInfos.Select(x => x.OldCaseCode).Distinct());
            }
            
            await _pollMasterRepostory.UpsertPollMasterOnStaffingDB(Constants.CaseAdditionalInfo, now);
            return returnMsgStringForCases ?? "Full Load Success";
        }

        public async Task<IEnumerable<string>> UpdateCaseEndDateFromCCMInCasePlanningBoard(DateTime? lastUpdated)
        {
            var lastPolledDateTime = lastUpdated.HasValue
                ? lastUpdated.Value
                : await _pollMasterRepostory.GetLastPolledTimeStampFromStaffingDB(Constants.CaseEndDateUpdatedInCCM);

            if (lastPolledDateTime == DateTime.MinValue)
            {
                lastPolledDateTime = Convert.ToDateTime(ConfigurationUtility.GetValue("SqlMinDate"));
            }

            var modifiedCases = await _ccmApiClient.GetModifiedCasesAfterLastPolledTime(lastPolledDateTime);
            var updatedOldCaseCodes = Enumerable.Empty<string>();

            if (modifiedCases.Any())
            {
                var modifiedOldCaseCodes = string.Join(",", modifiedCases.Select(mc => mc.OldCaseCode).Distinct());
                var newPollTimestamp = modifiedCases.Max(mc => mc.LastUpdated.Value);

                var caseDataInCasePlanningBoard = await _staffingApiClient.GetCasePlanningBoardDataByProjectIds(modifiedOldCaseCodes, null, null);

                if (caseDataInCasePlanningBoard.Any())
                {
                    foreach (var caseInPlanningboard in caseDataInCasePlanningBoard)
                    {
                        var modifiedCase = modifiedCases.FirstOrDefault(x => x.OldCaseCode.Equals(caseInPlanningboard.OldCaseCode)
                            && !x.EndDate.Equals(caseInPlanningboard.ProjectEndDate));

                        if (modifiedCase != null)
                        {
                            caseInPlanningboard.ProjectEndDate = modifiedCase.EndDate;
                            caseInPlanningboard.LastUpdatedBy = "Auto-CCM-EndDate-Change";
                        }
                    }
                    var casesWithUpdatedEndDate = await _staffingApiClient.UpsertCasePlanningBoardData(caseDataInCasePlanningBoard);
                    updatedOldCaseCodes = casesWithUpdatedEndDate.Select(ppc => ppc.OldCaseCode).Distinct();
                }
                await _pollMasterRepostory.UpsertPollMasterOnStaffingDB(Constants.CaseEndDateUpdatedInCCM, newPollTimestamp);
            }

            return updatedOldCaseCodes;
        }
        #region Method for Upsert currencyRate data for Capacity Breakdown

        public async Task UpsertCurrencyRates(DateTime? effectiveFromDate)
        {
            var currencyRates = await _basisApiClient.GetCurrencyRatesByEffectiveDate(effectiveFromDate);
            if (currencyRates.Count > 0)
            {
                var currencyDataTable = ConvertToCurrencyDataTable(currencyRates);
                await _CCMPollingRepository.UpsertCurrencyRates(currencyDataTable);
            }
        }
        public async Task UpsertCaseAttributes(DateTime? lastUpdatedDate)
        {
            var caseAttributes = await _ccmApiClient.GetCaseAttributesByLastUpdatedDate(lastUpdatedDate);
            if (caseAttributes.Count > 0)
            {
                var caseAttributeDataTable = ConvertToCaseAttributeDataTable(caseAttributes);
                await _CCMPollingRepository.UpsertCaseAttributes(caseAttributeDataTable);
            }
        }

        public async Task UpdateUSDCostForCurrencyRateChangedRecently(DateTime? lastUpdated)
        {
            var lastPolledDateTime = lastUpdated.HasValue
                ? lastUpdated.Value
                : await _pollMasterRepostory.GetLastPolledTimeStampFromAnalyticsDB(Constants.CurrencyRateUpdatedInCCM);

            var minSqlDate = Convert.ToDateTime(ConfigurationUtility.GetValue("SqlMinDate"));
            var minAnalyticsDate = Convert.ToDateTime(ConfigurationUtility.GetValue("AnalyticsMinDate"));

            lastPolledDateTime = lastPolledDateTime < minSqlDate ? minSqlDate : lastPolledDateTime;

            var currencyRates = await _CCMPollingRepository.GetCurrencyRatesChangedRecently(lastPolledDateTime);

            if (currencyRates.Any())
            {

                var newPollTimestamp = currencyRates.Max(cr => cr.TargetLastUpdated);

                currencyRates = currencyRates.Where(cr => cr.EffectiveDate >= minAnalyticsDate && cr.CurrencyRateTypeCode == "B");

                if (currencyRates.ToList().Count > 0)
                {
                    var currencyDataTable = ConvertToCurrencyDataTable(currencyRates.ToList());
                    await _CCMPollingRepository.UpdateUSDCostForChangeInCurrencyRate(currencyDataTable);
                    await _pollMasterRepostory.UpsertPollMasterOnAnalyticsDB(Constants.CurrencyRateUpdatedInCCM, newPollTimestamp);

                    /* *
                    * Trigger job to update records in capacity analysis daily table
                    * fot the changes happen in SMD and RA
                    * */
                    //_backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                    //     x.UpsertCapacityAnalysisDaily(false, null));
                }

            }
        }

        #endregion
        private static List<PreponedCasesAllocationsAudit> ConvertToPreponedCasesAllocationsAudit(IEnumerable<ResourceAssignmentViewModel> allocations, CaseViewModel preponedCase)
        {
            var auditData = allocations.Select(item => new PreponedCasesAllocationsAudit
            {
                Id = item.Id,
                CaseCode = item.CaseCode.Value,
                ClientCode = item.ClientCode,
                OldCaseCode = item.OldCaseCode,
                OriginalCaseStartDate = preponedCase.OriginalStartDate,
                UpdatedCaseStartDate = preponedCase.StartDate,
                OriginalCaseEndDate = preponedCase.OriginalEndDate,
                UpdatedCaseEndDate = preponedCase.EndDate,
                EmployeeCode = item.EmployeeCode,
                ServiceLineCode = item.ServiceLineCode,
                OperatingOfficeCode = Convert.ToInt16(item.OperatingOfficeCode.Value),
                CaseLastUpdatedBy = preponedCase.LastUpdatedBy,
                CaseLastUpdated = preponedCase.LastUpdated.Value,
                LastUpdatedBy = "Auto-CaseEndEarly"
            }).ToList();

            return auditData;
        }

        private DataTable ConvertToCurrencyDataTable(IList<CurrencyRate> currencyRates)
        {
            var currencyDataTable = new DataTable();
            currencyDataTable.Columns.Add("CurrencyCode", typeof(string));
            currencyDataTable.Columns.Add("CurrencyName", typeof(string));
            currencyDataTable.Columns.Add("CurrencyRateTypeCode", typeof(string));
            currencyDataTable.Columns.Add("CurrencyRateTypeName", typeof(string));
            currencyDataTable.Columns.Add("UsdRate", typeof(decimal));
            currencyDataTable.Columns.Add("ServiceCode", typeof(string));
            currencyDataTable.Columns.Add("EffectiveDate", typeof(string));

            foreach (var currencyRate in currencyRates)
            {
                var row = currencyDataTable.NewRow();

                row["CurrencyCode"] = (object)currencyRate.CurrencyCode ?? DBNull.Value;
                row["CurrencyName"] = (object)currencyRate.CurrencyName ?? DBNull.Value;
                row["CurrencyRateTypeCode"] = (object)currencyRate.CurrencyRateTypeCode ?? DBNull.Value;
                row["CurrencyRateTypeName"] = (object)currencyRate.CurrencyRateTypeName ?? DBNull.Value;
                row["UsdRate"] = (object)currencyRate.UsdRate ?? DBNull.Value;
                row["ServiceCode"] = (object)currencyRate.ServiceCode ?? DBNull.Value;
                row["EffectiveDate"] = (object)currencyRate.EffectiveDate ?? DBNull.Value;

                currencyDataTable.Rows.Add(row);
            }

            return currencyDataTable;
        }

        private DataTable ConvertToCaseAttributeDataTable(IList<CaseAttribute> caseAttributes)
        {
            var currencyDataTable = new DataTable();
            currencyDataTable.Columns.Add("clientCode", typeof(int));
            currencyDataTable.Columns.Add("caseCode", typeof(int));
            currencyDataTable.Columns.Add("oldCaseCode", typeof(string));
            currencyDataTable.Columns.Add("caseAttributeCode", typeof(int));
            currencyDataTable.Columns.Add("caseAttributeName", typeof(string));

            foreach (var caseAttribute in caseAttributes)
            {
                var row = currencyDataTable.NewRow();

                row["clientCode"] = (object)caseAttribute.clientCode ?? DBNull.Value;
                row["caseCode"] = (object)caseAttribute.caseCode ?? DBNull.Value;
                row["oldCaseCode"] = (object)caseAttribute.oldCaseCode ?? DBNull.Value;
                row["caseAttributeCode"] = (object)caseAttribute.caseAttributeCode ?? DBNull.Value;
                row["caseAttributeName"] = (object)caseAttribute.caseAttributeName ?? DBNull.Value;
                currencyDataTable.Rows.Add(row);
            }

            return currencyDataTable;
        }

        private DataTable ConvertToCaseAdditionalInfoDataTable(IEnumerable<CaseAdditionalInfo> caseAdditionalInfos)
        {
            var caseAdditionalInfoDataTable = new DataTable();
            caseAdditionalInfoDataTable.Columns.Add("caseCode", typeof(int));
            caseAdditionalInfoDataTable.Columns.Add("caseName", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("clientCode", typeof(int));
            caseAdditionalInfoDataTable.Columns.Add("clientName", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("oldCaseCode", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("caseTypeCode", typeof(int));
            caseAdditionalInfoDataTable.Columns.Add("caseTypeName", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("clientGroupCode", typeof(int));
            caseAdditionalInfoDataTable.Columns.Add("clientGroupName", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("primaryIndustry", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("primaryCapability", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("practiceAreaIndustryCode", typeof(int));
            caseAdditionalInfoDataTable.Columns.Add("practiceAreaIndustryAbbreviation", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("practiceAreaIndustry", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("practiceAreaCapabilityCode", typeof(int));
            caseAdditionalInfoDataTable.Columns.Add("practiceAreaCapabilityAbbreviation", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("practiceAreaCapability", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("pegCase", typeof(bool));
            caseAdditionalInfoDataTable.Columns.Add("caseManagerCode", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("caseManagerName", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("managingOfficeCode", typeof(int));
            caseAdditionalInfoDataTable.Columns.Add("managingOfficeAbbreviation", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("managingOfficeName", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("billingOfficeCode", typeof(int));
            caseAdditionalInfoDataTable.Columns.Add("billingOfficeAbbreviation", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("billingOfficeName", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("isPegCaseClass", typeof(bool));
            caseAdditionalInfoDataTable.Columns.Add("pegIndustryTermCode", typeof(int));
            caseAdditionalInfoDataTable.Columns.Add("pegIndustryTerm", typeof(string));
            caseAdditionalInfoDataTable.Columns.Add("pegIndustryAbbreviation", typeof(string));

            foreach (var caseAdditionalInfo in caseAdditionalInfos)
            {
                var row = caseAdditionalInfoDataTable.NewRow();

                row["caseCode"] = (object)caseAdditionalInfo.CaseCode ?? DBNull.Value;
                row["caseName"] = (object)caseAdditionalInfo.CaseName ?? DBNull.Value;
                row["clientCode"] = (object)caseAdditionalInfo.ClientCode ?? DBNull.Value;
                row["clientName"] = (object)caseAdditionalInfo.ClientName ?? DBNull.Value;
                row["oldCaseCode"] = (object)caseAdditionalInfo.OldCaseCode ?? DBNull.Value;
                row["caseTypeCode"] = (object)caseAdditionalInfo.CaseTypeCode ?? DBNull.Value;
                row["caseTypeName"] = (object)caseAdditionalInfo.CaseTypeName ?? DBNull.Value;
                row["clientGroupCode"] = (object)caseAdditionalInfo.ClientGroupCode ?? DBNull.Value;
                row["clientGroupName"] = (object)caseAdditionalInfo.ClientGroupName ?? DBNull.Value;
                row["primaryIndustry"] = (object)caseAdditionalInfo.PrimaryIndustry ?? DBNull.Value;
                row["primaryCapability"] = (object)caseAdditionalInfo.PrimaryCapability ?? DBNull.Value;
                row["practiceAreaIndustryCode"] = (object)caseAdditionalInfo.PracticeAreaIndustryCode ?? DBNull.Value;
                row["practiceAreaIndustryAbbreviation"] = (object)caseAdditionalInfo.PracticeAreaIndustryAbbreviation ?? DBNull.Value;
                row["practiceAreaIndustry"] = (object)caseAdditionalInfo.PracticeAreaIndustry ?? DBNull.Value;
                row["practiceAreaCapabilityCode"] = (object)caseAdditionalInfo.PracticeAreaCapabilityCode ?? DBNull.Value;
                row["practiceAreaCapabilityAbbreviation"] = (object)caseAdditionalInfo.PracticeAreaCapabilityAbbreviation ?? DBNull.Value;
                row["practiceAreaCapability"] = (object)caseAdditionalInfo.PracticeAreaCapability ?? DBNull.Value;
                row["pegCase"] = (object)caseAdditionalInfo.PegCase ?? DBNull.Value;
                row["caseManagerCode"] = (object)caseAdditionalInfo.CaseManagerCode ?? DBNull.Value;
                row["caseManagerName"] = (object)caseAdditionalInfo.CaseManagerName ?? DBNull.Value;
                row["managingOfficeCode"] = (object)caseAdditionalInfo.ManagingOfficeCode ?? DBNull.Value;
                row["managingOfficeAbbreviation"] = (object)caseAdditionalInfo.ManagingOfficeAbbreviation ?? DBNull.Value;
                row["managingOfficeName"] = (object)caseAdditionalInfo.ManagingOfficeName ?? DBNull.Value;
                row["billingOfficeCode"] = (object)caseAdditionalInfo.BillingOfficeCode ?? DBNull.Value;
                row["billingOfficeAbbreviation"] = (object)caseAdditionalInfo.BillingOfficeAbbreviation ?? DBNull.Value;
                row["billingOfficeName"] = (object)caseAdditionalInfo.BillingOfficeName ?? DBNull.Value;
                row["isPegCaseClass"] = (object)caseAdditionalInfo.IsPegCaseClass ?? DBNull.Value;
                row["pegIndustryTermCode"] = (object)caseAdditionalInfo.PegIndustryTermCode ?? DBNull.Value;
                row["pegIndustryTerm"] = (object)caseAdditionalInfo.PegIndustryTerm ?? DBNull.Value;
                row["pegIndustryAbbreviation"] = (object)caseAdditionalInfo.PegIndustryAbbreviation ?? DBNull.Value;
                caseAdditionalInfoDataTable.Rows.Add(row);
            }

            return caseAdditionalInfoDataTable;
        }

        #region Private Helpers
        private DataTable ConvertToCaseMasterDataTable(IList<CaseMaster> caseMaster)
        {
            var caseMasterDataTable = new DataTable();

            caseMasterDataTable.Columns.Add("clientCode", typeof(int));
            caseMasterDataTable.Columns.Add("caseCode", typeof(int));
            caseMasterDataTable.Columns.Add("caseName", typeof(string));
            caseMasterDataTable.Columns.Add("caseShortName", typeof(string));
            caseMasterDataTable.Columns.Add("statusCode", typeof(string));
            caseMasterDataTable.Columns.Add("startDate", typeof(DateTime));
            caseMasterDataTable.Columns.Add("projectedEndDate", typeof(DateTime));
            caseMasterDataTable.Columns.Add("endDate", typeof(DateTime));
            caseMasterDataTable.Columns.Add("caseTypeCode", typeof(int));
            caseMasterDataTable.Columns.Add("costCenterCode", typeof(string));
            caseMasterDataTable.Columns.Add("confidentialFlag", typeof(bool));
            caseMasterDataTable.Columns.Add("classCode", typeof(string));
            caseMasterDataTable.Columns.Add("billingCode", typeof(int));
            caseMasterDataTable.Columns.Add("dateAdded", typeof(DateTime));
            caseMasterDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            caseMasterDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            caseMasterDataTable.Columns.Add("replOffice", typeof(int));
            caseMasterDataTable.Columns.Add("replFlag", typeof(string));
            caseMasterDataTable.Columns.Add("pipelineId", typeof(Guid));
            caseMasterDataTable.Columns.Add("sysStartTime", typeof(DateTime));
            caseMasterDataTable.Columns.Add("sysEndTime", typeof(DateTime));

            if (caseMaster.Count > 0)
            {
                foreach (var record in caseMaster)
                {
                    var row = caseMasterDataTable.NewRow();

                    row["clientCode"] = record.ClientCode;
                    row["caseCode"] = record.CaseCode;
                    row["caseName"] = record.CaseName;
                    row["caseShortName"] = record.CaseShortName;
                    row["statusCode"] = record.StatusCode;
                    row["startDate"] = (object)record.StartDate ?? DBNull.Value;
                    row["projectedEndDate"] = (object)record.ProjectedEndDate ?? DBNull.Value;
                    row["endDate"] = (object)record.EndDate ?? DBNull.Value;
                    row["caseTypeCode"] = record.CaseTypeCode;
                    row["costCenterCode"] = record.CostCenterCode;
                    row["confidentialFlag"] = record.ConfidentialFlag;
                    row["classCode"] = record.ClassCode;
                    row["billingCode"] = (object)record.BillingCode ?? DBNull.Value;
                    row["dateAdded"] = record.DateAdded;
                    row["lastUpdated"] = record.LastUpdated;
                    row["lastUpdatedBy"] = record.LastUpdatedBy;
                    row["replOffice"] = record.ReplOffice;
                    row["replFlag"] = record.ReplFlag;
                    row["pipelineId"] = (object)record.PipelineId ?? DBNull.Value;
                    row["sysStartTime"] = record.SysStartTime;
                    row["sysEndTime"] = record.SysEndTime;

                    caseMasterDataTable.Rows.Add(row);
                }
            }
            return caseMasterDataTable;
        }

        private DataTable ConvertToCaseMasterHistoryDataTable(IList<CaseMasterHistory> caseMasterHistory)
        {
            var caseMasterHistoryDataTable = new DataTable();

            caseMasterHistoryDataTable.Columns.Add("clientCode", typeof(int));
            caseMasterHistoryDataTable.Columns.Add("caseCode", typeof(int));
            caseMasterHistoryDataTable.Columns.Add("caseName", typeof(string));
            caseMasterHistoryDataTable.Columns.Add("caseShortName", typeof(string));
            caseMasterHistoryDataTable.Columns.Add("statusCode", typeof(string));
            caseMasterHistoryDataTable.Columns.Add("startDate", typeof(DateTime));
            caseMasterHistoryDataTable.Columns.Add("projectedEndDate", typeof(DateTime));
            caseMasterHistoryDataTable.Columns.Add("endDate", typeof(DateTime));
            caseMasterHistoryDataTable.Columns.Add("caseTypeCode", typeof(int));
            caseMasterHistoryDataTable.Columns.Add("costCenterCode", typeof(string));
            caseMasterHistoryDataTable.Columns.Add("confidentialFlag", typeof(bool));
            caseMasterHistoryDataTable.Columns.Add("classCode", typeof(string));
            caseMasterHistoryDataTable.Columns.Add("billingCode", typeof(int));
            caseMasterHistoryDataTable.Columns.Add("dateAdded", typeof(DateTime));
            caseMasterHistoryDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            caseMasterHistoryDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            caseMasterHistoryDataTable.Columns.Add("replOffice", typeof(int));
            caseMasterHistoryDataTable.Columns.Add("replFlag", typeof(string));
            caseMasterHistoryDataTable.Columns.Add("pipelineId", typeof(Guid));
            caseMasterHistoryDataTable.Columns.Add("sysStartTime", typeof(DateTime));
            caseMasterHistoryDataTable.Columns.Add("sysEndTime", typeof(DateTime));

            if (caseMasterHistory.Count > 0)
            {
                foreach (var record in caseMasterHistory)
                {
                    var row = caseMasterHistoryDataTable.NewRow();

                    row["clientCode"] = record.ClientCode;
                    row["caseCode"] = record.CaseCode;
                    row["caseName"] = record.CaseName;
                    row["caseShortName"] = record.CaseShortName;
                    row["statusCode"] = record.StatusCode;
                    row["startDate"] = (object)record.StartDate ?? DBNull.Value;
                    row["projectedEndDate"] = (object)record.ProjectedEndDate ?? DBNull.Value;
                    row["endDate"] = (object)record.EndDate ?? DBNull.Value;
                    row["caseTypeCode"] = record.CaseTypeCode;
                    row["costCenterCode"] = record.CostCenterCode;
                    row["confidentialFlag"] = record.ConfidentialFlag;
                    row["classCode"] = record.ClassCode;
                    row["billingCode"] = (object)record.BillingCode ?? DBNull.Value;
                    row["dateAdded"] = record.DateAdded;
                    row["lastUpdated"] = record.LastUpdated;
                    row["lastUpdatedBy"] = record.LastUpdatedBy;
                    row["replOffice"] = record.ReplOffice;
                    row["replFlag"] = record.ReplFlag;
                    row["pipelineId"] = (object)record.PipelineId ?? DBNull.Value;
                    row["sysStartTime"] = record.SysStartTime;
                    row["sysEndTime"] = record.SysEndTime;

                    caseMasterHistoryDataTable.Rows.Add(row);
                }
            }
            return caseMasterHistoryDataTable;
        }

        private IEnumerable<ScheduleMaster> UpdateAllocationEndDateToCaseEndDate(IEnumerable<OpportunityViewModel> convertedOpportunities,
            IEnumerable<ScheduleMaster> allocationsOnOpportuntiesConvertedToCase,
            IEnumerable<CaseOpportunityMap> casesToWhichOpportunityConverted)
        {
            var allocations = new List<ScheduleMaster>();
            foreach (var opportunity in convertedOpportunities)
            {
                var allocationsOnOpportunity = allocationsOnOpportuntiesConvertedToCase.Where(p => p.PipelineId == opportunity.PipelineId);
                var OpportunityCaseMap = casesToWhichOpportunityConverted.FirstOrDefault(c => c.PipelineId == opportunity.PipelineId);
                foreach (var allocation in allocationsOnOpportunity)
                {
                    if (allocation.StartDate == opportunity.StartDate && allocation.EndDate == opportunity.EndDate)
                    {
                        allocation.EndDate = OpportunityCaseMap.EndDate > allocation.StartDate
                            ? OpportunityCaseMap.EndDate
                            : allocation.StartDate;
                    }
                    allocation.CaseTypeCode = OpportunityCaseMap.CaseTypeCode;
                    allocations.Add(allocation);
                }
            }
            return allocations;
        }

        private IEnumerable<ScheduleMasterPlaceholder> UpdatePlaceholderAllocationWithCaseData(IEnumerable<OpportunityViewModel> convertedOpportunities,
            IEnumerable<ScheduleMasterPlaceholder> placeholderAllocationsOnOpportuntiesConvertedToCase,
            IEnumerable<CaseOpportunityMap> casesToWhichOpportunityConverted)
        {
            var allocations = new List<ScheduleMasterPlaceholder>();
            foreach (var opportunity in convertedOpportunities)
            {
                var allocationsOnOpportunity = placeholderAllocationsOnOpportuntiesConvertedToCase.Where(p => p.PipelineId == opportunity.PipelineId);
                var OpportunityCaseMap = casesToWhichOpportunityConverted.FirstOrDefault(c => c.PipelineId == opportunity.PipelineId);
                foreach (var allocation in allocationsOnOpportunity)
                {
                    allocation.OldCaseCode = OpportunityCaseMap.OldCaseCode;
                    allocation.LastUpdatedBy = "Auto - CCM";

                    if (allocation.StartDate == opportunity.StartDate && allocation.EndDate == opportunity.EndDate)
                    {
                        allocation.EndDate = OpportunityCaseMap.EndDate > allocation.StartDate
                            ? OpportunityCaseMap.EndDate
                            : allocation.StartDate;
                    }
                    allocations.Add(allocation);
                }
            }
            return allocations;
        }


        private List<ScheduleMaster> CheckAndSplitAllocationForPrePostRevenue(IEnumerable<ScheduleMaster> allocations, IEnumerable<CaseOpportunityMap> oppConvertedToCases)
        {
            var newAllocations = new List<ScheduleMaster>();
            foreach (var newCaseData in oppConvertedToCases)
            {
                var allocationOnOpp = allocations.Where(ppc => ppc.PipelineId == newCaseData.PipelineId);

                foreach (var allocation in allocationOnOpp)
                {
                    allocation.CaseCode = newCaseData.CaseCode;
                    allocation.ClientCode = newCaseData.ClientCode;
                    allocation.OldCaseCode = newCaseData.OldCaseCode;

                    var allocStartDate = allocation.StartDate;
                    var allocEndDate = allocation.EndDate;

                    if (allocEndDate < newCaseData.StartDate)
                    {
                        newAllocations.Add(ConvertToPrePostAllocation(allocation, Convert.ToDateTime(allocStartDate), allocEndDate));
                    }
                    else if (allocStartDate <= newCaseData.EndDate && allocEndDate >= newCaseData.StartDate)
                    {
                        if (allocStartDate < newCaseData.StartDate && allocEndDate > newCaseData.EndDate)
                        {
                            newAllocations.Add(CreateNewAllocation(allocation, Convert.ToDateTime(allocStartDate), newCaseData.StartDate.AddDays(-1), 4));
                            newAllocations.Add(CreateNewAllocation(allocation, newCaseData.EndDate.AddDays(1), Convert.ToDateTime(allocEndDate), 4));
                            newAllocations.Add(ConvertToRegularAllocation(allocation, newCaseData.StartDate, newCaseData.EndDate));
                        }
                        else if (allocStartDate < newCaseData.StartDate && (allocEndDate >= newCaseData.StartDate && allocEndDate <= newCaseData.EndDate))
                        {
                            newAllocations.Add(CreateNewAllocation(allocation, Convert.ToDateTime(allocStartDate), newCaseData.StartDate.AddDays(-1), 4));
                            newAllocations.Add(ConvertToRegularAllocation(allocation, newCaseData.StartDate, allocEndDate));
                        }
                        else if (allocStartDate >= newCaseData.StartDate && allocEndDate <= newCaseData.EndDate)
                        {
                            newAllocations.Add(ConvertToRegularAllocation(allocation, allocStartDate, allocEndDate));
                        }
                        else if ((allocStartDate >= newCaseData.StartDate && allocStartDate <= newCaseData.EndDate) && allocEndDate > newCaseData.EndDate)
                        {
                            newAllocations.Add(CreateNewAllocation(allocation, newCaseData.EndDate.AddDays(1), Convert.ToDateTime(allocEndDate), 4));
                            newAllocations.Add(ConvertToRegularAllocation(allocation, allocStartDate, newCaseData.EndDate));
                        }

                    }
                    else if (allocStartDate > newCaseData.EndDate)
                    {
                        newAllocations.Add(ConvertToPrePostAllocation(allocation, Convert.ToDateTime(allocStartDate), allocEndDate));
                    }

                }
            }

            return newAllocations;
        }
        private ScheduleMaster ConvertToRegularAllocation(ScheduleMaster prePostStaffingAllocation, DateTime? startDate, DateTime? endDate)
        {
            prePostStaffingAllocation.StartDate = startDate;
            prePostStaffingAllocation.EndDate = endDate;
            prePostStaffingAllocation.LastUpdatedBy = "Auto-CCM";
            prePostStaffingAllocation.InvestmentCode = prePostStaffingAllocation.InvestmentCode == 4 ? null : prePostStaffingAllocation.InvestmentCode;

            return prePostStaffingAllocation;
        }

        private ScheduleMaster ConvertToPrePostAllocation(ScheduleMaster allocation, DateTime? startDate, DateTime? endDate)
        {
            allocation.StartDate = startDate;
            allocation.EndDate = endDate;
            allocation.LastUpdatedBy = "Auto-CCM";
            allocation.InvestmentCode = 4;

            return allocation;
        }

        private ScheduleMaster CreateNewAllocation(ScheduleMaster prePostStaffingAllocation, DateTime newStartDate, DateTime newEndDate, short? investmentCode)
        {
            return new ScheduleMaster
            {
                Allocation = prePostStaffingAllocation.Allocation,
                CaseCode = prePostStaffingAllocation.CaseCode,
                CaseRoleCode = prePostStaffingAllocation.CaseRoleCode,
                ClientCode = prePostStaffingAllocation.ClientCode,
                CurrentLevelGrade = prePostStaffingAllocation.CurrentLevelGrade,
                EmployeeCode = prePostStaffingAllocation.EmployeeCode,
                EndDate = newEndDate,
                InvestmentCode = investmentCode,
                OldCaseCode = prePostStaffingAllocation.OldCaseCode,
                OperatingOfficeCode = prePostStaffingAllocation.OperatingOfficeCode,
                PipelineId = prePostStaffingAllocation.PipelineId,
                ServiceLineCode = prePostStaffingAllocation.ServiceLineCode,
                ServiceLineName = prePostStaffingAllocation.ServiceLineName,
                StartDate = newStartDate,
                Notes = prePostStaffingAllocation.Notes,
                LastUpdatedBy = "Auto-CCM"
            };
        }

        private bool IsCcmCaseEndsAfterAllocationStarts(DateTime? allocationStartDate, DateTime ccmCaseEndDate)
        {
            return ccmCaseEndDate > allocationStartDate;
        }

        private bool isPrePostOverlapsWithCaseDuration(DateTime? allocationStartDate, DateTime? allocationEndDate, DateTime ccmCaseStartDate, DateTime ccmCaseEndDate)
        {
            return allocationStartDate <= ccmCaseEndDate && allocationEndDate >= ccmCaseStartDate;
        }

        private bool IsCcmCaseEndsBeforeAllocationStarts(DateTime? allocationStartDate, DateTime ccmCaseEndDate)
        {
            return ccmCaseEndDate < allocationStartDate;
        }

        private bool IsCcmCaseEndsOnOrAfterAllocationEnds(DateTime? allocationEndDate, DateTime ccmCaseEndDate)
        {
            return allocationEndDate <= ccmCaseEndDate;
        }

        private bool IsPrePostStartsAndEndsWithinCaseDuration(DateTime? allocationStartDate, DateTime? allocationEndDate, DateTime ccmCaseStartDate, DateTime ccmCaseEndDate)
        {
            return allocationStartDate >= ccmCaseStartDate && allocationEndDate <= ccmCaseEndDate;
        }

        private bool IsPrePostEndsBetweenCaseDuration(DateTime? allocationStartDate, DateTime? allocationEndDate, DateTime ccmCaseStartDate, DateTime ccmCaseEndDate)
        {
            return allocationStartDate < ccmCaseStartDate && (allocationEndDate >= ccmCaseStartDate && allocationEndDate <= ccmCaseEndDate);
        }
        private bool IsPrePostStartsBetweenCaseDuration(DateTime? allocationStartDate, DateTime? allocationEndDate, DateTime ccmCaseStartDate, DateTime ccmCaseEndDate)
        {
            return (allocationStartDate >= ccmCaseStartDate && allocationStartDate <= ccmCaseEndDate) && allocationEndDate > ccmCaseEndDate;
        }

        private bool IsCcmCaseEndBetweenAllocation(ScheduleMaster allocation, DateTime ccmCaseEndDate)
        {
            return allocation.EndDate > ccmCaseEndDate && allocation.StartDate <= ccmCaseEndDate;
        }

        private List<ScheduleMaster> SplitAllocationForPrePost(ScheduleMaster resourceAllocation, CaseRoll updatedCase)
        {
            var allocationsData = new List<ScheduleMaster>();

            var newAllocation = resourceAllocation.Clone();
            newAllocation.Id = null;
            newAllocation.StartDate = updatedCase.CurrentCaseEndDate.Value.AddDays(1).Date;
            newAllocation.EndDate = resourceAllocation.EndDate.Value;
            newAllocation.InvestmentCode = 4; //pre-post
            newAllocation.LastUpdatedBy = "Auto-CCM";

            //Update existing allocation till case end date
            resourceAllocation.EndDate = updatedCase.CurrentCaseEndDate.Value;
            resourceAllocation.LastUpdatedBy = "Auto-CCM";

            allocationsData.Add(resourceAllocation);
            allocationsData.Add(newAllocation);

            return allocationsData;
        }

        private List<ScheduleMaster> CreateResourceAllocationsToUpsert(IEnumerable<ScheduleMaster> allAllocationsByCases, IEnumerable<CaseRoll> updatedCaseRolls)
        {
            var allocationsToUpsert = new List<ScheduleMaster>();

            foreach (var updatedCase in updatedCaseRolls)
            {
                var allocationsByCase = allAllocationsByCases.Where(all => all.OldCaseCode == updatedCase.RolledFromOldCaseCode);

                if (!allocationsByCase.Any())
                    continue;

                foreach (var allocation in allocationsByCase)
                {
                    if (IsCcmCaseEndsAfterAllocationStarts(allocation.StartDate, updatedCase.CurrentCaseEndDate.Value))
                    {
                        // If CCM has extended the date, then extend the allocations
                        if (IsCcmCaseEndsOnOrAfterAllocationEnds(allocation.EndDate, updatedCase.CurrentCaseEndDate.Value))
                        {
                            allocation.EndDate = updatedCase.CurrentCaseEndDate;
                            allocation.LastUpdatedBy = "Auto-CCM";

                            allocationsToUpsert.Add(allocation);
                        }
                        //If CCM has reduced the date, then split the allocation
                        else if (IsCcmCaseEndBetweenAllocation(allocation, updatedCase.CurrentCaseEndDate.Value))
                        {
                            allocationsToUpsert.AddRange(SplitAllocationForPrePost(allocation, updatedCase));

                        }
                    }
                    //If allocation starts after case end date and it's a regular allocation, then convert it to pre-post
                    else if (IsCcmCaseEndsBeforeAllocationStarts(allocation.StartDate, updatedCase.CurrentCaseEndDate.Value) && allocation.InvestmentCode == null)
                    {
                        allocation.InvestmentCode = 4;
                        allocation.LastUpdatedBy = "Auto-CCM";

                        allocationsToUpsert.Add(allocation);
                    }

                }

            }

            return allocationsToUpsert;
        }

        private async Task DeleteOverlappedAllocations(IEnumerable<CaseRoll> rolledCases)
        {
            var oldCaseCodes = string.Join(",", rolledCases.Select(x => x.RolledFromOldCaseCode).Distinct());
            var allocations = await _staffingApiClient.GetResourceAllocationsBySelectedValues(oldCaseCodes, null, null, null, null);
            allocations = allocations.Where(x => x.InvestmentCode == null);
            var allocationsGroupedByEmployeeAndCase = allocations.GroupBy(x => new { x.EmployeeCode, x.OldCaseCode }).Select(g => g.OrderBy(o => o.StartDate).ThenByDescending(t => t.EndDate).ToList());
            var allocationIdToDelete = new List<Guid>();
            foreach (var groupedAllocation in allocationsGroupedByEmployeeAndCase)
            {
                for (var index = 0; index < groupedAllocation.Count(); index++)
                {
                    for (var innerindex = index + 1; innerindex < groupedAllocation.Count(); innerindex++)
                    {
                        if (groupedAllocation[innerindex].StartDate >= groupedAllocation[index].StartDate && groupedAllocation[innerindex].EndDate <= groupedAllocation[index].EndDate)
                        {
                            allocationIdToDelete.Add((Guid)groupedAllocation[innerindex].Id);
                        }
                    }
                }
            }

            if (allocationIdToDelete.Count > 0)
            {
                var allocationsIds = string.Join(",", allocationIdToDelete.Distinct());

                await _staffingApiClient.DeleteResourceAllocationByIds(allocationsIds, "CCM Polling");
            }
        }

        private static DataTable GetOpportunityCaseMapDto(IEnumerable<CaseOpportunityMap> caseOpportunityMapList)
        {
            var opportunityCaseMapDataTable = new DataTable();
            opportunityCaseMapDataTable.Columns.Add("pipelineId", typeof(Guid));
            opportunityCaseMapDataTable.Columns.Add("oldCaseCode", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("clientCode", typeof(int));
            opportunityCaseMapDataTable.Columns.Add("clientName", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("caseCode", typeof(int));
            opportunityCaseMapDataTable.Columns.Add("caseName", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("caseTypeCode", typeof(short));
            opportunityCaseMapDataTable.Columns.Add("caseTypeName", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("caseOfficeCode", typeof(short));
            opportunityCaseMapDataTable.Columns.Add("caseOfficeName", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("caseOfficeAbbreviation", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("billingOfficeCode", typeof(short));
            opportunityCaseMapDataTable.Columns.Add("billingOfficeName", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("billingOfficeAbbreviation", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("cortexOpportunityId", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("estimatedTeamSize", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("pricingTeamSize", typeof(string));
            opportunityCaseMapDataTable.Columns.Add("caseServedByRingfence", typeof(bool));


            foreach (var caseOpportunityMap in caseOpportunityMapList)
            {
                var row = opportunityCaseMapDataTable.NewRow();

                row["pipelineId"] = caseOpportunityMap.PipelineId;
                row["oldCaseCode"] = caseOpportunityMap.OldCaseCode;
                row["clientCode"] = caseOpportunityMap.ClientCode;
                row["clientName"] = caseOpportunityMap.ClientName;
                row["caseCode"] = caseOpportunityMap.CaseCode;
                row["caseName"] = caseOpportunityMap.CaseName;
                row["caseTypeCode"] = caseOpportunityMap.CaseTypeCode;
                row["caseTypeName"] = caseOpportunityMap.CaseTypeName;
                row["caseOfficeCode"] = caseOpportunityMap.CaseOfficeCode;
                row["caseOfficeName"] = caseOpportunityMap.CaseOfficeName;
                row["caseOfficeAbbreviation"] = caseOpportunityMap.CaseOfficeAbbreviation;
                row["billingOfficeCode"] = caseOpportunityMap.BillingOfficeCode;
                row["billingOfficeName"] = caseOpportunityMap.BillingOfficeName;
                row["billingOfficeAbbreviation"] = caseOpportunityMap.BillingOfficeAbbreviation;
                row["cortexOpportunityId"] = (object)caseOpportunityMap.CortexOpportunityId ?? null;
                row["estimatedTeamSize"] = (object)caseOpportunityMap.EstimatedTeamSize ?? null;
                row["pricingTeamSize"] = (object)caseOpportunityMap.PricingTeamSize ?? null;
                row["caseServedByRingfence"] = (object)caseOpportunityMap.CaseServedByRingfence ?? DBNull.Value;

                opportunityCaseMapDataTable.Rows.Add(row);

            }
            return opportunityCaseMapDataTable;
        }

        private IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(IEnumerable<ScheduleMaster> resourceAllocations)
        {
            var allocations = resourceAllocations.Select(item => new ResourceAssignmentViewModel
            {
                Id = item.Id,
                OldCaseCode = item.OldCaseCode,
                PipelineId = item.PipelineId,
                EmployeeCode = item.EmployeeCode,
                CurrentLevelGrade = item.CurrentLevelGrade,
                OperatingOfficeCode = item.OperatingOfficeCode,
                Allocation = item.Allocation,
                StartDate = Convert.ToDateTime(item.StartDate),
                EndDate = Convert.ToDateTime(item.EndDate),
                ServiceLineCode = item.ServiceLineCode,
                ServiceLineName = item.ServiceLineName,
                InvestmentCode = item.InvestmentCode,
                CaseRoleCode = item.CaseRoleCode,
                CaseTypeCode = item.CaseTypeCode,
                LastUpdatedBy = item.LastUpdatedBy,
                Notes = item.Notes,
            });

            return allocations;
        }

        #endregion

    }
}