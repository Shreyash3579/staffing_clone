using Dapper;
using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Repository
{
    public class CasePlanningMetricsPlaygroundRepository : ICasePlanningMetricsPlaygroundRepository
    {
        private readonly IBaseRepository<AvailabilityMetricsViewModel> _baseRepository;

        public CasePlanningMetricsPlaygroundRepository(IBaseRepository<AvailabilityMetricsViewModel> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<AvailabilityMetricsViewModel> CreateCasePlanningBoardMetricsPlayground(CasePlanningBoardPlaygroundFilters playgroundParameters, string createdBy)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
              StoredProcedureMap.CreateCasePlanningBoardMetricsPlayground,
              new
              {
                  createdBy,
                  playgroundParameters.SupplyStartDate,
                  playgroundParameters.DemandStartDate,
                  playgroundParameters.EndDate,
                  playgroundParameters.SupplyViewOfficeCodes,
                  playgroundParameters.SupplyViewStaffingTags,
                  playgroundParameters.LevelGrades,
                  playgroundParameters.PositionCodes,
                  playgroundParameters.PracticeAreaCodes,
                  playgroundParameters.AffiliationRoleCodes,
                  playgroundParameters.DemandViewOfficeCodes,
                  playgroundParameters.CaseAttributeNames,
                  playgroundParameters.CaseTypeCodes,
                  playgroundParameters.DemandTypes,
                  playgroundParameters.OpportunityStatusTypeCodes,
                  playgroundParameters.MinOpportunityProbability,
                  playgroundParameters.IndustryPracticeAreaCodes,
                  playgroundParameters.CapabilityPracticeAreaCodes,
                  playgroundParameters.IsCountOfIndividualResourcesToggle,
                  playgroundParameters.EnableMemberGrouping,
                  playgroundParameters.EnableNewlyAvailableHighlighting,
                  playgroundParameters.LastUpdatedBy
              },
              commandType: CommandType.StoredProcedure,
              commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var playgroundId = result.Read<Guid>().FirstOrDefault();
            var availabilityMetrics = result.Read<AvailabilityMetrics>().ToList();
            var availabilityMetrics_Nupur = result.Read<AvailabilityMetrics_Nupur>().ToList();

            var availabilityMetricsAndDetailedSupplyData = ConvertToAvailabilityMetricsViewModel(playgroundId, availabilityMetrics, availabilityMetrics_Nupur);

            return availabilityMetricsAndDetailedSupplyData;
        }

        #region private methods
        private AvailabilityMetricsViewModel ConvertToAvailabilityMetricsViewModel(Guid? playgroundId, List<AvailabilityMetrics> availabilityMetrics, List<AvailabilityMetrics_Nupur> availabilityMetrics_Nupur)
        {
            var availabilityMetricsViewModel = new AvailabilityMetricsViewModel
            {
                PlaygroundId = playgroundId,
                AvailabilityMetrics = availabilityMetrics,
                AvailabilityMetrics_Nupur = availabilityMetrics_Nupur
            };

            return availabilityMetricsViewModel;
        }
        #endregion
    }
}
