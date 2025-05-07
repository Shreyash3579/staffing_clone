using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Core.Helpers;
using CCM.API.Models;
using CCM.API.ViewModels;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CCM.API.Core.Repository
{
    public class CaseAttributeRepository : ICaseAttributeRepository
    {
        private readonly IBaseRepository<CaseAttributeModel> _baseRepository;
        public CaseAttributeRepository(IBaseRepository<CaseAttributeModel> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<CaseAttributeModel>> GetCaseAttributesByLastUpdatedDate(DateTime? lastupdated)
        {
            return await _baseRepository.GetAllAsync(new { lastupdated }, StoredProcedureMap.GetCaseAttributeByLastUpdatedDate);
        }
    }
}
