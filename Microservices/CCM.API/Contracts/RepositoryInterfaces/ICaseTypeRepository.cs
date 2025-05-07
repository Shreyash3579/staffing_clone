using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Contracts.RepositoryInterfaces
{
    public interface ICaseTypeRepository
    {
        Task<IEnumerable<CaseType>> GetCaseTypeList();
    }
}
