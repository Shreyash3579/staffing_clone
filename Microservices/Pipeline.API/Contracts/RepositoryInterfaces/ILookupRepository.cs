using Pipeline.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pipeline.API.Contracts.RepositoryInterfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<OpportunityStatusType>> GetOpportunityStatusTypeList();
    }
}
