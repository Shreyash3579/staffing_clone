using Hcpd.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hcpd.API.Contracts.RepositoryInterfaces
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetReviewsByEmployeeCode(string employeeCode);
    }
}
