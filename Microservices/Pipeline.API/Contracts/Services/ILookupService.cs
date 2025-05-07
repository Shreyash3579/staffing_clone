using Pipeline.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pipeline.API.Contracts.Services
{
    public interface ILookupService
    {
        Task<IEnumerable<OpportunityStatusType>> GetOpportunityStatusTypeList();
    }
}
