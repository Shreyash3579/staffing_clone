using Pipeline.API.Contracts.RepositoryInterfaces;
using Pipeline.API.Contracts.Services;
using Pipeline.API.Models;
using Pipeline.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipeline.API.Core.Services
{
    public class OpportunityService : IOpportunityService
    {
        private readonly IOpportunityRepository _opportunityRepository;
        private readonly ICortexRepository _cortexRepository;

        public OpportunityService(IOpportunityRepository opportunityRepository, ICortexRepository cortexRepository)
        {
            _opportunityRepository = opportunityRepository;
            _cortexRepository = cortexRepository;
        }

        public async Task<IEnumerable<OpportunityViewModel>> GetOpportunitiesByOfficesActiveInDateRange(DateTime startDate,
            DateTime endDate, string officeCodes, string opportunityStatusTypeCodes, string clientCodes)
        {
            if (startDate > endDate)
                throw new ArgumentException("endDate should be greater than or equal to startDate");
            if (string.IsNullOrEmpty(officeCodes))
                throw new ArgumentException("Office Codes can not be null");
            if (string.IsNullOrEmpty(opportunityStatusTypeCodes))
                throw new ArgumentException("Opportunity Status Types can not be null");

            var opportunitiesData = await
                _opportunityRepository.GetOpportunitiesByOfficesActiveInDateRange(startDate, endDate, officeCodes, opportunityStatusTypeCodes, clientCodes);

            var CortexOpportunityId = string.Join(',', opportunitiesData.Where(x => x.CortexOpportunityId != null).Select(x => x.CortexOpportunityId)?.Distinct());

            //var cortexIds = "0061l00000ZPhkNAAT,0061l00000ZNHRxAAP";
            var opportunitiesDataWithTeamSize = await _cortexRepository.GetTeamSizeFromCortex(CortexOpportunityId);

            var opportunityWithTeamSize = ConvertToOpportunityViewModel(opportunitiesData, opportunitiesDataWithTeamSize);

            return opportunityWithTeamSize;
        }

        public async Task<IEnumerable<OpportunityDetailsViewModel>> GetOpportunityDetailsByPipelineIds(string pipelineIds)
        {
            if (pipelineIds == null || pipelineIds == String.Empty)
                throw new ArgumentException("pipelineIds can not be null or empty");
            var opportunitiesData = await
                _opportunityRepository.GetOpportunityDetailsByPipelineIds(pipelineIds);

            var CortexOpportunityId = string.Join(',', opportunitiesData.Where(x => x.CortexOpportunityId != null).Select(x => x.CortexOpportunityId)?.Distinct());

            var opportunitiesDataWithTeamSize = await _cortexRepository.GetTeamSizeFromCortex(CortexOpportunityId);

            var opportunityWithTeamSize = ConvertToOpportunityDetailsViewModel(opportunitiesData, opportunitiesDataWithTeamSize);

            var opportunities = ConvertToOpportunityDetailsViewModel(opportunitiesData);

            return opportunityWithTeamSize;
        }

        public async Task<IEnumerable<OpportunityViewModel>> GetOpportunitiesForTypeahead(string searchString)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
            {
                return Enumerable.Empty<OpportunityViewModel>();
            }

            var opportunitiesData = await
                _opportunityRepository.GetOpportunitiesForTypeahead(searchString);

            var opportunities = ConvertToOpportunityViewModel(opportunitiesData);

            return opportunities;
        }

        public async Task<IEnumerable<OpportunityViewModel>> GetOpportunitiesWithTaxonomiesByPipelineIds(string pipelineIdList, 
            string officeCodes, string opportunityStatusTypeCodes)
        {
            if (string.IsNullOrEmpty(pipelineIdList))
                return Enumerable.Empty<OpportunityViewModel>();

            var opportunitiesData = await
                _opportunityRepository.GetOpportunitiesWithTaxonomiesByPipelineIds(pipelineIdList, officeCodes, opportunityStatusTypeCodes);

            var CortexOpportunityId = string.Join(',', opportunitiesData.Where(x => x.CortexOpportunityId != null).Select(x => x.CortexOpportunityId)?.Distinct());

            var opportunitiesDataWithTeamSize = await _cortexRepository.GetTeamSizeFromCortex(CortexOpportunityId);

            var opportunityWithTeamSize = ConvertToOpportunityViewModel(opportunitiesData, opportunitiesDataWithTeamSize);

            var opportunities = ConvertToOpportunityViewModel(opportunitiesData);

            //return opportunities;
            return opportunityWithTeamSize;
        }

        public async Task<IEnumerable<OpportunityViewModel>> GetOpportunityMasterChangesSinceLastPolled(DateTime? lastPolledDateTime)
        {
            if (lastPolledDateTime == DateTime.MinValue)
                lastPolledDateTime = null;
            var updatedOpps = await _opportunityRepository.GetOpportunityMasterChangesSinceLastPolled(lastPolledDateTime);
            var opportunities = ConvertToOpportunityViewModel(updatedOpps);

            return opportunities;
        }

        public async Task<IEnumerable<OpportunityFlatViewModel>> GetOpportunitiesFlatData(DateTime? lastUpdated)
        {
            if (lastUpdated == DateTime.MinValue)
                lastUpdated = null;

            var opportunitiesFlatData = await _opportunityRepository.GetOpportunitiesFlatData(lastUpdated);
            var opportunitiesFlatDataVM = ConvertToOpportunityViewModel(opportunitiesFlatData);

            return opportunitiesFlatDataVM;
        }


        public async Task<IEnumerable<Opportunity>> GetOppDataFromCortex(string CortexOpportunityId)
        {
            if (CortexOpportunityId == null || CortexOpportunityId == String.Empty)
                throw new ArgumentException("cortexIds can not be null or empty");

            var opportunitiesDataWithTeamSize = await _cortexRepository.GetTeamSizeFromCortex(CortexOpportunityId);

            return opportunitiesDataWithTeamSize;
        }

        private static DateTime? CalculateLikelyEndDate(string duration, DateTime? startDate)
        {
            if (string.IsNullOrEmpty(duration))
                return null;
            /*
             * Duration consists of months and weeks in the form of
             * 4.25 --> 4 months and 1 week 
             */
            var durationSplit = duration.Split('.');
            var months = durationSplit[0];
            var endDate = startDate ?? DateTime.Now;
            if (!string.IsNullOrEmpty(months))
            {
                try
                {
                    endDate = endDate.AddMonths(Convert.ToInt32(months));
                }
                catch(ArgumentOutOfRangeException ex)
                {
                    const short maxYearsForProjectEndDate = 20;
                    endDate = endDate.AddYears(maxYearsForProjectEndDate); //max 20 years in future in case of longer duration projects
                }
            }
                
            var weeksCode = durationSplit[1];
            int weeks;
            switch (weeksCode)
            {
                case "25":
                    weeks = 1;
                    break;
                case "50":
                    weeks = 2;
                    break;
                case "75":
                    weeks = 3;
                    break;
                default:
                    weeks = 0;
                    break;
            }

            endDate = endDate.AddDays(weeks * 7);
            return endDate;
        }

        private static IEnumerable<OpportunityViewModel> ConvertToOpportunityViewModel(IEnumerable<Opportunity> opportunitiesData, IEnumerable<Opportunity> opportunitiesDataWithTeamSize)
        { 
            var opportunities = (from opp in opportunitiesData 
                                 join opts in opportunitiesDataWithTeamSize
                                 on opp.CortexOpportunityId equals opts.CortexOpportunityId into oppwithTS
                                 from opportunity in oppwithTS.DefaultIfEmpty()
                                 select new OpportunityViewModel()
                                 {
                                     PipelineId = opp.PipelineId,
                                     CortexOpportunityId = opp.CortexOpportunityId,
                                     EstimatedTeamSize = opportunity?.EstimatedTeamSize,
                                     CoordinatingPartnerCode = opp.CoordinatingPartnerCode,
                                     BillingPartnerCode = opp.BillingPartnerCode,
                                     OtherPartnersCodes = opp.OtherPartnersCodes,
                                     OpportunityName = opp.OpportunityName,
                                     ClientName = opp.ClientName,
                                     ClientCode = opp.ClientCode,
                                     StartDate = opp.StartDate,
                                     EndDate = CalculateLikelyEndDate(opp.Duration, opp.StartDate),
                                     Duration = opp.Duration,
                                     ProbabilityPercent = opp.ProbabilityPercent,
                                     PrimaryCapability = opp.PrimaryCapability,
                                     PrimaryIndustry = opp.PrimaryIndustry,
                                     ManagingOfficeAbbreviation = opp.ManagingOfficeAbbreviation,
                                     ManagingOfficeCode = opp.ManagingOfficeCode,
                                     CaseAttributes = opp.CaseAttributes,
                                     ClientPriority = opp.ClientPriority,
                                     ClientPrioritySortOrder = opp.ClientPrioritySortOrder,
                                     IndustryPracticeAreaCode = opp.IndustryPracticeAreaCode,
                                     IndustryPracticeArea = opp.IndustryPracticeArea,
                                     CapabilityPracticeAreaCode = opp.CapabilityPracticeAreaCode,
                                     CapabilityPracticeArea = opp.CapabilityPracticeArea

                                 }).ToList();
            return opportunities;
            
        }

        private static IEnumerable<OpportunityDetailsViewModel> ConvertToOpportunityDetailsViewModel(IEnumerable<Opportunity> opportunitiesData, IEnumerable<Opportunity> opportunitiesDataWithTeamSize)
        {
            var opportunities = (from opp in opportunitiesData
                                 join opts in opportunitiesDataWithTeamSize
                                 on opp.CortexOpportunityId equals opts.CortexOpportunityId into oppwithTS
                                 from opportunity in oppwithTS.DefaultIfEmpty()
                                 select new OpportunityDetailsViewModel()
                                 {
                                     PipelineId = opp.PipelineId,
                                     CortexOpportunityId = opp.CortexOpportunityId,
                                     EstimatedTeamSize = opportunity?.EstimatedTeamSize,
                                     CoordinatingPartnerCode = opp.CoordinatingPartnerCode,
                                     BillingPartnerCode = opp.BillingPartnerCode,
                                     OtherPartnersCodes = opp.OtherPartnersCodes,
                                     OpportunityName = opp.OpportunityName,
                                     OpportunityStatus = opp.OpportunityStatus,
                                     ClientName = opp.ClientName,
                                     ClientCode = opp.ClientCode,
                                     ClientGroupCode = opp.ClientGroupCode,
                                     ClientGroupName = opp.ClientGroupName,
                                     PrimaryCapability = opp.PrimaryCapability,
                                     IndustryPracticeArea = opp.IndustryPracticeArea,
                                     PrimaryIndustry = opp.PrimaryIndustry,
                                     CapabilityPracticeArea = opp.CapabilityPracticeArea,
                                     StartDate = opp.StartDate,
                                     EndDate = CalculateLikelyEndDate(opp.Duration, opp.StartDate),
                                     Duration = opp.Duration,
                                     ProbabilityPercent = opp.ProbabilityPercent,
                                     CaseAttributes = opp.CaseAttributes
                                 }).ToList();
            return opportunities;

        }

        private static IEnumerable<OpportunityDetailsViewModel> ConvertToOpportunityDetailsViewModel(
            IEnumerable<Opportunity> opportunitiesData)
        {
            var opportunities = opportunitiesData.Select(item => new OpportunityDetailsViewModel
            {
                PipelineId = item.PipelineId,
                CortexOpportunityId = item.CortexOpportunityId,
                CoordinatingPartnerCode = item.CoordinatingPartnerCode,
                BillingPartnerCode = item.BillingPartnerCode,
                OtherPartnersCodes = item.OtherPartnersCodes,
                OpportunityName = item.OpportunityName,
                OpportunityStatus = item.OpportunityStatus,
                ClientName = item.ClientName,
                ClientCode = item.ClientCode,
                ClientGroupCode = item.ClientGroupCode,
                ClientGroupName = item.ClientGroupName,
                PrimaryCapability = item.PrimaryCapability,
                IndustryPracticeArea = item.IndustryPracticeArea,
                PrimaryIndustry = item.PrimaryIndustry,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                StartDate = item.StartDate,
                EndDate = CalculateLikelyEndDate(item.Duration, item.StartDate),
                Duration = item.Duration,
                ProbabilityPercent = item.ProbabilityPercent,
                CaseAttributes = item.CaseAttributes
            });

            return opportunities;
        }

        private static IEnumerable<OpportunityViewModel> ConvertToOpportunityViewModel(
            IEnumerable<Opportunity> opportunitiesData)
        {
            var opportunities = opportunitiesData.Select(item => new OpportunityViewModel
            {
                PipelineId = item.PipelineId,
                CoordinatingPartnerCode = item.CoordinatingPartnerCode,
                BillingPartnerCode = item.BillingPartnerCode,
                OtherPartnersCodes = item.OtherPartnersCodes,
                OpportunityName = item.OpportunityName,
                ClientName = item.ClientName,
                ClientCode = item.ClientCode,
                StartDate = item.StartDate,
                EndDate = CalculateLikelyEndDate(item.Duration, item.StartDate),
                Duration = item.Duration,
                ProbabilityPercent = item.ProbabilityPercent,
                PrimaryCapability = item.PrimaryCapability,
                PrimaryIndustry = item.PrimaryIndustry,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                CaseAttributes = item.CaseAttributes,
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea
            });

            return opportunities;
        }

        private static IEnumerable<OpportunityFlatViewModel> ConvertToOpportunityViewModel(
            IEnumerable<OpportunityFlatViewModel> opportunitiesData)
        {
            var opportunities = opportunitiesData.Select(item => new OpportunityFlatViewModel
            {
                PipelineId = item.PipelineId,
                CoordinatingPartnerCode = item.CoordinatingPartnerCode,
                PrimaryPartnerCode = item.PrimaryPartnerCode,
                OpportunityName = item.OpportunityName,
                OpportunityStatusCode = item.OpportunityStatusCode,
                OpportunityStatusName = item.OpportunityStatusName,
                ClientName = item.ClientName,
                ClientCode = item.ClientCode,
                StartDate = item.StartDate,
                EndDate = CalculateLikelyEndDate(item.Duration, item.StartDate),
                Duration = item.Duration,
                ProbabilityPercent = item.ProbabilityPercent,
                PrimaryCapability = item.PrimaryCapability,
                PrimaryIndustry = item.PrimaryIndustry,
                ManagingOfficeCode = item.ManagingOfficeCode,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeName  = item.ManagingOfficeName,
                CaseAttributes = item.CaseAttributes,
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                LastUpdated = item.LastUpdated,
                LastUpdatedBy = item.LastUpdatedBy
            });

            return opportunities;
        }

    }
}