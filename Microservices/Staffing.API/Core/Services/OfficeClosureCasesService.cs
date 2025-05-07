using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class OfficeClosureCasesService : IOfficeClosureCasesService
    {
        private readonly IOfficeClosureCasesRepository _officeClosureCasesRepository;

        public OfficeClosureCasesService(IOfficeClosureCasesRepository officeClosureCasesRepository)
        {
            _officeClosureCasesRepository = officeClosureCasesRepository;
        }

        public async Task<OfficeClosureCases> UpsertOfficeClosureCases(OfficeClosureCases officeClosureCases)
        {
            if (officeClosureCases.OfficeCode == 0
                || string.IsNullOrEmpty(officeClosureCases.CaseTypeCodes)
                || officeClosureCases.OfficeClosureStartDate == DateTime.MinValue
                || officeClosureCases.OfficeClosureEndDate == DateTime.MinValue
                || string.IsNullOrEmpty(officeClosureCases.LastUpdatedBy))
            {
                return new OfficeClosureCases();
            }
            return await _officeClosureCasesRepository.UpsertOfficeClosureCases(officeClosureCases);
        }

        public async Task<OfficeClosureCases> GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(officeCodes) || startDate == DateTime.MinValue || endDate == DateTime.MinValue || string.IsNullOrEmpty(caseTypeCodes))
            {
                return new OfficeClosureCases();
            }
            return await _officeClosureCasesRepository.GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(officeCodes, caseTypeCodes, startDate, endDate);
        }
    }
}
