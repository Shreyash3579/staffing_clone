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
    public class CasePlanningMetricsRepository : ICasePlanningMetricsRepository
    {
        private readonly IBaseRepository<AvailabilityMetrics> _baseRepository;

        public CasePlanningMetricsRepository(IBaseRepository<AvailabilityMetrics> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<AvailabilityMetricsViewModel> GetAvailabilityMetricsByFilterValues(SupplyFilterCriteria supplyFilterCriteria)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
              StoredProcedureMap.GetAvailabilityMetricsByFilterValues,
              new
              {
                  supplyFilterCriteria.OfficeCodes,
                  supplyFilterCriteria.StartDate,
                  supplyFilterCriteria.EndDate,
                  supplyFilterCriteria.LevelGrades,
                  supplyFilterCriteria.PositionCodes,
                  supplyFilterCriteria.StaffingTags,
                  supplyFilterCriteria.PracticeAreaCodes,
                  supplyFilterCriteria.AffiliationRoleCodes
              },
              commandType: CommandType.StoredProcedure,
              commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var availabilityMetrics = result.Read<AvailabilityMetrics>().ToList();
            var availabilityMetrics_Nupur = result.Read<AvailabilityMetrics_Nupur>().ToList();

            var availabilityMetricsAndDetailedSupplyData = ConvertToAvailabilityMetricsViewModel(availabilityMetrics, availabilityMetrics_Nupur);
            return availabilityMetricsAndDetailedSupplyData;
        }

        public async Task<AvailabilityMetricsViewModel> GetAvailabilityMetricsForPlaygroundById(string playgroundId)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
              StoredProcedureMap.GetAvailabilityMetricsForPlaygroundById,
              new
              {
                  playgroundId
              },
              commandType: CommandType.StoredProcedure,
              commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var availabilityMetrics = result.Read<AvailabilityMetrics>().ToList();
            var availabilityMetrics_Nupur = result.Read<AvailabilityMetrics_Nupur>().ToList();

            var availabilityMetricsAndDetailedSupplyData = ConvertToAvailabilityMetricsViewModel(new Guid(playgroundId), availabilityMetrics, availabilityMetrics_Nupur);
            return availabilityMetricsAndDetailedSupplyData;
        }

        public async Task<AvailabilityMetricsViewModel> UpsertAndGetCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(Guid playgroundId, DataTable playgroundAllocations, string lastUpdatedBy)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
              StoredProcedureMap.UpsertCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations,
              new
              {
                  playgroundId,
                  allocationsData =
                        playgroundAllocations.AsTableValuedParameter(
                            "[dbo].[casePlanningBoardPlaygroundAllocationTableType]"),
                  lastUpdatedBy
              },
              commandType: CommandType.StoredProcedure,
              commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var availabilityMetrics = result.Read<AvailabilityMetrics>().ToList();
            var availabilityMetrics_Nupur = result.Read<AvailabilityMetrics_Nupur>().ToList();

            var availabilityMetricsAndDetailedSupplyData = ConvertToAvailabilityMetricsViewModel(playgroundId, availabilityMetrics, availabilityMetrics_Nupur);
            return availabilityMetricsAndDetailedSupplyData;
        }

        public async Task<Guid> DeleteCasePlanningBoardMetricsPlaygroundById(Guid playgroundId, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { playgroundId, lastUpdatedBy }, StoredProcedureMap.DeleteCasePlanningBoardMetricsPlaygroundById);

            return playgroundId;
        }

        public async Task<CasePlanningBoardPlaygroundFilters> GetCasePlanningBoardPlaygroundFiltersByPlaygroundId(string playgroundId)
        {
            var playgroundFilters = await
                _baseRepository.Context.Connection.QueryAsync<CasePlanningBoardPlaygroundFilters>(
                    StoredProcedureMap.GetCasePlanningBoardPlaygroundFiltersByPlaygroundId,
                    new { playgroundId = playgroundId },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return playgroundFilters.FirstOrDefault();
        }


        #region private methods
        private AvailabilityMetricsViewModel ConvertToAvailabilityMetricsViewModel(List<AvailabilityMetrics> availabilityMetrics, List<AvailabilityMetrics_Nupur> availabilityMetrics_Nupur)
        {
            var availabilityMetricsViewModel = new AvailabilityMetricsViewModel
            {
                AvailabilityMetrics = availabilityMetrics,
                AvailabilityMetrics_Nupur = availabilityMetrics_Nupur
            };

            return availabilityMetricsViewModel;
        }

        private AvailabilityMetricsViewModel ConvertToAvailabilityMetricsViewModel(Guid playgroundId, List<AvailabilityMetrics> availabilityMetrics, List<AvailabilityMetrics_Nupur> availabilityMetrics_Nupur)
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
