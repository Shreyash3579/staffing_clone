using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IPreponedCasesAllocationsAuditRepository
    {
        Task<IEnumerable<PreponedCasesAllocationsAudit>> UpsertPreponedCaseAllocationsAudit(DataTable preponedCasesAllocationsAuditDataTable);
        Task<IEnumerable<PreponedCasesAllocationsAudit>> GetPreponedCaseAllocationsAudit(string serviceLineCodes, string officeCodes,
            DateTime? startDate = null, DateTime? endDate = null);
    }
}
