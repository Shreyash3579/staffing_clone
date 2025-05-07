using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Services
{
    public class CasePlanningMetricsPlaygroundService : ICasePlanningMetricsPlaygroundService
    {
        private readonly ICasePlanningMetricsPlaygroundRepository _caseplanningMetricsPlaygroundRepository;

        public CasePlanningMetricsPlaygroundService(ICasePlanningMetricsPlaygroundRepository casePlanningWhiteboardRepository)
        {
            _caseplanningMetricsPlaygroundRepository = casePlanningWhiteboardRepository;
        }

        public async Task<AvailabilityMetricsViewModel> CreateCasePlanningBoardMetricsPlayground(DemandFilterCriteria demandFilterCriteria, SupplyFilterCriteria supplyFilterCriteria,
            bool isCountOfIndividualResourcesToggle, bool enableMemberGrouping, bool enableNewlyAvailableHighlighting, string lastUpdatedBy)
        {
            var playgroundParameters = ConvertFiltersToCasePlanningBoardPlaygroundFilters(demandFilterCriteria, supplyFilterCriteria, isCountOfIndividualResourcesToggle,
                enableMemberGrouping, enableNewlyAvailableHighlighting, lastUpdatedBy);

            var playgroundData = await _caseplanningMetricsPlaygroundRepository.CreateCasePlanningBoardMetricsPlayground(playgroundParameters, lastUpdatedBy);
            
            return playgroundData;
        }

        #region Private Methods
        private CasePlanningBoardPlaygroundFilters ConvertFiltersToCasePlanningBoardPlaygroundFilters(DemandFilterCriteria demandFilterCriteria, SupplyFilterCriteria supplyFilterCriteria,
            bool isCountOfIndividualResourcesToggle, bool enableMemberGrouping, bool enableNewlyAvailableHighlighting, string lastUpdatedBy)
        {
            var playgroundParams = new CasePlanningBoardPlaygroundFilters()
            {
                AffiliationRoleCodes = supplyFilterCriteria.AffiliationRoleCodes,
                PracticeAreaCodes = supplyFilterCriteria.PracticeAreaCodes,
                LevelGrades = supplyFilterCriteria.LevelGrades,
                PositionCodes = supplyFilterCriteria.PositionCodes,
                SupplyViewOfficeCodes = supplyFilterCriteria.OfficeCodes,
                SupplyViewStaffingTags = supplyFilterCriteria.StaffingTags,
                SupplyStartDate = supplyFilterCriteria.StartDate,
                CapabilityPracticeAreaCodes = demandFilterCriteria.CapabilityPracticeAreaCodes,
                IndustryPracticeAreaCodes = demandFilterCriteria.IndustryPracticeAreaCodes,
                CaseAttributeNames = demandFilterCriteria.CaseAttributeNames,
                CaseTypeCodes = demandFilterCriteria.CaseTypeCodes,
                DemandTypes = demandFilterCriteria.DemandTypes,
                DemandViewOfficeCodes = demandFilterCriteria.OfficeCodes,
                DemandStartDate = demandFilterCriteria.StartDate,
                EndDate = demandFilterCriteria.EndDate,
                MinOpportunityProbability = demandFilterCriteria.MinOpportunityProbability,
                OpportunityStatusTypeCodes = demandFilterCriteria.OpportunityStatusTypeCodes,
                IsCountOfIndividualResourcesToggle = isCountOfIndividualResourcesToggle,
                EnableMemberGrouping = enableMemberGrouping,
                EnableNewlyAvailableHighlighting = enableNewlyAvailableHighlighting,
                LastUpdatedBy = lastUpdatedBy
            };
            return playgroundParams;
        }
        #endregion
    }
}
