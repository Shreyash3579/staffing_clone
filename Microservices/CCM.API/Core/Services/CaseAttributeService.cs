using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Contracts.Services;
using CCM.API.Models;
using CCM.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCM.API.Core.Services
{
    public class CaseAttributeService :ICaseAttributeService
    {
        private readonly ICaseAttributeRepository _caseAttributeRepository;
        public CaseAttributeService(ICaseAttributeRepository caseAttributeRepository)
        {
            _caseAttributeRepository = caseAttributeRepository;
        }

        private IEnumerable<CaseAttributeModel> ConvertToCaseAttributeData(IEnumerable<CaseAttributeModel> caseattributesbylastupdateddate)
        {
            return caseattributesbylastupdateddate.Select(x => new CaseAttributeModel
            {
                clientCode = x.clientCode,
                caseCode = x.caseCode,
                oldCaseCode = x.oldCaseCode,
                caseAttributeCode = x.caseAttributeCode,
                caseAttributeName = x.caseAttributeName,
                lastUpdated = x.lastUpdated,
                lastUpdatedBy = x.lastUpdatedBy
            });
        }
        public async Task<IEnumerable<CaseAttributeModel>> GetCaseAttributesByLastUpdatedDate(DateTime? lastUpdatedDate)
        {
            var caseattrubutesbylastupdateddate = await _caseAttributeRepository.GetCaseAttributesByLastUpdatedDate(lastUpdatedDate);
            return ConvertToCaseAttributeData(caseattrubutesbylastupdateddate);
        }
    }
}
