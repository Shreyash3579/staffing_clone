using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IPreponedCasesAllocationsAuditService
    {
        Task<IEnumerable<PreponedCasesAllocationsAudit>> UpsertPreponedCaseAllocationsAudit(IEnumerable<PreponedCasesAllocationsAudit> preponedCasesAllocationsAudit);
        Task<IEnumerable<PreponedCasesAllocationsAudit>> GetPreponedCaseAllocationsAudit(string serviceLineCodes, string officeCodes, DateTime? startDate = null, DateTime? endDate= null);

    }
}
