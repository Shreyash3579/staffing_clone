using CCM.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCM.API.Contracts.RepositoryInterfaces
{
    public interface ICaseAttributeRepository
    {
        Task<IEnumerable<CaseAttributeModel>> GetCaseAttributesByLastUpdatedDate(DateTime? lastUpdatedDate);
    }
}
