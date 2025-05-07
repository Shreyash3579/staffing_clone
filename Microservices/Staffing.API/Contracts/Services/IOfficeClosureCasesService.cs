using Staffing.API.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IOfficeClosureCasesService
    {
        Task<OfficeClosureCases> UpsertOfficeClosureCases(OfficeClosureCases officeClosureCases);
        Task<OfficeClosureCases> GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate);
    }
}
