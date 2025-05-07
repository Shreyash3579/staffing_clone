using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IAggregationService
    {
        Task<ResourceCommitmentViewModel> GetAllCommitmentsForEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<EmployeeInfoWithGxcAffiliationsViewModel> GetEmployeeInfoWithGxcAffiliations(string employeeCode);
    }
}
