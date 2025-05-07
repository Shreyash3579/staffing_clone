using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Repository;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class PipelinePollingService: IPipelinePollingService
    {
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IPipelineApiClient _pipelineApiClient;
        private readonly IPipelinePollingRepository _pipelinePollingRepository;

        public PipelinePollingService(IStaffingApiClient staffingApiClient,
            IPipelineApiClient pipelineApiClient, IPipelinePollingRepository pipelinePollingRepository)
        {
            _staffingApiClient = staffingApiClient;
            _pipelineApiClient = pipelineApiClient;
            _pipelinePollingRepository = pipelinePollingRepository;
        }

        public async Task<IEnumerable<Guid?>> UpdateOpportunityEndDateFromPipeline()
        {
            var oppsInCasePlanningBoard = await _staffingApiClient.GetOpportunityDataInCasePlanningBoard();

            if (oppsInCasePlanningBoard.Count() > 0)
            {
                var listPipelineIds = string.Join(",", oppsInCasePlanningBoard.Select(ppc => ppc.PipelineId).Distinct());
                var oppsFromPipeline = await _pipelineApiClient.GetOpportunitiesByPipelineIds(listPipelineIds);

                foreach(var opp in oppsInCasePlanningBoard)
                {
                    opp.ProjectEndDate = oppsFromPipeline.FirstOrDefault(x => x.PipelineId == opp.PipelineId)?.EndDate;
                    opp.LastUpdatedBy = "Auto-Pipeline-EndDate-Change";
                }

                var oppsWithUpdatedEndDate = await _staffingApiClient.UpsertCasePlanningBoardData(oppsInCasePlanningBoard);

                var updatedPipelineIds = oppsInCasePlanningBoard.Select(ppc => ppc.PipelineId).Distinct();

                return updatedPipelineIds;
            }

            return Enumerable.Empty<Guid?>();
        }

        public async Task UpsertOpportunitiesFlatDataFromPipeline(bool isFullLoad, DateTime? lastUpdated)
        {
            //DateTime? lastPolledDateTime = lastUpdated.HasValue
            //    ? lastUpdated.Value
            //    : await _pollMasterRepostory.GetLastPolledTimeStampFromStaffingDB(Constants.CaseAdditionalInfo);

            DateTime? lastPolledDateTime = null;
            if (isFullLoad || lastPolledDateTime == DateTime.MinValue)
            {
                lastPolledDateTime = null;
            }

            var now = DateTime.UtcNow; //saving UTC date in Poll Master as we are comparing with sysstarttime to get incremental data which is store in UTC

            var opportunitiesInfo = await _pipelineApiClient.GetOpportunitiesFlatData(lastPolledDateTime);

            if (opportunitiesInfo.Any())
            {
                var opportunitiesInfoDataTable = ConvertToOpportunitiesInfoDataTableDataTable(opportunitiesInfo);

                //await _CCMPollingRepository.UpsertCaseAdditionalInfo(caseAdditionalInfoDataTable, isFullLoad);
                await _pipelinePollingRepository.UpsertOpportunitiesFlatData(opportunitiesInfoDataTable, isFullLoad);
                await _pipelinePollingRepository.UpsertOpportunitiesFlatDataInPipeline(opportunitiesInfoDataTable, isFullLoad);
            }

            //await _pollMasterRepostory.UpsertPollMasterOnStaffingDB(Constants.CaseAdditionalInfo, now);
        }

        #region Helper Methods
        private DataTable ConvertToOpportunitiesInfoDataTableDataTable(IEnumerable<OpportunityFlatViewModel> opportunitiesInfo)
        {
            var opportunityInfoDataTable = new DataTable();
            opportunityInfoDataTable.Columns.Add("pipelineId", typeof(Guid));
            opportunityInfoDataTable.Columns.Add("opportunityName", typeof(string));
            opportunityInfoDataTable.Columns.Add("clientCode", typeof(int));
            opportunityInfoDataTable.Columns.Add("clientName", typeof(string));
            opportunityInfoDataTable.Columns.Add("statusCode", typeof(short));
            opportunityInfoDataTable.Columns.Add("statusText", typeof(string));
            opportunityInfoDataTable.Columns.Add("startDate", typeof(DateTime));
            opportunityInfoDataTable.Columns.Add("endDate", typeof(DateTime));
            opportunityInfoDataTable.Columns.Add("probability", typeof(short));
            opportunityInfoDataTable.Columns.Add("coordinatingPartnerCode", typeof(string));
            opportunityInfoDataTable.Columns.Add("primaryPartnerCode", typeof(string));
            opportunityInfoDataTable.Columns.Add("managingOfficeCode", typeof(int));
            opportunityInfoDataTable.Columns.Add("managingOfficeAbbreviation", typeof(string));
            opportunityInfoDataTable.Columns.Add("managingOfficeName", typeof(string));
            opportunityInfoDataTable.Columns.Add("primaryIndustry", typeof(string));
            opportunityInfoDataTable.Columns.Add("primaryCapability", typeof(string));
            opportunityInfoDataTable.Columns.Add("practiceAreaIndustryCode", typeof(int));
            opportunityInfoDataTable.Columns.Add("practiceAreaIndustry", typeof(string));
            opportunityInfoDataTable.Columns.Add("practiceAreaCapabilityCode", typeof(int));
            opportunityInfoDataTable.Columns.Add("practiceAreaCapability", typeof(string));
            opportunityInfoDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            opportunityInfoDataTable.Columns.Add("lastUpdatedBy", typeof(string));



            foreach (var opportunityInfo in opportunitiesInfo)
            {
                var row = opportunityInfoDataTable.NewRow();

                row["pipelineId"] = opportunityInfo.PipelineId;
                row["opportunityName"] = opportunityInfo.OpportunityName;
                row["clientCode"] = opportunityInfo.ClientCode;
                row["clientName"] = opportunityInfo.ClientName;
                row["statusCode"] = opportunityInfo.OpportunityStatusCode;
                row["statusText"] = opportunityInfo.OpportunityStatusName;
                row["startDate"] = (object)opportunityInfo.StartDate ?? DBNull.Value;
                row["endDate"] = (object)opportunityInfo.EndDate ?? DBNull.Value;
                row["probability"] = (object)opportunityInfo.ProbabilityPercent ?? DBNull.Value;
                row["coordinatingPartnerCode"] = (object)opportunityInfo.CoordinatingPartnerCode ?? DBNull.Value;
                row["primaryPartnerCode"] = (object)opportunityInfo.PrimaryPartnerCode ?? DBNull.Value;
                row["managingOfficeCode"] = (object)opportunityInfo.ManagingOfficeCode ?? DBNull.Value;
                row["managingOfficeAbbreviation"] = (object)opportunityInfo.ManagingOfficeAbbreviation ?? DBNull.Value;
                row["managingOfficeName"] = (object)opportunityInfo.ManagingOfficeName ?? DBNull.Value;
                row["primaryIndustry"] = (object)opportunityInfo.PrimaryIndustry ?? DBNull.Value;
                row["primaryCapability"] = (object)opportunityInfo.PrimaryCapability ?? DBNull.Value;
                row["practiceAreaIndustryCode"] = (object)opportunityInfo.IndustryPracticeAreaCode ?? DBNull.Value;
                row["practiceAreaIndustry"] = (object)opportunityInfo.IndustryPracticeArea ?? DBNull.Value;
                row["practiceAreaCapabilityCode"] = (object)opportunityInfo.CapabilityPracticeAreaCode ?? DBNull.Value;
                row["practiceAreaCapability"] = (object)opportunityInfo.CapabilityPracticeArea ?? DBNull.Value;
                row["lastUpdated"] = (object)opportunityInfo.LastUpdated ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)opportunityInfo.LastUpdatedBy ?? DBNull.Value;
                opportunityInfoDataTable.Rows.Add(row);
            }

            return opportunityInfoDataTable;
        }

        #endregion

    }
}
