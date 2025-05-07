using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IPreponedCasesAllocationsAuditService
    {
        Task<IEnumerable<PreponedCasesAllocationsAuditViewModel>> GetPreponedCaseAllocationsAudit(string serviceLineCodes, string officeCodes, DateTime? startDate = null, DateTime? endDate= null);
    }
}
