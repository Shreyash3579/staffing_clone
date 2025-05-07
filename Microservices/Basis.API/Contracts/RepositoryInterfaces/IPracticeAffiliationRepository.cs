using Basis.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Contracts.RepositoryInterfaces
{
    public interface IPracticeAffiliationRepository
    {
        Task<IEnumerable<PracticeAffiliation>> GetAllPracticeAffiliation();
    }
}
