using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Services
{
    public class CasePlanningMetricsService: ICasePlanningMetricsService
    {
        private readonly ICasePlanningMetricsRepository _casePlanningMetricsRepository;
        
        public CasePlanningMetricsService(ICasePlanningMetricsRepository casePlanningMetricsRepository)
        {
            _casePlanningMetricsRepository = casePlanningMetricsRepository;
        }

        public async Task<AvailabilityMetricsViewModel> GetAvailabilityMetricsByFilterValues(SupplyFilterCriteria supplyFilterCriteria)
        {
            if (string.IsNullOrEmpty(supplyFilterCriteria.OfficeCodes) || supplyFilterCriteria.StartDate == DateTime.MinValue || supplyFilterCriteria.EndDate == DateTime.MinValue || string.IsNullOrEmpty(supplyFilterCriteria.StaffingTags))
                throw new ArgumentException("OfficeCode,StartDate,EndDate and StaffingTags parameters are required.");
            else
            {
                var resourcesBecomingAvailable = await _casePlanningMetricsRepository.GetAvailabilityMetricsByFilterValues(supplyFilterCriteria);

                return resourcesBecomingAvailable;
            }
        }
        public async Task<AvailabilityMetricsViewModel> GetAvailabilityMetricsForPlaygroundById(string playgroundId)
        {
            if (string.IsNullOrEmpty(playgroundId))
                throw new ArgumentException("Playground ID cannot be null");

            var availabilityMetricsData = await _casePlanningMetricsRepository.GetAvailabilityMetricsForPlaygroundById(playgroundId);

            return availabilityMetricsData;
        }

        public async Task<AvailabilityMetricsViewModel> UpsertAndGetCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(Guid playgroundId, IEnumerable<CasePlanningBoardPlaygroundAllocation> playgroundAllocations, string lastUpdatedBy)
        {
            if (playgroundId.Equals(Guid.Empty) || playgroundAllocations == null || !playgroundAllocations.Any())
                throw new ArgumentException("Playground ID and Playground Allocations parameters are required.");

            var casePlanningBoardPlaygroundAllocationTableType = ConvertPlaygroundAllocationsToTableType(playgroundAllocations);
            var availabilityMetricsData = await _casePlanningMetricsRepository.UpsertAndGetCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(playgroundId, casePlanningBoardPlaygroundAllocationTableType, lastUpdatedBy);

            return availabilityMetricsData;
        }

        public async Task<Guid> DeleteCasePlanningBoardMetricsPlaygroundById(Guid playgroundId, string lastUpdatedBy)
        {
            if (playgroundId == Guid.Empty) 
                throw new ArgumentException("Id cannot be null or empty");
            if (string.IsNullOrEmpty(lastUpdatedBy))
                throw new ArgumentException("lastUpdatedBy cannot be null or empty");
            
            var availabilityMetricsData = await _casePlanningMetricsRepository.DeleteCasePlanningBoardMetricsPlaygroundById(playgroundId, lastUpdatedBy);

            return availabilityMetricsData;
        }

        public async Task<CasePlanningBoardPlaygroundFilters> GetCasePlanningBoardPlaygroundFiltersByPlaygroundId(string playgroundId)
        {
            if (string.IsNullOrEmpty(playgroundId))
                throw new ArgumentException("Id cannot be null or empty");

           return await _casePlanningMetricsRepository.GetCasePlanningBoardPlaygroundFiltersByPlaygroundId(playgroundId);
        }

        #region private methods
        private DataTable ConvertPlaygroundAllocationsToTableType(IEnumerable<CasePlanningBoardPlaygroundAllocation> playgroundAllocations)
        {
            var casePlanningBoardPlaygroundAllocationTable = new DataTable();
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("employeeCode", typeof(string));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("newStartDate", typeof(DateTime));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("newEndDate", typeof(DateTime));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("newAllocation", typeof(short));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("newInvestmentCode", typeof(short));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("previousStartDate", typeof(DateTime));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("previousEndDate", typeof(DateTime));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("previousAllocation", typeof(short));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("previousInvestmentCode", typeof(short));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("isOpportunity", typeof(bool));
            casePlanningBoardPlaygroundAllocationTable.Columns.Add("caseEndDate", typeof(DateTime));

            foreach (var allocations in playgroundAllocations)
            {
                var row = casePlanningBoardPlaygroundAllocationTable.NewRow();
                
                row["employeeCode"] = allocations.EmployeeCode;
                row["newStartDate"] = (object)allocations.NewStartDate ?? DBNull.Value;
                row["newEndDate"] = (object)allocations.NewEndDate ?? DBNull.Value;
                row["newAllocation"] = (object)allocations.NewAllocation ?? DBNull.Value;
                row["newInvestmentCode"] = (object)allocations.NewInvestmentCode ?? DBNull.Value;
                row["previousStartDate"] = (object)allocations.PreviousStartDate ?? DBNull.Value;
                row["previousEndDate"] = (object)allocations.PreviousEndDate ?? DBNull.Value;
                row["previousAllocation"] = (object)allocations.PreviousAllocation ?? DBNull.Value;
                row["previousInvestmentCode"] = (object)allocations.PreviousInvestmentCode ?? DBNull.Value;
                row["isOpportunity"] = allocations.IsOpportunity;
                row["caseEndDate"] = (object)allocations.CaseEndDate ?? DBNull.Value;

                casePlanningBoardPlaygroundAllocationTable.Rows.Add(row);
            }

            return casePlanningBoardPlaygroundAllocationTable;
        }

        #endregion
    }
}
