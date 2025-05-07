using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Contracts.Services;
using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Core.Services
{
    public class CaseTypeService : ICaseTypeService
    {
        private readonly ICaseTypeRepository _caseTypeRepository;

        public CaseTypeService(ICaseTypeRepository caseTypeRepository)
        {
            _caseTypeRepository = caseTypeRepository;
        }

        /// <summary>
        ///     This method returns the list of different case type 
        /// </summary>
        /// <returns>Case types like billiable, non-billable etc</returns>
        public async Task<IEnumerable<CaseType>> GetCaseTypeList()
        {
            var caseTypes = await
                _caseTypeRepository.GetCaseTypeList();

            return caseTypes;
        }
    }
}
