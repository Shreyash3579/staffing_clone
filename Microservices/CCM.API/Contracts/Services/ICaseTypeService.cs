using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Contracts.Services
{
    public interface ICaseTypeService
    {
        Task<IEnumerable<CaseType>> GetCaseTypeList();
    }
}
