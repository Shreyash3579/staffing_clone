using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class ResourceHistoryService : IResourceHistoryService
    {
        private readonly IResourceHistoryRepository _resourceAllocationRepository;
        public ResourceHistoryService(IResourceHistoryRepository resourceAllocationRepository)
        {
            _resourceAllocationRepository = resourceAllocationRepository;
        }
        public async Task<IEnumerable<ResourceAllocationViewModel>> GetHistoricalStaffingAllocationsForEmployee(string employeeCode)
        {
            var resourceHistory = await _resourceAllocationRepository.GetHistoricalStaffingAllocationsForEmployee(employeeCode);
            return ConvertToResourceAllocationViewModel(resourceHistory);
        }

        private static IEnumerable<ResourceAllocationViewModel> ConvertToResourceAllocationViewModel(
            IEnumerable<ResourceAllocation> allocations)
        {
            var viewModel = allocations.Select(item => new ResourceAllocationViewModel
            {
                Id = item.Id,
                OldCaseCode = item.OldCaseCode,
                PipelineId = item.PipelineId,
                EmployeeCode = item.EmployeeCode,
                CurrentLevelGrade = item.CurrentLevelGrade,
                OperatingOfficeCode = item.OperatingOfficeCode,
                Allocation = item.Allocation,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                InvestmentCode = item.InvestmentCode,
                CaseRoleCode = item.CaseRoleCode,
                CaseRoleName = item.CaseRoleName,
                LastUpdatedBy = item.LastUpdatedBy,
                Notes = item.Notes
            });

            return viewModel;
        }
    }
}
