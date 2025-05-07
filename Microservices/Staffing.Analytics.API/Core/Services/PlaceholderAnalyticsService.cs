using Hangfire;
using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Core.Repository;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Services
{
    public class PlaceholderAnalyticsService : IPlaceholderAnalyticsService
    {
        private readonly IPlaceholderAllocationRepository _placeholderAllocationRepository;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IPipelineApiClient _pipelineApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IAnalyticsService _analyticsService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public PlaceholderAnalyticsService(IPlaceholderAllocationRepository placeholderAllocationRepository, IStaffingApiClient staffingApiClient,
            ICCMApiClient ccmApiClient, IPipelineApiClient pipelineApiClient, IResourceApiClient resourceApiClient, IAnalyticsService analyticsService, IBackgroundJobClient backgroundJobClient)
        {
            _placeholderAllocationRepository = placeholderAllocationRepository;
            _staffingApiClient = staffingApiClient;
            _pipelineApiClient = pipelineApiClient;
            _ccmApiClient = ccmApiClient;
            _resourceApiClient = resourceApiClient;
            _analyticsService = analyticsService;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<IEnumerable<string>> CorrectPlaceholderAnalyticsData()
        {
            var schedueleIdList = await _placeholderAllocationRepository.GetPlaceholderScheduleIdsIncorrectlyProcessedInAnalytics();
            var totalRecordsToUpsert = schedueleIdList.ScheduleIdsToUpsert.Count();
            var batchSize = 50;
            var skipRecords = 0;
            var correctedScheduleIds = new List<string>();
            while (skipRecords < totalRecordsToUpsert)
            {
                var scheduleIdsToCorrect = schedueleIdList.ScheduleIdsToUpsert.OrderBy(x => x).Skip(skipRecords).Take(batchSize);
                correctedScheduleIds.AddRange(await CreatePlaceholderAnalyticsReport(string.Join(",", scheduleIdsToCorrect)));
                skipRecords += batchSize;
            }

            var totalRecordsToDelete = schedueleIdList.ScheduleIdsToDelete.Count();
            skipRecords = 0;
            while (skipRecords < totalRecordsToDelete)
            {
                var scheduleIdsToCorrect = schedueleIdList.ScheduleIdsToDelete.OrderBy(x => x).Skip(skipRecords).Take(batchSize);
                await DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(string.Join(",", scheduleIdsToCorrect));
                skipRecords += batchSize;
            }
            return correctedScheduleIds;
        }

        public async Task<IEnumerable<string>> CreatePlaceholderAnalyticsReport(string scheduleMasterPlaceholderIds)
        {
            var placeholderScheduleIdList = scheduleMasterPlaceholderIds.Split(",");
            var totalRecords = placeholderScheduleIdList.Length;
            var batchSize = 50;
            var skipRecords = 0;

            var processedIds = new List<string>();

            while (skipRecords < totalRecords)
            {
                
                var selectedScheduleIdList = placeholderScheduleIdList.OrderBy(x => x).Skip(skipRecords).Take(batchSize);

                var selectedPlaceholderScheduleIds = string.Join(",", selectedScheduleIdList);

                var placeholderAllocationsWithDetails = await GetPlaceholderAllocationWithDetails(selectedPlaceholderScheduleIds);

                if (!placeholderAllocationsWithDetails.Any())
                {
                    return processedIds;
                }

                var namedPlaceholderAllocationsWithDetails = placeholderAllocationsWithDetails.Where(x => !string.IsNullOrEmpty(x.EmployeeCode));
                var guessedPlaceholderAllocationsWithDetails = placeholderAllocationsWithDetails.Where(x => string.IsNullOrEmpty(x.EmployeeCode));

                if (guessedPlaceholderAllocationsWithDetails.Any())
                {
                    var resourcePlaceholderAllocationsForTableauDataTable = CreatePlaceholderAssignmentDataTable(guessedPlaceholderAllocationsWithDetails);
                    var previousAllocationRangeBeforeUpdateForProspectiveResources = await _placeholderAllocationRepository.UpsertPlaceholderAnalyticsReportData(resourcePlaceholderAllocationsForTableauDataTable);
                    processedIds.AddRange(guessedPlaceholderAllocationsWithDetails.Select(x => x.Id.ToString()).Distinct());
                }
                
                if(namedPlaceholderAllocationsWithDetails.Any())
                {
                    var placeholderAllocations = new List<ResourceAllocation>();

                    var employeeCodes = string.Join(",", namedPlaceholderAllocationsWithDetails.Select(r => r.EmployeeCode).Distinct());

                    await _analyticsService.SplitAllocationsForHistoricalTransactionOrLevelGradeChange(namedPlaceholderAllocationsWithDetails, placeholderAllocations, employeeCodes);

                    namedPlaceholderAllocationsWithDetails = placeholderAllocations.Any() ? placeholderAllocations : namedPlaceholderAllocationsWithDetails;

                    placeholderAllocations = new List<ResourceAllocation>(); //This is the object that will be updated with all the data during split for cost and workday etc

                    await _analyticsService.SplitAllocationsForPendingTransactionOrLoA(namedPlaceholderAllocationsWithDetails, placeholderAllocations, employeeCodes);

                    placeholderAllocations = await _analyticsService.UpdateCommitmentsInAllocations(placeholderAllocations.ToList());

                    var placeholderAllocationsWithBillRateForTableau = await _analyticsService.GetResourcesAllocationsWithBillRate(placeholderAllocations);

                    placeholderAllocationsWithBillRateForTableau = await VerifyProcessedPlaceholderAllocationExistsInDB(placeholderAllocationsWithBillRateForTableau);

                    processedIds.AddRange(placeholderAllocations.Select(x => x.Id.ToString()).Distinct().ToList());
                    var resourcePlaceholderAllocationsForTableauDataTable = CreatePlaceholderAssignmentDataTable(placeholderAllocationsWithBillRateForTableau);

                    //upsert new placeholder data. SP returns data before upsert ONLY if data before updated allocation was propspective for availability recalculation as availability is impacted for prospective placheolders
                    var previousAllocationRangeBeforeUpdateForProspectiveResources = await _placeholderAllocationRepository.UpsertPlaceholderAnalyticsReportData(resourcePlaceholderAllocationsForTableauDataTable);

                    /*
                     * NOTE: Trigger Hangfire jobs to update Resource Availability only if data before update was for prospective placeholder or if new data to upsert is of prospective or both. 
                     * this is done to avoid unneccasry updates to RA table when no propective data is involved as it does not impact RA
                    */
                    if (previousAllocationRangeBeforeUpdateForProspectiveResources.Any() || namedPlaceholderAllocationsWithDetails.Any(x => x.IncludeInCapacityReporting.HasValue && (bool)x.IncludeInCapacityReporting))
                    {
                        var dateRangeToUpsertAvailabilityFor = _analyticsService.GetAvailabilityDateRangeToUpsertFromPreviousAndNewAllocationDates(previousAllocationRangeBeforeUpdateForProspectiveResources, placeholderAllocationsWithBillRateForTableau);

                        // Trigger Hangfire jobs to update dependent Tables like Resource Availability
                        var parentJobId = _backgroundJobClient.Enqueue<IAnalyticsService>(x =>
                            x.UpsertAvailabilityDataBetweenDateRange(dateRangeToUpsertAvailabilityFor));

                        _backgroundJobClient.ContinueJobWith<IAnalyticsService>(parentJobId,
                            x => x.UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));
                    }
                }
                

                
                skipRecords += batchSize;
            }

            return processedIds;

        }

        public async Task DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(string scheduleMasterPlaceholderIds)
        {
            if (string.IsNullOrEmpty(scheduleMasterPlaceholderIds))
            {
                throw new ArgumentException("scheduleMasterPlaceholderIds cannot be null or empty");
            }
                
            var listDeletedEmployees = await _placeholderAllocationRepository.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(scheduleMasterPlaceholderIds);

            if(listDeletedEmployees.Any())
            {
                var employeeCodes = string.Join(",", listDeletedEmployees);

                _backgroundJobClient.Enqueue<IAnalyticsService>(x =>
                   x.UpdateCostForResourcesAvailableInFullCapacity(employeeCodes));
            }
            
            return;
        }

        #region Private Members
        private async Task<IEnumerable<ResourceAllocation>> GetPlaceholderAllocationWithDetails(string placeholderScheduleIds)
        {
            if (string.IsNullOrEmpty(placeholderScheduleIds))
            {
                return Enumerable.Empty<ResourceAllocation>();
            }

            var distinctPlaceholderScheduleIds = placeholderScheduleIds.Split(",").Select(r => r.ToString().ToLower()).Distinct();

            var placeholderAllocations = await _staffingApiClient.GetPlaceholderAllocationsByScheduleIds(string.Join(",", distinctPlaceholderScheduleIds));

            var casesTask = Task.FromResult<IEnumerable<CaseViewModel>>(new List<CaseViewModel>());
            var opportunitiesTask = Task.FromResult<IEnumerable<OpportunityDetailsViewModel>>(new List<OpportunityDetailsViewModel>());
            var planningCardsTask = Task.FromResult<IEnumerable<PlanningCard>>(new List<PlanningCard>());
            var employeesTask = Task.FromResult<IEnumerable<Resource>>(new List<Resource>());
            var caseRoleTypesTask = Task.FromResult<IEnumerable<CaseRoleType>>(new List<CaseRoleType>());
            var investmentCategoriesTask = Task.FromResult<IEnumerable<InvestmentCategory>>(new List<InvestmentCategory>());
            var serviceLinesTask = Task.FromResult<IEnumerable<ServiceLine>>(new List<ServiceLine>());
            var ccmOfficesTask = Task.FromResult<IEnumerable<Office>>(new List<Office>());
            var jobProfilesTask = Task.FromResult<IEnumerable<Models.Workday.JobProfile>>(new List<Models.Workday.JobProfile>());

            if (placeholderAllocations.Any())
            {

                var employeeCodes = string.Join(",", placeholderAllocations.Select(r => r.EmployeeCode).Distinct());
                var oldCaseCodes = string.Join(",", placeholderAllocations.Select(r => r.OldCaseCode).Distinct());
                var pipelineIds = string.Join(",", placeholderAllocations.Where(r => string.IsNullOrEmpty(r.OldCaseCode) && r.PipelineId.HasValue).Select(r => r.PipelineId).Distinct());
                var planningCardIds = string.Join(",", placeholderAllocations.Where(r => string.IsNullOrEmpty(r.OldCaseCode) && !r.PipelineId.HasValue && r.PlanningCardId.HasValue).Select(r => r.PlanningCardId).Distinct());
                casesTask = _ccmApiClient.GetCaseDetailsByCaseCodes(oldCaseCodes);
                opportunitiesTask = _pipelineApiClient.GetOpportunityDetailsByPipelineIds(pipelineIds);
                planningCardsTask = _staffingApiClient.GetPlanningCardByPlanningCardIds(planningCardIds);
                employeesTask = _resourceApiClient.GetEmployees();
                caseRoleTypesTask = _staffingApiClient.GetCaseRoleTypeLookupList();
                investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryLookupList();
                serviceLinesTask = _resourceApiClient.GetServiceLineList();
                ccmOfficesTask = _ccmApiClient.GetOfficeList();
                jobProfilesTask = _resourceApiClient.GetJobProfileList();
            }

            await Task.WhenAll(casesTask, opportunitiesTask, employeesTask, caseRoleTypesTask,
                investmentCategoriesTask, serviceLinesTask, ccmOfficesTask, planningCardsTask);

            if (!placeholderAllocations.Any())
            {
                return Enumerable.Empty<ResourceAllocation>();
            }

            var cases = casesTask.Result ?? Enumerable.Empty<CaseViewModel>();
            var opportunities = opportunitiesTask.Result ?? Enumerable.Empty<OpportunityDetailsViewModel>();
            var planningCards = planningCardsTask.Result ?? Enumerable.Empty<PlanningCard>();
            var employees = employeesTask.Result ?? Enumerable.Empty<Resource>();
            var caseRoleTypes = caseRoleTypesTask.Result ?? Enumerable.Empty<CaseRoleType>();
            var investmentCategories = investmentCategoriesTask.Result ?? Enumerable.Empty<InvestmentCategory>();
            var serviceLines = serviceLinesTask.Result ?? Enumerable.Empty<ServiceLine>();
            var offices = ccmOfficesTask.Result ?? Enumerable.Empty<Models.Office>();
            var jobProfiles = jobProfilesTask.Result ?? Enumerable.Empty<Models.Workday.JobProfile>();

            var placeholderAllocationsMappedToCasesAndOpportunitiesAndPlanningCard = (from allocation in placeholderAllocations
                                                                    join c in cases on allocation.OldCaseCode equals c.OldCaseCode into allocCaseGroups
                                                                    from caseItem in allocCaseGroups.DefaultIfEmpty()
                                                                    join opp in opportunities on allocation.PipelineId equals opp.PipelineId into allocOppGroups
                                                                    from opportunityItem in allocOppGroups.DefaultIfEmpty()
                                                                    join pc in planningCards on allocation.PlanningCardId equals pc.Id into allocPcGroups
                                                                    from planningCarditem in allocPcGroups.DefaultIfEmpty()
                                                                    join emp in employees on allocation.EmployeeCode equals emp.EmployeeCode into allocEmployeeGroups
                                                                    from resource in allocEmployeeGroups.DefaultIfEmpty()
                                                                    select new ResourceAllocation
                                                                    {
                                                                        // Common to case and opp
                                                                        ClientCode = caseItem?.ClientCode ?? opportunityItem?.ClientCode,
                                                                        ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                                                        ClientGroupCode = caseItem?.ClientGroupCode ?? opportunityItem?.ClientGroupCode,
                                                                        ClientGroupName = caseItem?.ClientGroupName ?? opportunityItem?.ClientGroupName,
                                                                        // Opp related info
                                                                        PipelineId = opportunityItem?.PipelineId,
                                                                        OpportunityName = opportunityItem?.OpportunityName,
                                                                        // Case related info
                                                                        OldCaseCode = caseItem?.OldCaseCode,
                                                                        CaseCode = caseItem?.CaseCode,
                                                                        CaseName = caseItem?.CaseName,
                                                                        CaseTypeCode = caseItem?.CaseTypeCode,
                                                                        CaseTypeName = caseItem?.CaseType,
                                                                        ManagingOfficeCode = caseItem?.ManagingOfficeCode,
                                                                        ManagingOfficeAbbreviation = caseItem?.ManagingOfficeAbbreviation,
                                                                        ManagingOfficeName = caseItem?.ManagingOfficeName,
                                                                        BillingOfficeCode = caseItem?.BillingOfficeCode,
                                                                        BillingOfficeAbbreviation = caseItem?.BillingOfficeAbbreviation,
                                                                        BillingOfficeName = caseItem?.BillingOfficeName,
                                                                        PegCase = caseItem?.CaseServedByRingfence,
                                                                        PrimaryIndustryTermCode = caseItem?.PrimaryIndustryTermCode,
                                                                        PrimaryIndustryTagId = caseItem?.PrimaryIndustryTagId,
                                                                        PrimaryIndustry = caseItem?.PrimaryIndustry,
                                                                        PracticeAreaIndustryCode = caseItem?.IndustryPracticeAreaCode,
                                                                        PracticeAreaIndustry = caseItem?.IndustryPracticeArea,
                                                                        PrimaryCapabilityTermCode = caseItem?.PrimaryCapabilityTermCode,
                                                                        PrimaryCapabilityTagId = caseItem?.PrimaryCapabilityTagId,
                                                                        PrimaryCapability = caseItem?.PrimaryCapability,
                                                                        PracticeAreaCapabilityCode = caseItem?.CapabilityPracticeAreaCode,
                                                                        PracticeAreaCapability = caseItem?.CapabilityPracticeArea,
                                                                        // Employee related info
                                                                        EmployeeCode = allocation.EmployeeCode,
                                                                        EmployeeName = resource?.FullName,
                                                                        EmployeeStatusCode = Constants.EmployeeStatus.Active,
                                                                        Fte = resource?.Fte,
                                                                        PositionCode = resource?.Position?.PositionCode,
                                                                        PositionName = resource?.Position?.PositionName,
                                                                        PositionGroupName = !string.IsNullOrEmpty(allocation.EmployeeCode) ? resource?.Position?.PositionGroupName : jobProfiles.FirstOrDefault(x => x.PositionGroupCode == allocation.PositionGroupCode)?.PositionGroupName,
                                                                        PositionGroupCode = allocation.PositionGroupCode,
                                                                        TerminationDate = resource?.TerminationDate,
                                                                        InternetAddress = resource?.InternetAddress,
                                                                        CurrentLevelGrade = allocation.CurrentLevelGrade,
                                                                        OperatingOfficeCode = (int) allocation.OperatingOfficeCode,
                                                                        OperatingOfficeAbbreviation = offices.FirstOrDefault(o => o.OfficeCode == allocation.OperatingOfficeCode)?.OfficeAbbreviation,
                                                                        OperatingOfficeName = offices.FirstOrDefault(o => o.OfficeCode == allocation.OperatingOfficeCode)?.OfficeName,
                                                                        ServiceLineCode = allocation.ServiceLineCode,
                                                                        ServiceLineName = serviceLines.FirstOrDefault(s => s.ServiceLineCode == allocation.ServiceLineCode)?.ServiceLineName,
                                                                        // Staffing related info
                                                                        Id = (Guid)allocation.Id,
                                                                        Allocation = allocation.Allocation != null ? (int)allocation.Allocation : 0,
                                                                        StartDate = (DateTime)allocation.StartDate,
                                                                        EndDate = (DateTime)allocation.EndDate,
                                                                        InvestmentCode = allocation.InvestmentCode,
                                                                        InvestmentName = investmentCategories.FirstOrDefault(ic => ic.InvestmentCode == allocation.InvestmentCode)?.InvestmentName,
                                                                        CaseRoleCode = allocation.CaseRoleCode,
                                                                        CaseRoleName = caseRoleTypes.FirstOrDefault(crt => crt.CaseRoleCode == allocation.CaseRoleCode)?.CaseRoleName,
                                                                        Notes = allocation.Notes,
                                                                        LastUpdatedBy = allocation.LastUpdatedBy,
                                                                        PlanningCardId = planningCarditem?.Id,
                                                                        PlanningCardName = planningCarditem?.Name,
                                                                        IsPlaceholderAllocation = allocation.IsPlaceholderAllocation,
                                                                        IncludeInCapacityReporting = planningCarditem?.IncludeInCapacityReporting

                                                                    }).ToList();

         

            return placeholderAllocationsMappedToCasesAndOpportunitiesAndPlanningCard;

        }
        #endregion

        private static DataTable CreatePlaceholderAssignmentDataTable(IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var resourceAllocationsDataTable = new DataTable();
            // Common to case and opp
            resourceAllocationsDataTable.Columns.Add("clientCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("clientName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("clientGroupCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("clientGroupName", typeof(string));
            // Opp related info
            resourceAllocationsDataTable.Columns.Add("pipelineId", typeof(Guid));
            resourceAllocationsDataTable.Columns.Add("opportunityName", typeof(string));
            // Case related info
            resourceAllocationsDataTable.Columns.Add("oldCaseCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("caseName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseTypeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("caseTypeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("managingOfficeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("managingOfficeAbbreviation", typeof(string));
            resourceAllocationsDataTable.Columns.Add("managingOfficeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billingOfficeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("billingOfficeAbbreviation", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billingOfficeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("pegCase", typeof(bool));
            resourceAllocationsDataTable.Columns.Add("primaryIndustryTermCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("primaryIndustryTagId", typeof(string));
            resourceAllocationsDataTable.Columns.Add("primaryIndustry", typeof(string));
            resourceAllocationsDataTable.Columns.Add("practiceAreaIndustryCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("practiceAreaIndustry", typeof(string));
            resourceAllocationsDataTable.Columns.Add("primaryCapabilityTermCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("primaryCapabilityTagId", typeof(string));
            resourceAllocationsDataTable.Columns.Add("primaryCapability", typeof(string));
            resourceAllocationsDataTable.Columns.Add("practiceAreaCapabilityCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("practiceAreaCapability", typeof(string));
            // Employee related info
            resourceAllocationsDataTable.Columns.Add("employeeCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("employeeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("employeeStatusCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("fte", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("serviceLineCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("serviceLineName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionGroupName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionGroupCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("currentLevelGrade", typeof(string));
            resourceAllocationsDataTable.Columns.Add("operatingOfficeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            resourceAllocationsDataTable.Columns.Add("operatingOfficeName", typeof(string));
            // Staffing related info
            resourceAllocationsDataTable.Columns.Add("id", typeof(Guid));
            resourceAllocationsDataTable.Columns.Add("allocation", typeof(short));
            resourceAllocationsDataTable.Columns.Add("startDate", typeof(DateTime));
            resourceAllocationsDataTable.Columns.Add("endDate", typeof(DateTime));
            resourceAllocationsDataTable.Columns.Add("investmentCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("investmentName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseRoleCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseRoleName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("notes", typeof(string));
            resourceAllocationsDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            // Finance related info
            resourceAllocationsDataTable.Columns.Add("billRate", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("billCode", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("billRateType", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billRateCurrency", typeof(string));
            resourceAllocationsDataTable.Columns.Add("actualCost", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("actualCostInUSD", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("costUSDEffectiveYear", typeof(int));
            resourceAllocationsDataTable.Columns.Add("usdRate", typeof(decimal));
            // Commitment related info
            resourceAllocationsDataTable.Columns.Add("priorityCommitmentTypeCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("priorityCommitmentTypeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("commitmentTypeCodes", typeof(string));
            resourceAllocationsDataTable.Columns.Add("ringfence", typeof(string));
            resourceAllocationsDataTable.Columns.Add("isStaffingTag", typeof(bool));
            resourceAllocationsDataTable.Columns.Add("isOverriddenInSource", typeof(bool));
            // Placeholder related info
            resourceAllocationsDataTable.Columns.Add("planningCardId", typeof(string));
            resourceAllocationsDataTable.Columns.Add("isPlaceholderAllocation", typeof(bool));
            resourceAllocationsDataTable.Columns.Add("planningCardName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("includeInCapacityReporting", typeof(bool));
           
        

            foreach (var allocations in resourceAllocations)
            {
                var row = resourceAllocationsDataTable.NewRow();
                // Common to case and opp
                row["clientCode"] = (object)allocations.ClientCode ?? DBNull.Value;
                row["clientName"] = (object)allocations.ClientName ?? DBNull.Value;
                row["clientGroupCode"] = (object)allocations.ClientGroupCode ?? DBNull.Value;
                row["clientGroupName"] = (object)allocations.ClientGroupName ?? DBNull.Value;
                // Opp related info
                row["pipelineId"] = (object)allocations.PipelineId ?? DBNull.Value;
                row["opportunityName"] = (object)allocations.OpportunityName ?? DBNull.Value;
                // Case related info
                row["oldCaseCode"] = (object)allocations.OldCaseCode ?? DBNull.Value;
                row["caseCode"] = (object)allocations.CaseCode ?? DBNull.Value;
                row["caseName"] = (object)allocations.CaseName ?? DBNull.Value;
                row["caseTypeCode"] = (object)allocations.CaseTypeCode ?? DBNull.Value;
                row["caseTypeName"] = (object)allocations.CaseTypeName ?? DBNull.Value;
                row["managingOfficeCode"] = (object)allocations.ManagingOfficeCode ?? DBNull.Value;
                row["managingOfficeAbbreviation"] = (object)allocations.ManagingOfficeAbbreviation ?? DBNull.Value;
                row["managingOfficeName"] = (object)allocations.ManagingOfficeName ?? DBNull.Value;
                row["billingOfficeCode"] = (object)allocations.BillingOfficeCode ?? DBNull.Value;
                row["billingOfficeAbbreviation"] = (object)allocations.BillingOfficeAbbreviation ?? DBNull.Value;
                row["billingOfficeName"] = (object)allocations.BillingOfficeName ?? DBNull.Value;
                row["pegCase"] = (object)allocations.PegCase ?? DBNull.Value;
                row["primaryIndustryTermCode"] = (object)allocations.PrimaryIndustryTermCode ?? DBNull.Value;
                row["primaryIndustryTagId"] = (object)allocations.PrimaryIndustryTagId ?? DBNull.Value;
                row["primaryIndustry"] = (object)allocations.PrimaryIndustry ?? DBNull.Value;
                row["practiceAreaIndustryCode"] = (object)allocations.PracticeAreaIndustryCode ?? DBNull.Value;
                row["practiceAreaIndustry"] = (object)allocations.PracticeAreaIndustry ?? DBNull.Value;
                row["primaryCapabilityTermCode"] = (object)allocations.PrimaryCapabilityTermCode ?? DBNull.Value;
                row["primaryCapabilityTagId"] = (object)allocations.PrimaryCapabilityTagId ?? DBNull.Value;
                row["primaryCapability"] = (object)allocations.PrimaryCapability ?? DBNull.Value;
                row["practiceAreaCapabilityCode"] = (object)allocations.PracticeAreaCapabilityCode ?? DBNull.Value;
                row["practiceAreaCapability"] = (object)allocations.PracticeAreaCapability ?? DBNull.Value;
                // Employee related info
                row["employeeCode"] = allocations.EmployeeCode;
                row["employeeName"] = (object)allocations.EmployeeName ?? DBNull.Value;
                row["employeeStatusCode"] = (object)((int)allocations.EmployeeStatusCode) ?? DBNull.Value;
                row["fte"] = (object)allocations.Fte ?? DBNull.Value;
                row["serviceLineCode"] = (object)allocations.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)allocations.ServiceLineName ?? DBNull.Value;
                row["positionCode"] = (object)allocations.PositionCode ?? DBNull.Value;
                row["positionName"] = (object)allocations.PositionName ?? DBNull.Value;
                row["positionGroupName"] = (object)allocations.PositionGroupName ?? DBNull.Value;
                row["positionGroupCode"] = (object)allocations.PositionGroupCode ?? DBNull.Value;
                row["currentLevelGrade"] = allocations.CurrentLevelGrade;
                row["operatingOfficeCode"] = allocations.OperatingOfficeCode;
                row["operatingOfficeAbbreviation"] = (object)allocations.OperatingOfficeAbbreviation ?? DBNull.Value;
                row["operatingOfficeName"] = (object)allocations.OperatingOfficeName ?? DBNull.Value;
                // Staffing Related info
                row["id"] = (object)allocations.Id ?? DBNull.Value;
                row["allocation"] = allocations.Allocation;
                row["startDate"] = allocations.StartDate;
                row["endDate"] = allocations.EndDate;
                row["investmentCode"] = (object)allocations.InvestmentCode ?? DBNull.Value;
                row["investmentName"] = (object)allocations.InvestmentName ?? DBNull.Value;
                row["caseRoleCode"] = (object)allocations.CaseRoleCode ?? DBNull.Value;
                row["caseRoleName"] = (object)allocations.CaseRoleName ?? DBNull.Value;
                row["notes"] = allocations.Notes;
                row["lastUpdatedBy"] = allocations.LastUpdatedBy;

                // Finance related info
                row["billRate"] = (object)allocations.BillRate ?? DBNull.Value;
                row["billCode"] = (object)allocations.BillCode ?? DBNull.Value;
                row["billRateType"] = (object)allocations.BillRateType ?? DBNull.Value;
                row["billRateCurrency"] = (object)allocations.BillRateCurrency ?? DBNull.Value;
                row["actualCost"] = (object)allocations.ActualCost ?? DBNull.Value;
                row["actualCostInUSD"] = (object)allocations.CostInUSD ?? DBNull.Value;
                row["costUSDEffectiveYear"] = (object)allocations.CostUSDEffectiveYear ?? DBNull.Value;
                row["usdRate"] = (object)allocations.UsdRate ?? DBNull.Value;

                // Commitment related info
                row["priorityCommitmentTypeCode"] = (object)allocations.PriorityCommitmentTypeCode ?? DBNull.Value;
                row["priorityCommitmentTypeName"] = (object)allocations.PriorityCommitmentTypeName ?? DBNull.Value;
                row["commitmentTypeCodes"] = (object)allocations.CommitmentTypeCodes ?? DBNull.Value;
                row["ringfence"] = (object)allocations.Ringfence ?? DBNull.Value;
                row["isStaffingTag"] = (object)allocations.isStaffingTag ?? DBNull.Value;
                row["isOverriddenInSource"] = (object)allocations.isOverriddenInSource ?? DBNull.Value;
                resourceAllocationsDataTable.Rows.Add(row);
                // Placeholder related info
                row["planningCardId"] = (object)allocations.PlanningCardId ?? DBNull.Value;
                row["isPlaceholderAllocation"] = (object)allocations.IsPlaceholderAllocation ?? DBNull.Value;
                row["planningCardName"] = (object)allocations.PlanningCardName ?? DBNull.Value;
                row["includeInCapacityReporting"] = (object)allocations.IncludeInCapacityReporting ?? DBNull.Value; ;
        
            }

            return resourceAllocationsDataTable;
        }

        private async Task<IEnumerable<ResourceAllocation>> VerifyProcessedPlaceholderAllocationExistsInDB(IEnumerable<ResourceAllocation> placeholderResourceAllocationsWitBillRate)
        {
            var distinctScheduleIds = placeholderResourceAllocationsWitBillRate.Select(r => r.Id.ToString().ToLower()).Distinct();

            var placeholderResourceAllocations = await _staffingApiClient.GetPlaceholderAllocationsByScheduleIds(string.Join(",", distinctScheduleIds));

            var scheduleIdsExistsInDB = placeholderResourceAllocations.Select(r => r.Id.ToString().ToLower()).Distinct();

            placeholderResourceAllocationsWitBillRate = placeholderResourceAllocationsWitBillRate.Where(x => scheduleIdsExistsInDB.ToList().Contains(x.Id.ToString().ToLower()));
            return placeholderResourceAllocationsWitBillRate;
        }

    }
}
