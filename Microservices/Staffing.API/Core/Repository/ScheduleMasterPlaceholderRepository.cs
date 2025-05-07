using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class ScheduleMasterPlaceholderRepository : IScheduleMasterPlaceholderRepository
    {
        private readonly IBaseRepository<ScheduleMasterPlaceholder> _baseRepository;

        public ScheduleMasterPlaceholderRepository(IBaseRepository<ScheduleMasterPlaceholder> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPlaceholderScheduleIds(string placeholderScheduleIds)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { placeholderScheduleIds },
                    StoredProcedureMap.GetPlaceholderAllocationsByPlaceholderScheduleIds);

            return allocations;
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByCaseCodes(string oldCaseCodes)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { oldCaseCodes },
                    StoredProcedureMap.GetPlaceholderAllocationsByCaseCodes);

            return allocations;
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPipelineIds(string pipelineIds)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { pipelineIds },
                    StoredProcedureMap.GetPlaceholderAllocationsByPipelineIds);

            return allocations;
        }

        public async Task DeletePlaceholderAllocationsByIds(string placeholderIds, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { placeholderIds, lastUpdatedBy }, StoredProcedureMap.DeletePlaceholderAllocationsByIds);
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> UpsertPlaceholderAllocation(DataTable placeholderAllocationsDatatable)
        {
            var allocations = await _baseRepository.Context.Connection.QueryAsync<ScheduleMasterPlaceholder>(
                StoredProcedureMap.UpsertPlaceholderAllocations,
                new
                {
                    placeholderAllocations =
                        placeholderAllocationsDatatable.AsTableValuedParameter(
                            "[dbo].[placeholderAllocationTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return allocations;
        }

        public async Task<IList<ScheduleMasterPlaceholder>> GetPlaceholderPlanningCardAllocationsWithinDateRange(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            var allocations = await Task.Run(() => _baseRepository.Context.Connection.Query<ScheduleMasterPlaceholder>(
                    StoredProcedureMap.GetPlanningCardsPlaceholdersAllocationsWithinDateRange,
                    new { employeeCodes, startDate, endDate },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 180).ToList());

            return allocations;
        }


        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByEmployeeCodes(string employeeCodes,
            DateTime? startDate, DateTime? endDate)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new
                {
                    employeeCodes,
                    startDate,
                    endDate
                },
                    StoredProcedureMap.GetPlaceholderAllocationsByEmployeeCodes);

            return allocations;
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPlanningCardIds(string planningCardIds)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { planningCardIds },
                    StoredProcedureMap.GetPlaceholderAllocationsByPlanningCardIds);

            return allocations;
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetAllocationsByPlanningCardIds(string planningCardIds)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { planningCardIds },
                    StoredProcedureMap.GetAllocationsByPlanningCardIds);

            return allocations;
        }
    }
}
