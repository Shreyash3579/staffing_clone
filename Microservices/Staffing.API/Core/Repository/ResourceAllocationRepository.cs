using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class ResourceAllocationRepository : IResourceAllocationRepository
    {
        private readonly IBaseRepository<ResourceAllocation> _baseRepository;

        public ResourceAllocationRepository(IBaseRepository<ResourceAllocation> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<ResourceAllocation>> UpsertResourceAllocations(DataTable resourceAllocationDataTable)
        {
            var allocations = await _baseRepository.Context.Connection.QueryAsync<ResourceAllocation>(
                StoredProcedureMap.UpsertResourceAllocations,
                new
                {
                    resourceAllocations =
                        resourceAllocationDataTable.AsTableValuedParameter(
                            "[dbo].[ResourceAllocationTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return allocations;
        }

        public async Task DeleteResourceAllocationById(Guid id, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { id, lastUpdatedBy }, StoredProcedureMap.DeleteResourceAllocationById);
        }

        public async Task DeleteResourceAllocationByIds(string ids, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { ids, lastUpdatedBy }, StoredProcedureMap.DeleteResourceAllocationByIds);
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByCaseCodes(string oldCaseCodes)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { oldCaseCodes },
                    StoredProcedureMap.GetResourceAllocationsByCaseCodes);

            return allocations;
        }

        public async Task<IEnumerable<ResourcesCount>> GetResourceAllocationsCountByCaseCodes(string oldCaseCodes)
        {
            var allocationsCount = await Task.Run(() => _baseRepository.Context.Connection.Query<ResourcesCount>(
                StoredProcedureMap.GetResourceAllocationsCountByCaseCodes,
                new { oldCaseCodes},
               commandType: CommandType.StoredProcedure,
               commandTimeout: 180).ToList());

            return allocationsCount;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsBySelectedValues(string oldCaseCodes, string employeeCodes, DateTime? lastUpdated, DateTime? startDate,
            DateTime? endDate, string caseRoleCodes)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes },
                    StoredProcedureMap.GetResourceAllocationsBySelectedValues);
            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsBySelectedValuesV2(string oldCaseCodes, string employeeCodes, DateTime? lastUpdated, DateTime? startDate,
            DateTime? endDate, string caseRoleCodes, string action)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes, action },
                    StoredProcedureMap.GetResourceAllocationsBySelectedValues_v2);
            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsBySelectedValues(string action, string oldCaseCodes, string employeeCodes, DateTime? lastUpdated, DateTime? startDate,
            DateTime? endDate, string caseRoleCodes)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { action, oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes },
                    StoredProcedureMap.GetResourceAllocationsBySelectedValues_v3);
            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByPipelineIds(string pipelineIds)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { pipelineIds },
                    StoredProcedureMap.GetResourceAllocationsByPipelineIds);

            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByScheduleIds(string scheduleIds)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { scheduleIds },
                    StoredProcedureMap.GetResourceAllocationsByScheduleIds);

            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByOfficeCodes(string officeCodes,
            DateTime startDate, DateTime endDate)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new
                {
                    officeCodes,
                    startDate,
                    endDate
                },
                    StoredProcedureMap.GetResourceAllocationsByOfficeCodes);

            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByEmployeeCodes(string employeeCodes,
            DateTime? startDate, DateTime? endDate)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new
                {
                    employeeCodes,
                    startDate,
                    endDate
                },
                    StoredProcedureMap.GetResourceAllocationsByEmployeeCodes);

            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetRecordsUpdatedOnCaseRoll(string oldCaseCode, DateTime caseCurrentEndDate)
        {
            var result = await
                _baseRepository.GetAllAsync(new { oldCaseCode, caseCurrentEndDate },
                    StoredProcedureMap.GetRecordsUpdatedOnCaseRoll);

            return result;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsOnCaseRollByCaseCodes(string oldCaseCodes)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { oldCaseCodes },
                    StoredProcedureMap.GetResourceAllocationsOnCaseRollByCaseCodes);

            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetAllocationsForEmployeesOnPrePost()
        {
            var allocations = await
                _baseRepository.GetAllAsync(StoredProcedureMap.GetAllocationsForEmployeesOnPrePost);

            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsBySelectedSupplyValues(string officeCodes, DateTime startDate, DateTime endDate,
            string staffingTags, string currentLevelGrades)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { officeCodes, startDate, endDate, staffingTags, currentLevelGrades },
                    StoredProcedureMap.GetResourceAllocationsBySelectedSupplyValues);
            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocation>> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date)
        {
            var allocations = await
                _baseRepository.GetAllAsync(new { employeeCode, date },
                    StoredProcedureMap.GetLastTeamByEmployeeCode);
            return allocations;
        }

        public async Task<IEnumerable<EmployeeLastBillableDateViewModel>> GetLastBillableDateByEmployeeCodes(string employeeCodes, DateTime? date)
        {
            var result = await Task.Run(() => _baseRepository.Context.Connection.Query<EmployeeLastBillableDateViewModel>(
                StoredProcedureMap.GetLastBillableDateByEmployeeCodes,
                new { employeeCodes, date },
               commandType: CommandType.StoredProcedure,
               commandTimeout: 180).ToList());

            return result;
        }
    }
}