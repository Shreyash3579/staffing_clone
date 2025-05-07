using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Core.Helpers;
using Basis.API.Models;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Core.Repository
{
    public class PracticeAreaRepository : IPracticeAreaRepository
    {
        private readonly IBaseRepository<PracticeAreaAffiliation> _baseRepository;

        public PracticeAreaRepository(IBaseRepository<PracticeAreaAffiliation> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<PracticeAreaAffiliation>> GetAllPracticeArea()
        {
            return await _baseRepository.GetAllAsync(null, StoredProcedureMap.GetAllPracticeArea);
        }

        public async Task<IEnumerable<PracticeAreaAffiliation>> GetIndustryPracticeAreaLookupList()
        {
            return await _baseRepository.GetAllAsync(null, StoredProcedureMap.GetIndustryPracticeAreaLookupList);
        }

        public async Task<IEnumerable<PracticeAreaAffiliation>> GetCapabilityPracticeAreaLookupList()
        {
            return await _baseRepository.GetAllAsync(null, StoredProcedureMap.GetCapabilityPracticeAreaLookupList);
        }

        public async Task<IEnumerable<PracticeAreaAffiliation>> GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(string employeeCodes, string practiceAreaCodes, string affiliationRoleCodes="")
        {
            return await _baseRepository.GetAllAsync(new { employeeCodes, practiceAreaCodes, affiliationRoleCodes }, StoredProcedureMap.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes);
        }

        public async Task<IEnumerable<AffiliationRole>> GetAffiliationRoleList()
        {
            var affiliationRoles = await Task.Run(() => _baseRepository.Context.Connection.Query<AffiliationRole>(
                StoredProcedureMap.GetAffiliationRoleList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return affiliationRoles;

        }

    }
}
