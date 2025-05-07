using Staffing.API.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IOfficeClosureCasesRepository
    {
        Task<OfficeClosureCases> UpsertOfficeClosureCases(OfficeClosureCases officeClosureCases);
        Task<OfficeClosureCases> GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate);
    }
}
