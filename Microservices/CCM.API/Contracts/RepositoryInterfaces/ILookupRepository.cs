using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Contracts.RepositoryInterfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<CaseAttribute>> GetCaseAttributeLookupList();
    }
}