using Staffing.HttpAggregator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IIrisApiClient
    {
        Task<IEnumerable<PracticeArea>> GetAllIndustryPracticeArea();
        Task<IEnumerable<PracticeArea>> GetAllCapabilityPracticeArea();
    }
}
