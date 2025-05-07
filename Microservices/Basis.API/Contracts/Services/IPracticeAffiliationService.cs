using Basis.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basis.API.Contracts.Services
{
    public interface IPracticeAffiliationService
    {
        Task<IEnumerable<PracticeAffiliation>> GetAllPracticeAffiliation();
    }
}
