using System;
using Pipeline.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipeline.API.Contracts.RepositoryInterfaces
{
    public interface ICortexRepository
    {
        Task<IEnumerable<Opportunity>> GetTeamSizeFromCortex(string cortexId);

    }
}
