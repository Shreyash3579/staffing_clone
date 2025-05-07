using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class OfficeClosureCasesRepository : IOfficeClosureCasesRepository
    {
        private readonly IBaseRepository<OfficeClosureCases> _baseRepository;

        public OfficeClosureCasesRepository(IBaseRepository<OfficeClosureCases> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<OfficeClosureCases> UpsertOfficeClosureCases(OfficeClosureCases officeClosureCases)
        {
            var upsertedData = await
                _baseRepository.UpdateAsync(new
                {
                    officeClosureCases.OfficeCode,
                    officeClosureCases.CaseTypeCodes,
                    officeClosureCases.OldCaseCodes,
                    officeClosureCases.OfficeClosureStartDate,
                    officeClosureCases.OfficeClosureEndDate,
                    officeClosureCases.LastUpdatedBy
                }, StoredProcedureMap.UpsertOfficeClosureCases);

            return upsertedData;
        }

        public async Task<OfficeClosureCases> GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate)
        {
            var officeClosureChanges = await
                    _baseRepository.GetByDynamicAsync(
                        new { officeCode = officeCodes, startDate, endDate },
                        StoredProcedureMap.GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType
                        );

            return officeClosureChanges;
        }
    }
}
